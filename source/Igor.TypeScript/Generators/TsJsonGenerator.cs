using Igor.Text;
using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using System.Linq;

namespace Igor.TypeScript.Json
{
    internal class TsJsonGenerator : ITsGenerator
    {
        public void Generate(TsModel model, Module mod)
        {
            var ts = model.FileOf(mod);

            foreach (var type in mod.Types)
            {
                switch (type)
                {
                    case EnumForm enumForm:
                        if (enumForm.tsJsonGenerateSerializer)
                            GenEnumSerializer(ts, enumForm);
                        break;

                    case RecordForm recordForm:
                        if (recordForm.tsJsonGenerateSerializer)
                            GenRecordSerializer(ts, recordForm);
                        break;

                    case VariantForm variantForm:
                        if (variantForm.tsJsonGenerateSerializer)
                            GenVariantSerializer(ts, variantForm);
                        break;
                }
            }
        }

        private void GenEnumSerializer(TsFile ts, EnumForm enumForm)
        {
            var tsTypeName = enumForm.tsType.relativeName(enumForm.tsNamespace);
            var c = ts.Namespace(enumForm.tsJsonGeneratedSerializerClass);

            c.Function(
$@"export function toJson(value: {tsTypeName}): Igor.Json.JsonValue {{
    return toJsonKey(value);
}}
");

            c.Function(
$@"export function fromJson(json: Igor.Json.JsonValue): {tsTypeName} {{
    return fromJsonKey(json);
}}
");

            var serFields = enumForm.Fields.JoinLines(f => $"        case {f.tsQualifiedName(enumForm.tsNamespace)}: return {f.tsJsonString}; ");

            c.Function(
$@"export function toJsonKey(value: {tsTypeName}): Igor.Json.JsonValue {{
    switch (value) {{
{serFields}
        default: throw new Error(`Invalid {tsTypeName} value: ${{value}}`);
    }}
}}
");

            var deserFields = enumForm.Fields.JoinLines(f => $"        case {f.tsJsonString}: return {f.tsQualifiedName(enumForm.tsNamespace)};");

            c.Function(
$@"export function fromJsonKey(json: Igor.Json.JsonValue): {tsTypeName} {{
    switch (json) {{
{deserFields}
        default: throw new Error(`Invalid {tsTypeName} value: ${{json}}`);
    }}
}}
");
        }

        private void GenRecordSerializer(TsFile ts, StructForm structForm)
        {
            var c = ts.Class(structForm.tsJsonGeneratedSerializerClass);
            var ns = structForm.tsNamespace;
            var tsTypeName = structForm.tsType.relativeName(ns);
            if (structForm.IsGeneric)
                c.GenericArgs = structForm.Args.Select(arg => arg.tsName).ToList();

            var setupCtor = structForm.tsSetupCtor;

            var genericArgWithTypes = structForm.Args.JoinStrings(", ", arg => $"{arg.tsVarName}: Igor.Json.IJsonSerializer<{arg.tsType.relativeName(structForm.tsNamespace)}>");
            var genericArgVars = structForm.Args.JoinStrings(", ", arg => arg.tsVarName);
            var genericArgs = structForm.IsGeneric ? "<" + structForm.Args.JoinStrings(", ", arg => arg.tsName) + ">" : "";
            var commaGenericArgVars = structForm.IsGeneric ? ", " + genericArgVars : "";
            var commaGenericArgWithTypes = structForm.IsGeneric ? ", " + genericArgWithTypes : "";

            var fields = structForm.jsonSerializedFields;
            var maybeOverride = structForm.Ancestor == null || !TsVersion.SupportsOverride ? "" : "override ";

            {
                // fromJson
                var r = new Renderer();
                r += $"static {maybeOverride}fromJson{genericArgs}(json: Igor.Json.JsonValue{commaGenericArgWithTypes}): {tsTypeName} {{";
                r++;
                if (fields.Any(f => !f.IsTag))
                    r += "const jsonObject = json as Igor.Json.JsonObject;";
                if (!setupCtor)
                    r += $"const obj = new {tsTypeName}();";
                foreach (var field in fields.Where(f => !f.IsTag))
                {
                    var fieldName = structForm.IsException && field.tsErrorMessage ? "message" : field.tsName;
                    var assignTo = setupCtor ? $"const {field.tsVarName}" : $"obj.{fieldName}";
                    if (structForm.IsPatch)
                    {
                        r += $"if (jsonObject['{field.jsonKey}'] !== undefined) {assignTo} = {field.tsType.fromJson($"jsonObject['{field.jsonKey}']", ns)};";
                    }
                    else if (field.IsOptional || field.HasDefault)
                    {
                        r += $"{assignTo} = ('{field.jsonKey}' in jsonObject && jsonObject['{field.jsonKey}'] != null) ? {field.tsType.nonOptType.fromJson($"jsonObject['{field.jsonKey}']", ns)} : {field.tsDefault};";
                    }
                    else
                    {
                        r += $"{assignTo} = {field.tsType.fromJson($"jsonObject['{field.jsonKey}']", ns)};";
                    }
                }
                if (setupCtor)
                    r += $"return new {tsTypeName}({fields.Where(f => !f.IsTag).JoinStrings(", ", f => f.tsVarName)});";
                else
                    r += "return obj;";
                r--;
                r += "}";
                c.Function(r.Build());
            }

            {
                // toJson
                var keepNulls = structForm.jsonNulls ?? false;
                var r = new Renderer();
                r += $"static {maybeOverride}toJson{genericArgs}(value: {tsTypeName}{commaGenericArgWithTypes}): Igor.Json.JsonValue {{";
                r++;
                r += "const result: Igor.Json.JsonObject = {};";
                foreach (var field in fields)
                {
                    var fieldName = structForm.IsException && field.tsErrorMessage ? "message" : field.tsName;
                    if (structForm.IsPatch)
                        r += $@"if (value.{fieldName} !== undefined) result['{field.jsonKey}'] = {field.tsType.toJson($"value.{fieldName}", ns)};";
                    else if (!field.IsOptional)
                        r += $@"result['{field.jsonKey}'] = {field.tsType.toJson($"value.{fieldName}", ns)};";
                    else if (keepNulls)
                        r += $@"result['{field.jsonKey}'] = (value.{fieldName} != null) ? {field.tsType.nonOptType.toJson($"value.{fieldName}", ns)} : null;";
                    else
                        r += $@"if (value.{fieldName} != null) result['{field.jsonKey}'] = {field.tsType.nonOptType.toJson($"value.{fieldName}", ns)};";
                }
                r += "return result;";
                r--;
                r += "}";
                c.Function(r.Build());
            }

            c.Function(
$@"
{maybeOverride}toJson({genericArgWithTypes}): Igor.Json.JsonValue {{
    return {structForm.tsName}.toJson{genericArgs}(this{commaGenericArgVars});
}}
");

            if (structForm.IsGeneric)
            {
                c.Function(
$@"
static instanceJsonSerializer{genericArgs}({genericArgWithTypes}): Igor.Json.IJsonSerializer<{tsTypeName}> {{
    return {{
        toJson(value: {tsTypeName}): Igor.Json.JsonValue {{
            return value.toJson({genericArgVars});
        }},

        fromJson(json: Igor.Json.JsonValue): {tsTypeName} {{
            return {structForm.tsName}.fromJson{genericArgs}(json, {genericArgVars});
        }}
    }};
}}");
            }
        }

        private void GenVariantSerializer(TsFile ts, VariantForm variantForm)
        {
            var c = ts.Class(variantForm.tsJsonGeneratedSerializerClass);
            var ns = variantForm.tsNamespace;
            var tsTypeName = variantForm.tsType.relativeName(ns);
            if (variantForm.IsGeneric)
                c.GenericArgs = variantForm.Args.Select(arg => arg.tsName).ToList();

            string JsonReadField(RecordField field)
            {
                if (field.HasDefault)
                {
                    return $"const {field.tsName} = ('{field.jsonKey}' in jsonObject && jsonObject['{field.jsonKey}'] != null) ? {field.tsType.fromJson($"jsonObject['{field.jsonKey}']", ns)} : {field.tsDefault};";
                }
                else
                {
                    return $"const {field.tsName} = {field.tsType.fromJson($"jsonObject['{field.jsonKey}']", ns)};";
                }
            }

            string ReadRecord(RecordForm record)
            {
                return $@"case {record.TagField.tsDefault}:
    return {record.tsType.fromJson("json", variantForm.tsNamespace)};";
            }

            var genericArgWithTypes = variantForm.Args.JoinStrings(", ", arg => $"{arg.tsVarName}: Igor.Json.IJsonSerializer<{arg.tsType.relativeName(variantForm.tsNamespace)}>");
            var genericArgVars = variantForm.Args.JoinStrings(", ", arg => arg.tsVarName);
            var genericArgs = variantForm.IsGeneric ? "<" + variantForm.Args.JoinStrings(", ", arg => arg.tsName) + ">" : "";
            var commaGenericArgWithTypes = variantForm.IsGeneric ? ", " + genericArgWithTypes : "";

            c.Function(
$@"
static fromJson{genericArgs}(json: Igor.Json.JsonValue{commaGenericArgWithTypes}): {tsTypeName} {{
    const jsonObject = json as Igor.Json.JsonObject;
    {JsonReadField(variantForm.TagField)}
    switch({variantForm.TagField.tsName}) {{
{variantForm.Records.JoinLines(ReadRecord).Indent(8)}
        default:
            throw new Error(`Invalid {variantForm.TagField.tsTypeName} value: ${{{variantForm.TagField.tsName}}}`);
    }}
}}
");
            c.Function(
$@"
static toJson{genericArgs}(value: {tsTypeName}{commaGenericArgWithTypes}): Igor.Json.JsonValue {{
    return value.toJson({genericArgVars});
}}
");
            c.Function(
$@"
abstract toJson({genericArgWithTypes}): Igor.Json.JsonValue;
");
        }
    }
}
