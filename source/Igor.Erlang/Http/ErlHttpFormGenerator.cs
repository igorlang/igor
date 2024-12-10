using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Http
{
    internal class ErlHttpFormGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);

            foreach (var type in mod.Types)
            {
                switch (type)
                {
                    case RecordForm recordForm:
                        if (recordForm.erlHttpFormGenSerializer)
                        {
                            GenRecordPacker(erl, recordForm);
                            GenRecordParser(erl, recordForm);
                        }
                        break;
                }
            }
        }

        private void GenRecordPacker(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlHttpFormGenPackerName, 1 + structForm.Arity, structForm);
            erl.IncludeLib("stdlib/include/assert.hrl");

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_json:pack_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";
            var r = new Renderer();

            var serFields = structForm.erlHttpFormSerializedFields;
            r += $@"-spec {structForm.erlHttpFormGenPackerName}({structForm.erlLocalType}{erlSpecArgs}) -> string().";
            r += null;

            if (serFields.Any())
            {
                r += $"{structForm.erlHttpFormGenPackerName}(Record{erlCommaArgs}) ->";
                r++;
                CodeUtils.DeconstructStruct(r, structType, serFields.Where(f => !f.IsTag), "Record");

                string Parameter(RecordField field)
                {
                    if (field.HasDefault)
                        return $"{{\"{field.httpFormName}\", {field.erlVarName}, {HttpSerialization.HttpQueryTag(field.Type, field, field).PackTag}, {Helper.ErlValue(field.Default, field.Type)}}}";
                    else
                        return $"{{\"{field.httpFormName}\", {field.erlVarName}, {HttpSerialization.HttpQueryTag(field.Type, field, field).PackTag}}}";
                }

                r += $"igor_http:compose_query([{serFields.JoinStrings(", ", Parameter)}]).";
            }
            else
            {
                r += $@"{structForm.erlHttpFormGenPackerName}({structType.Empty}{erlCommaArgs}) -> """".";
            }
            erl.Function(r.Build(), structForm);
        }

        private string ParseField(RecordField f)
        {
            if (f.IsOptional || f.HasDefault)
                return $"igor_http:parse_query(Form, {f.httpFormName}, {HttpSerialization.HttpQueryTag(f.Type, f, f).ParseTag}, {f.erlValue})";
            else
                return $"igor_http:parse_query(Form, {f.httpFormName}, {HttpSerialization.HttpQueryTag(f.Type, f, f).ParseTag})";
        }

        private void GenRecordParser(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlHttpFormGenParserName, 1 + structForm.Arity, structForm);

            var serFields = structForm.erlHttpFormSerializedFields.Where(f => !f.IsTag);
            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_json:parse_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";

            var r = new Renderer();

            r += $@"-spec {structForm.erlHttpFormGenParserName}(string(){erlSpecArgs}) -> {structForm.erlLocalType}.";
            r += null;
            if (serFields.Any())
            {
                r += $@"{structForm.erlHttpFormGenParserName}(Form{erlCommaArgs}) ->";
                r++;
                foreach (var field in serFields)
                    r += $"{field.erlVarName} = {ParseField(field)},";
                CodeUtils.ReturnStruct(r, structType, serFields, 3);
            }
            else
            {
                r += $@"{structForm.erlHttpFormGenParserName}(_Json{erlCommaArgs}) -> {structType.Empty}.";
            }

            erl.Function(r.Build(), structForm);
        }
    }
}
