using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Json
{
    internal class ErlJsonGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);

            foreach (var type in mod.Types)
            {
                if (type.erlJsonIsSerializerGenerated)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumPacker(erl, enumForm);
                            GenEnumParser(erl, enumForm);
                            break;

                        case VariantForm variantForm:
                            GenVariantPacker(erl, variantForm);
                            GenVariantParser(erl, variantForm);
                            break;

                        case RecordForm recordForm:
                            GenRecordPacker(erl, recordForm);
                            GenRecordParser(erl, recordForm);
                            break;

                        case InterfaceForm interfaceForm when interfaceForm.erlInterfaceRecords:
                            GenRecordPacker(erl, interfaceForm);
                            GenRecordParser(erl, interfaceForm);
                            break;

                        case UnionForm unionForm:
                            GenUnionPacker(erl, unionForm);
                            GenUnionParser(erl, unionForm);
                            break;
                    }
                }
            }
        }

        private void GenEnumPacker(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlJsonGenPackerName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlJsonGenPackerName}({enumForm.erlLocalType}) -> igor_json:json_string().

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlJsonGenPackerName}({field.erlName}) -> <<""{field.jsonKey}"">>")}.
", enumForm);
        }

        private void GenEnumParser(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlJsonGenParserName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlJsonGenParserName}(igor_json:json()) -> {enumForm.erlLocalType}.

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlJsonGenParserName}(<<""{field.jsonKey}"">>) -> {field.erlName}")}.
", enumForm);
        }

        private void GenRecordPacker(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlJsonGenPackerName, 1 + structForm.Arity, structForm);
            erl.IncludeLib("stdlib/include/assert.hrl");

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_json:pack_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";
            var r = new Renderer();

            var serFields = structForm.jsonSerializedFields;
            r += $@"-spec {structForm.erlJsonGenPackerName}({structForm.erlLocalType}{erlSpecArgs}) -> igor_json:json_object().";
            r += null;

            string PackField(RecordField field)
            {
                if (field.IsTag)
                    return $"{field.erlJsonName} => igor_json:pack({field.erlValue}, {field.erlJsonTag.PackTag})";
                else
                    return $"{field.erlJsonName} => igor_json:pack({field.erlVarName}, {field.erlJsonTag.PackTag})";
            }

            if (serFields.Any(f => !f.IsTag))
            {
                r += $"{structForm.erlJsonGenPackerName}(Record{erlCommaArgs}) ->";
                r++;
                CodeUtils.DeconstructStruct(r, structType, serFields.Where(f => !f.IsTag), "Record");
                if (structForm.IsPatch || !(structForm.jsonNulls ?? true))
                {
                    var jsoni = 0;
                    foreach (var field in serFields)
                    {
                        var jsonVar = jsoni == 0 ? "#{}" : $"Json{jsoni}";
                        var value = field.IsTag ? field.erlDefault : field.erlVarName;
                        r += $"Json{jsoni + 1} = igor_json:maybe_pack_field({jsonVar}, {field.erlJsonName}, {value}, {field.erlJsonTag.PackTag}),";
                        jsoni++;
                    }
                    r += $"Json{jsoni}.";
                }
                else
                {
                    r += "#{";
                    r++;
                    r.Blocks(serFields.Select(PackField), delimiter: ",");
                    r--;
                    r += "}.";
                }
            }
            else if (serFields.Any())
            {
                r += $"{structForm.erlJsonGenPackerName}({structType.Empty}{erlCommaArgs}) ->";
                r++;
                r += $"#{{{PackField(serFields.First())}}}.";
            }
            else
            {
                r += $"{structForm.erlJsonGenPackerName}({structType.Empty}{erlCommaArgs}) -> #{{}}.";
            }
            erl.Function(r.Build(), structForm);
        }

        private string ParseField(RecordField f)
        {
            if (f.IsOptional || f.HasDefault)
                return $"igor_json:parse(Json, {f.erlJsonName}, {f.erlJsonTag.ParseTag}, {f.erlValue})";
            else
                return $"igor_json:parse(Json, {f.erlJsonName}, {f.erlJsonTag.ParseTag})";
        }

        private void GenRecordParser(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlJsonGenParserName, 1 + structForm.Arity, structForm);

            var serFields = structForm.jsonSerializedFields.Where(f => !f.IsTag);
            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_json:parse_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";

            var r = new Renderer();

            r += $@"-spec {structForm.erlJsonGenParserName}(igor_json:json_object(){erlSpecArgs}) -> {structForm.erlLocalType}.";
            r.EmptyLine();
            if (serFields.Any())
            {
                r += $@"{structForm.erlJsonGenParserName}(Json{erlCommaArgs}) ->";
                r++;
                r.Blocks(serFields, field => $"{field.erlVarName} = {ParseField(field)},");
                CodeUtils.ReturnStruct(r, structType, serFields, 3);
            }
            else
            {
                r += $@"{structForm.erlJsonGenParserName}(_Json{erlCommaArgs}) -> {structType.Empty}.";
            }

            erl.Function(r.Build(), structForm);
        }

        private void GenVariantPacker(ErlModule erl, VariantForm variantForm)
        {
            string erlJsonPackVariantClause(RecordForm recordForm, string funName) =>
     $@"{funName}(Record) when {recordForm.erlGuard("Record")} ->
    {recordForm.erlJsonTag(variantForm).PackJson("Record", variantForm.Module.erlName)}";

            erl.Export(variantForm.erlJsonGenPackerName, 1 + variantForm.Arity, variantForm);
            erl.IncludeLib("stdlib/include/assert.hrl");

            erl.Function(
$@"-spec {variantForm.erlJsonGenPackerName}({variantForm.erlLocalType}) -> igor_json:json_object().

{variantForm.Records.JoinStrings(";\n", r => erlJsonPackVariantClause(r, variantForm.erlJsonGenPackerName))}.
", variantForm);
        }

        private void GenVariantParser(ErlModule erl, VariantForm variantForm)
        {
            string erlJsonParseVariantClause(RecordForm recordForm) =>
$@"        {recordForm.TagField.erlValue} ->
            {JsonSerialization.ParseJson(recordForm.erlJsonTag(variantForm), "Json", variantForm.Module.erlName)}";

            erl.Export(variantForm.erlJsonGenParserName, 1 + variantForm.Arity, variantForm);

            erl.Function(
$@"-spec {variantForm.erlJsonGenParserName}(igor_json:json_object()) -> {variantForm.erlLocalType}.

{variantForm.erlJsonGenParserName}(Json) ->
    Tag = {ParseField(variantForm.TagField)},
    case Tag of
{variantForm.Records.JoinStrings(";\n", erlJsonParseVariantClause)}
    end.
", variantForm);
        }

        private void GenUnionPacker(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_json:pack_type({arg.erlName})")}" : "";

            string UnionClause(UnionClause clause)
            {
                if (clause.IsSingleton)
                    return $@"{unionForm.erlJsonGenPackerName}({clause.erlTag}{clause.erlGenericArgs}) ->
    <<""{clause.jsonKey}"">>";
                else if (clause.erlTagged)
                    return $@"{unionForm.erlJsonGenPackerName}({{{clause.erlTag}, Value}}{clause.erlGenericArgs}) ->
    #{{<<""{clause.jsonKey}"">> => {JsonSerialization.JsonTag(clause.Type, unionForm).PackJson("Value", erl.Name)}}}";
                else
                    return $@"{unionForm.erlJsonGenPackerName}(Value{clause.erlGenericArgs}) when {clause.erlGuard("Value")} ->
    #{{<<""{clause.jsonKey}"">> => {JsonSerialization.JsonTag(clause.Type, unionForm).PackJson("Value", erl.Name)}}}";
            }

            erl.Export(unionForm.erlJsonGenPackerName, 1 + unionForm.Arity, unionForm);

            erl.Function(
$@"-spec {unionForm.erlJsonGenPackerName}({unionForm.erlLocalType}{erlSpecArgs}) -> igor_json:json().

{unionForm.Clauses.JoinStrings(";\n", UnionClause)}.", unionForm);
        }

        private void GenUnionParser(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_json:parse_type({arg.erlName})")}" : "";

            string UnionClause(UnionClause clause)
            {
                if (clause.IsSingleton)
                    return $@"{unionForm.erlJsonGenParserName}(<<""{clause.jsonKey}"">>{clause.erlGenericArgs}) ->
    {clause.erlTag}";
                else if (clause.erlTagged)
                    return $@"{unionForm.erlJsonGenParserName}(#{{<<""{clause.jsonKey}"">> := JsonValue}}{clause.erlGenericArgs}) ->
    {{{clause.erlTag}, {JsonSerialization.ParseJson(JsonSerialization.JsonTag(clause.Type, unionForm), "JsonValue", erl.Name)}}}";
                else
                    return $@"{unionForm.erlJsonGenParserName}(#{{<<""{clause.jsonKey}"">> := JsonValue}}{clause.erlGenericArgs}) ->
    {JsonSerialization.ParseJson(JsonSerialization.JsonTag(clause.Type, unionForm), "JsonValue", erl.Name)}";
            }
            erl.Export(unionForm.erlJsonGenParserName, 1 + unionForm.Arity, unionForm);

            erl.Function(
$@"-spec {unionForm.erlJsonGenParserName}(igor_json:json(){erlSpecArgs}) -> {unionForm.erlLocalType}.

{unionForm.Clauses.JoinStrings(";\n", UnionClause)}.", unionForm);
        }
    }
}
