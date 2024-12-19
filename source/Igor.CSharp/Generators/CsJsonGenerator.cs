using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsJsonGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var type in mod.Types)
            {
                if (type.jsonEnabled && type.csEnabled)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            if (enumForm.csJsonGenerateSerializer)
                                GenerateEnum(model, enumForm);
                            break;
                        case RecordForm recordForm:
                            if (recordForm.csGenerateDeclaration && recordForm.csJsonSerializable)
                                GenerateJsonSerializable(model, recordForm);
                            if (recordForm.csJsonGenerateSerializer)
                                GenerateJsonStruct(model, recordForm);
                            break;
                        case VariantForm variantForm:
                            if (variantForm.csGenerateDeclaration && variantForm.csJsonSerializable)
                                GenerateJsonSerializable(model, variantForm);
                            if (variantForm.csJsonGenerateSerializer)
                            {
                                GenerateJsonStruct(model, variantForm);
                                GenerateJsonVariant(model, variantForm);
                            }
                            break;
                    }
                }
            }
        }

        public void GenerateEnum(CsModel model, EnumForm enumForm)
        {
            var file = model.FileOf(enumForm);
            file.UseAlias("JsonSerializer", "Json.Serialization.JsonSerializer");
            var csTypeName = enumForm.csType.relativeName(enumForm.csJsonNamespace);
            var c = file.Namespace(enumForm.csJsonNamespace).Class(enumForm.csJsonGeneratedSerializerClass);
            c.Sealed = true;
            c.Interface($"Json.Serialization.IJsonSerializer<{csTypeName}>");
            c.Interface($"Json.Serialization.IJsonKeySerializer<{csTypeName}>");

            c.Method(
$@"public Json.ImmutableJson Serialize({csTypeName} value)
{{
    return SerializeKey(value);
}}
");

            c.Method(
$@"public {csTypeName} Deserialize(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    return DeserializeKey(json.AsString);
}}
");

            if (enumForm.csJsonTestEnabled)
                c.Method(
$@"public bool Test(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    return json.IsString;
}}
");

            var serializedFields = enumForm.Fields.Select(f => (f.csQualifiedName(enumForm.csJsonNamespace), f.csJsonString));
            c.Method(
$@"public string SerializeKey({csTypeName} value)
{{
{CsSyntaxUtils.ReturnSwitch("value", serializedFields, $"throw new System.ArgumentOutOfRangeException({CsVersion.NameOf("value")})").Indent(4)}
}}
");

            var deserializedFields = enumForm.Fields.Select(f => (f.csJsonString, f.csQualifiedName(enumForm.csJsonNamespace)));
            c.Method(
$@"public {csTypeName} DeserializeKey(string jsonKey)
{{
{CsSyntaxUtils.ReturnSwitch("jsonKey", deserializedFields, $"throw new System.ArgumentOutOfRangeException({CsVersion.NameOf("jsonKey")})").Indent(4)}
}}
");

            c.Property("Instance",
$@"public static readonly {enumForm.csJsonGeneratedSerializerClass} Instance = new {enumForm.csJsonGeneratedSerializerClass}();
");
        }

        public void GenerateJsonSerializable(CsModel model, StructForm structForm)
        {
            var c = model.TypeOf(structForm);
            c.Interface("Json.Serialization.IJsonSerializable");
            c.Method(
$@"public Json.ImmutableJson SerializeJson()
{{
    return {structForm.csJsonSerializerInstance(structForm.csNamespace)}.Serialize(this);
}}
");
            c.Method(
$@"public void DeserializeJson(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    {structForm.csJsonSerializerInstance(structForm.csNamespace)}.Deserialize(json.AsObject, this);
}}
");
        }

        public void GenerateJsonStruct(CsModel model, StructForm structForm)
        {
            var file = model.FileOf(structForm);
            file.UseAlias("JsonSerializer", "Json.Serialization.JsonSerializer");
            var c = file.Namespace(structForm.csJsonNamespace).Class(structForm.csJsonGeneratedSerializerClass);
            c.Sealed = true;
            var csTypeName = structForm.csType.relativeName(structForm.csJsonNamespace);
            if (structForm.IsGeneric)
                c.GenericArgs = structForm.Args.Select(arg => arg.csName).ToList();
            c.Interface($"Json.Serialization.IJsonSerializer<{csTypeName}>");

            if (!(structForm is VariantForm))
            {
                string JsonWrite(RecordField f)
                {
                    if (f.IsOptional)
                        return
$@"    if (value.{f.csNotNull})
        json[""{f.jsonKey}""] = {f.csJsonSerializer}.Serialize(value.{f.csJsonWriteValue});";
                    else
                        return
$@"    json[""{f.jsonKey}""] = {f.csJsonSerializer}.Serialize(value.{f.csName});";
                }

                var hasOptionalFields = structForm.jsonSerializedFields.Any(f => f.IsOptional);
                var jsonResult = hasOptionalFields || Context.Instance.TargetVersion < CsVersion.Version60 ?
                    $@"    var json = new Json.JsonObject();
{structForm.jsonSerializedFields.JoinLines(JsonWrite)}
    return json;"
                    :
$@"    return new Json.JsonObject
    {{
{structForm.jsonSerializedFields.JoinStrings(",\n", f => $@"        [""{f.jsonKey}""] = {f.csJsonSerializer}.Serialize(value.{f.csName})")}
    }};";
                c.Method(
$@"
public Json.ImmutableJson Serialize({csTypeName} value)
{{
{structForm.csRequire("value")}
{structForm.jsonSerializedFields.JoinLines(f => f.csJsonRequire("value"))}
{jsonResult}
}}
");

                string csJsonRead(RecordField field, string varName, bool initDefault, bool defineVar = false)
                {
                    var r = new Renderer();
                    if (field.IsOptional || field.HasDefault)
                    {
                        if (defineVar)
                            r += $"{field.csTypeName} {varName};";
                        string maybeVar = "var ";
                        if (!CsVersion.SupportsOutVar)
                        {
                            r += $"Json.ImmutableJson json{field.csName};";
                            maybeVar = "";
                        }
                        r +=
$@"if (json.AsObject.TryGetValue(""{field.jsonKey}"", out {maybeVar} json{field.csName}) && !json{field.csName}.IsNull)
    {varName} = {field.csJsonSerializer}.Deserialize(json{field.csName});";
                        if (initDefault)
                            r += $@"else
    {varName} = {field.csDefault ?? "null"};";
                    }
                    else
                    {
                        var maybeDefineVar = defineVar ? "var " : "";
                        r += $@"{maybeDefineVar}{varName} = {field.csJsonSerializer}.Deserialize(json[""{field.jsonKey}""]);";
                    }
                    return r.Build().TrimEnd();
                }

                if (structForm.csSetupCtor)
                {
                    c.Method($@"
public {csTypeName} Deserialize(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
{structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinLines(f => csJsonRead(f, f.csVarName, true, true)).Indent(4)}
    return new {csTypeName}({structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinStrings(", ", f => f.csVarName)});
}}
");
                }
                else
                {
                    var refKeyword = structForm.csReference ? "" : "ref";

                    c.Method(
$@"
public {csTypeName} Deserialize(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    var result = new {csTypeName}();
    Deserialize(json, {refKeyword} result);
    return result;
}}
");

                    c.Method(
    $@"
public void Deserialize(Json.ImmutableJson json, {refKeyword} {csTypeName} value)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
{structForm.csRequire("value")}
{structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinLines(f => csJsonRead(f, $"value.{f.csName}", false)).Indent(4)}
}}
");
                }

                if (structForm.csJsonTestEnabled)
                    c.Method(
    $@"public bool Test(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    return json.IsObject;
}}
");
            }

            if (structForm.IsGeneric)
            {
                foreach (var arg in structForm.Args)
                {
                    c.Property(arg.csVarName, $"{arg.csJsonSerializerType} {arg.csVarName};");
                }
                var ctorArgs = structForm.Args.JoinStrings(", ", arg => $"{arg.csJsonSerializerType} {arg.csVarName}");
                var ctorFields = structForm.Args.JoinLines(arg => $"    this.{arg.csVarName} = {arg.csVarName};");
                c.Constructor(
$@"public {structForm.csJsonGeneratedSerializerClass}({ctorArgs})
{{
{ctorFields}
}}
");
            }
            else
            {
                c.Property("Instance",
$@"public static readonly {structForm.csJsonGeneratedSerializerClass} Instance = new {structForm.csJsonGeneratedSerializerClass}();
");
            }
        }

        public void GenerateJsonVariant(CsModel model, VariantForm variantForm)
        {
            var c = model.FileOf(variantForm).Namespace(variantForm.csJsonNamespace).Class(variantForm.csJsonGeneratedSerializerClass);

            var deserializeDescendants = variantForm.Records.Select(rec => (rec.TagField.csDefault, $"{rec.csJsonSerializerInstance(variantForm.csJsonNamespace)}.Deserialize(json)"));

            var serializeDescendants = variantForm.Records.Select(rec => (rec.TagField.csDefault, $"{rec.csJsonSerializerInstance(variantForm.csJsonNamespace)}.Serialize(({rec.csType.relativeName(variantForm.csJsonNamespace)})value)"));

            c.Method(
    $@"public Json.ImmutableJson Serialize({variantForm.csName} value)
{{
{variantForm.csRequire("value")}
{CsSyntaxUtils.ReturnSwitch($"value.{variantForm.TagField.csName}", serializeDescendants, @"throw new System.ArgumentException(""Invalid variant tag"")").Indent(4)}
}}
");

            c.Method(
            $@"public {variantForm.csName} Deserialize(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    {variantForm.TagField.csTypeName} {variantForm.TagField.csVarName} = {variantForm.TagField.csJsonSerializer}.Deserialize(json[""{variantForm.TagField.jsonKey}""]);
{CsSyntaxUtils.ReturnSwitch(variantForm.TagField.csVarName, deserializeDescendants, @"throw new System.ArgumentException(""Invalid variant tag"")").Indent(4)}
}}
");

            if (variantForm.csJsonTestEnabled)
                c.Method(
$@"public bool Test(Json.ImmutableJson json)
{{
    if (json == null)
        throw new System.ArgumentNullException({CsVersion.NameOf("json")});
    return json.IsObject && json.AsObject.ContainsKey(""{variantForm.TagField.jsonKey}"");
}}
");
        }
    }
}