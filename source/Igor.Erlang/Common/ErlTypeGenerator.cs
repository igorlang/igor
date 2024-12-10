using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;

namespace Igor.Erlang
{
    internal class ErlTypeGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);
            erl.Comment = mod.Annotation;

            foreach (var form in mod.Types)
            {
                if (form.erlEnabled && form.erlAlias == null)
                {
                    switch (form)
                    {
                        case EnumForm enumForm:
                            GenEnumType(erl, enumForm);
                            break;

                        case VariantForm variantForm:
                            GenVariantType(erl, variantForm);
                            break;

                        case RecordForm recordForm:
                            GenRecordType(model, erl, recordForm);
                            break;

                        case UnionForm unionForm:
                            GenUnionType(erl, unionForm);
                            break;

                        case InterfaceForm interfaceForm when interfaceForm.erlInterfaceRecords:
                            GenRecordType(model, erl, interfaceForm);
                            break;

                        case DefineForm defineForm:
                            GenDefineType(erl, defineForm);
                            break;
                    }
                }
            }
        }

        private void GenEnumType(ErlModule erl, EnumForm enumForm)
        {
            erl.ExportType(enumForm.erlLocalTypeName, 0);
            erl.Type(enumForm.erlLocalTypeName, enumForm.Fields.JoinStrings(" | ", f => Helper.QuotedAtomName(f.erlName)));

            if (enumForm.erlEnumToInteger)
            {
                erl.Export(enumForm.erlPackToIntName, 1, enumForm);
                var packToInt =
    $@"-spec {enumForm.erlPackToIntName}({enumForm.erlLocalType}) -> {enumForm.erlIntType}.

{enumForm.Fields.JoinStrings(";\n", field => $"{enumForm.erlPackToIntName}({field.erlName}) -> {field.Value}")}.
";
                erl.Function(packToInt, enumForm);

                erl.Export(enumForm.erlParseFromIntName, 1, enumForm);
                var parserFromInt =
     $@"-spec {enumForm.erlParseFromIntName}({enumForm.erlIntType}) -> {enumForm.erlLocalType}.

{enumForm.Fields.JoinStrings(";\n", field => $"{enumForm.erlParseFromIntName}({field.Value}) -> {field.erlName}")}.
";
                erl.Function(parserFromInt, enumForm);
            }
        }

        private void GenRecordType(ErlModel model, ErlModule erl, StructForm form)
        {
            var structType = form.erlStructType;
            if (structType.IsRecord)
            {
                var hrl = model.Header(form.Module.hrlFileName);
                var rec = hrl.Record(form.erlRecordName);
                rec.Comment = form.Annotation;
                foreach (var f in form.erlFields)
                {
                    var field = rec.Field(f.erlName);
                    if (f.erlGenTypeSpec)
                        field.TypeSpec = f.erlFieldType;
                    if (f.HasDefault || (f.erlRecordFieldErrors && !f.IsOptional))
                        field.Default = f.erlDefault;
                    field.Comment = f.Annotation;
                }
                erl.Include(form.Module.hrlFileName);
            }

            erl.ExportType(form.erlLocalTypeName, form.Arity);
            string typeSpec;
            bool shadowGenericArgs = false;
            if (structType.IsTuple)
            {
                typeSpec = $"{{{form.erlTupleTypeSpec}}}";
            }
            else if (structType.IsMap)
            {
                typeSpec = form.erlMapTypeSpec;
            }
            else if (form.erlGenDetailedTypeSpec)
            {
                typeSpec = $"#{form.erlRecordName}{{{form.erlDetailedTypeSpec}}}";
            }
            else
            {
                typeSpec = $"#{form.erlRecordName}{{}}";
                shadowGenericArgs = true;
            }
            erl.PolymorphicType(form.erlLocalTypeName, form.Args.JoinStrings(", ", arg => (shadowGenericArgs ? "_" : "") + arg.erlName), typeSpec);
        }

        private void GenVariantType(ErlModule erl, VariantForm variantForm)
        {
            erl.ExportType(variantForm.erlLocalTypeName, 0);
            erl.Type(variantForm.erlLocalTypeName, variantForm.Descendants.JoinStrings(" | ", d => d.erlRemoteType));
        }

        private void GenUnionType(ErlModule erl, UnionForm unionForm)
        {
            erl.ExportType(unionForm.erlLocalTypeName, unionForm.Arity);
            erl.PolymorphicType(unionForm.erlLocalTypeName, unionForm.erlArgs, unionForm.Clauses.JoinStrings(" | ", f => f.erlType));
        }

        private void GenDefineType(ErlModule erl, DefineForm defineForm)
        {
            erl.ExportType(defineForm.erlLocalTypeName, defineForm.Arity);
            erl.PolymorphicType(defineForm.erlLocalTypeName, defineForm.erlArgs, Helper.ErlType(defineForm.Type, false));
        }
    }
}
