using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Binary
{
    internal class ErlBinaryGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);

            foreach (var type in mod.Types)
            {
                if (type.erlBinaryIsSerializerGenerated)
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
            erl.Export(enumForm.erlBinaryGenPackerName, 1, enumForm);

            var packToIodata =
$@"-spec {enumForm.erlBinaryGenPackerName}({enumForm.erlLocalType}) -> iodata().

{enumForm.erlBinaryGenPackerName}(Enum) ->
    igor_binary:pack_value({enumForm.erlPackToIntName}(Enum), {enumForm.erlIntTag}).
";
            erl.Function(packToIodata, enumForm);
        }

        private void GenEnumParser(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlBinaryGenParserName, 1, enumForm);

            var parser =
$@"-spec {enumForm.erlBinaryGenParserName}(binary()) -> {{{enumForm.erlLocalType}, binary()}}.

{enumForm.erlBinaryGenParserName}(Binary) ->
    {{Int, Tail}} = igor_binary:parse_value(Binary, {enumForm.erlIntTag}),
    {{{enumForm.erlParseFromIntName}(Int), Tail}}.
";

            erl.Function(parser, enumForm);
        }

        private void GenRecordPacker(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlBinaryGenPackerName, 1 + structForm.Arity, structForm);

            var serFields = structForm.binarySerializedFields;

            var stream = new ErlIgorWriter();
            if (structForm.binaryHeader)
            {
                foreach (var f in serFields.Where(f => f.IsOptional))
                    stream.Bit(f.erlVarName);
                stream.PadBits(structForm.erlBinaryBitPadding);
                foreach (var f in serFields)
                {
                    if (f.IsOptional)
                        stream.MaybePack(f.erlVarName, f.erlBinaryTag);
                    else
                        stream.Var(f.erlVarName, f.erlBinaryTag);
                }
            }
            else
            {
                foreach (var f in serFields)
                    stream.Var(f.erlVarName, f.erlBinaryTag);
            }

            erl.IncludeLib("stdlib/include/assert.hrl");
            if (stream.RequireIgorInclude())
                erl.IncludeLib("igor/include/igor_binary.hrl");

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_binary:pack_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";

            var r = new Renderer();
            r += $@"-spec {structForm.erlBinaryGenPackerName}({structForm.erlLocalType}{erlSpecArgs}) -> iodata().";
            r += null;

            if (serFields.Any())
            {
                r += $"{structForm.erlBinaryGenPackerName}(Record{erlCommaArgs}) ->";
                r++;
                CodeUtils.DeconstructStruct(r, structType, serFields, "Record");
                stream.Render(r, ".");
            }
            else
            {
                r += $"{structForm.erlBinaryGenPackerName}({structType.Empty}{erlCommaArgs}) -> <<>>.";
            }
            erl.Function(r.Build(), structForm);
        }

        private void GenRecordParser(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlBinaryGenParserName, 1 + structForm.Arity, structForm);

            var serFields = structForm.binarySerializedFields;

            var stream = new ErlIgorReader();
            if (structForm.binaryHeader)
            {
                foreach (var f in serFields.Where(f => f.IsOptional))
                    stream.Bit(f.erlVarName);
                stream.PadBits(structForm.erlBinaryBitPadding);
                foreach (var f in serFields)
                {
                    if (f.IsOptional)
                        stream.MaybeParseCall(f.erlVarName, f.erlBinaryTag);
                    else
                        stream.Var(f.erlVarName, f.erlBinaryTag);
                }
            }
            else
            {
                foreach (var f in serFields)
                    stream.Var(f.erlVarName, f.erlBinaryTag);
            }

            if (stream.RequireIgorInclude())
                erl.IncludeLib("igor/include/igor_binary.hrl");

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_binary:parse_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";

            var r = new Renderer();

            r += $@"-spec {structForm.erlBinaryGenParserName}(binary(){erlSpecArgs}) -> {{{structForm.erlLocalType}, binary()}}.";
            r += null;

            if (serFields.Any())
            {
                r += $@"{structForm.erlBinaryGenParserName}(Binary{erlCommaArgs}) ->";
                r++;
                stream.Render(r);
                CodeUtils.ConstructStructVar(r, structType, serFields, "Result", structType.IsTuple ? 100 : 3);
                r += "{Result, Tail}.";
            }
            else
            {
                r += $@"{structForm.erlBinaryGenParserName}(Binary{erlCommaArgs}) ->
    {{{structType.Empty}, Binary}}.";
            }

            erl.Function(r.Build(), structForm);
        }

        private void GenVariantPacker(ErlModule erl, VariantForm variantForm)
        {
            string PackVariantClause(RecordForm rec) =>
     $@"{variantForm.erlBinaryGenPackerName}(Record) when {rec.erlGuard("Record")} ->
    [
        igor_binary:pack_value({rec.TagField.erlValue}, {rec.TagField.erlBinaryTag.PackTag}),
        {rec.erlBinaryTag(variantForm).PackBinary("Record", rec.Module.erlName)}
    ]";

            erl.Export(variantForm.erlBinaryGenPackerName, 1 + variantForm.Arity, variantForm);

            erl.Function(
$@"-spec {variantForm.erlBinaryGenPackerName}({variantForm.erlLocalType}) -> iodata().

{variantForm.Records.JoinStrings(";\n", PackVariantClause)}.
", variantForm);
        }

        private void GenVariantParser(ErlModule erl, VariantForm variantForm)
        {
            string ParseVariantClause(RecordForm rec) =>
$@"        {rec.TagField.erlValue} ->
            {BinarySerialization.ParseBinary(rec.erlBinaryTag(variantForm), "Tail", rec.Module.erlName)}";

            erl.Export(variantForm.erlBinaryGenParserName, 1 + variantForm.Arity, variantForm);

            erl.Function(
$@"-spec {variantForm.erlBinaryGenParserName}(binary()) -> {{{variantForm.erlLocalType}, binary()}}.

{variantForm.erlBinaryGenParserName}(Binary) ->
    {{Tag, Tail}} = igor_binary:parse_value(Binary, {variantForm.TagField.erlBinaryTag.ParseTag}),
    case Tag of
{variantForm.Records.JoinStrings(";\n", ParseVariantClause)}
    end.
", variantForm);
        }

        private void GenUnionPacker(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_binary:pack_type({arg.erlName})")}" : "";

            string UnionClause(UnionClause clause, int tag)
            {
                if (clause.IsSingleton)
                    return $@"{unionForm.erlBinaryGenPackerName}({clause.erlTag}{clause.erlGenericArgs}) ->
    {tag}";
                else if (clause.erlTagged)
                    return $@"{unionForm.erlBinaryGenPackerName}({{{clause.erlTag}, Value}}{clause.erlGenericArgs}) ->
    [{tag}, {BinarySerialization.BinaryTag(clause.Type, unionForm).PackBinary("Value", erl.Name)}]";
                else
                    return $@"{unionForm.erlBinaryGenPackerName}(Value{clause.erlGenericArgs}) when {clause.erlGuard("Value")} ->
    [{tag}, {BinarySerialization.BinaryTag(clause.Type, unionForm).PackBinary("Value", erl.Name)}]";
            }

            erl.Export(unionForm.erlBinaryGenPackerName, 1 + unionForm.Arity, unionForm);

            erl.Function(
$@"-spec {unionForm.erlBinaryGenPackerName}({unionForm.erlLocalType}{erlSpecArgs}) -> iodata().

{unionForm.Clauses.SelectWithIndex(UnionClause).JoinStrings(";\n")}.", unionForm);
        }

        private void GenUnionParser(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_binary:parse_type({arg.erlName})")}" : "";

            string UnionClause(UnionClause clause, int tag)
            {
                if (clause.IsSingleton)
                    return $@"{unionForm.erlBinaryGenParserName}(<<?byte({tag}),Tail/binary>>{clause.erlGenericArgs}) ->
    {{{clause.erlTag}, Tail}}";
                else if (clause.erlTagged)
                    return $@"{unionForm.erlBinaryGenParserName}(<<?byte({tag}),Binary/binary>>{clause.erlGenericArgs}) ->
    {{Value, Tail}} = {BinarySerialization.ParseBinary(BinarySerialization.BinaryTag(clause.Type, unionForm), "Binary", erl.Name)},
    {{{{{clause.erlTag}, Value}}, Tail}}";
                else
                    return $@"{unionForm.erlBinaryGenParserName}(<<?byte({tag}),Binary/binary>>{clause.erlGenericArgs}) ->
    {BinarySerialization.ParseBinary(BinarySerialization.BinaryTag(clause.Type, unionForm), "Binary", erl.Name)}";
            }
            erl.Export(unionForm.erlBinaryGenParserName, 1 + unionForm.Arity, unionForm);
            erl.IncludeLib("igor/include/igor_binary.hrl");

            erl.Function(
$@"-spec {unionForm.erlBinaryGenParserName}(binary(){erlSpecArgs}) -> {{{unionForm.erlLocalType}, binary()}}.

{unionForm.Clauses.SelectWithIndex(UnionClause).JoinStrings(";\n")}.", unionForm);
        }
    }
}
