using System.Linq;
using Igor.Elixir.AST;
using Igor.Elixir.Model;
using Igor.Elixir.Render;
using Igor.Text;

namespace Igor.Elixir.Generators
{
    class ExTypeGenerator : IElixirGenerator
    {
        public void Generate(ExModel model, Module mod)
        {
            var module = model.ModuleOf(mod);
            module.Annotation = mod.Annotation;

            foreach (var form in mod.Types)
            {
                if (form.exEnabled)
                {
                    switch (form)
                    {
                        case EnumForm enumForm:
                            GenEnum(module, enumForm);
                            break;

                        case VariantForm variantForm:
                            GenVariant(module, variantForm);
                            break;

                        case RecordForm recordForm:
                            GenRecord(module, recordForm);
                            break;

                        case UnionForm unionForm:
                            GenUnion(module, unionForm);
                            break;

                        case DefineForm defineForm:
                            GenDefine(module, defineForm);
                            break;
                    }
                }
            }
        }

        private void GenEnum(ExModule protocolModule, EnumForm enumForm)
        {
            var ex = protocolModule.Module(enumForm.exName);
            if (enumForm.Annotation != null)
                ex.Annotation = enumForm.Annotation;
            ex.Type(enumForm.exLocalTypeName, enumForm.Fields.JoinStrings(" | ", f => f.exName));
            ex.Function($"defguard {enumForm.exGuardName}(value) when {enumForm.Fields.JoinStrings(" or ", f => $"value === {f.exName}")}");

            /*
            if (enumForm.exEnumToInteger)
            {
                ex.Export(enumForm.erlPackToIntName, 1, enumForm);
                var packToInt =
    $@"-spec {enumForm.erlPackToIntName}({enumForm.erlLocalType}) -> {enumForm.erlIntType}.

{enumForm.Fields.JoinStrings(";\n", field => $"{enumForm.erlPackToIntName}({field.erlName}) -> {field.Value}")}.
";
                ex.Function(packToInt, enumForm);

                ex.Export(enumForm.erlParseFromIntName, 1, enumForm);
                var parserFromInt =
     $@"-spec {enumForm.erlParseFromIntName}({enumForm.erlIntType}) -> {enumForm.erlLocalType}.

{enumForm.Fields.JoinStrings(";\n", field => $"{enumForm.erlParseFromIntName}({field.Value}) -> {field.erlName}")}.
";
                ex.Function(parserFromInt, enumForm);
            }*/
        }

        private void GenRecord(ExModule protocolModule, StructForm form)
        {
            var ex = protocolModule.Module(form.exName);
            if (form.Annotation != null)
                ex.Annotation = form.Annotation;

            string typeSpec;
            var structType = form.exStructType;
            if (structType.IsStruct)
            {
                var rec = ex.DefStruct();
                rec.IsException = form.IsException;
                ex.Annotation = form.Annotation;
                foreach (var f in form.exFields)
                {
                    var field = rec.Field(f.exName);
                    if (f.HasDefault)
                        field.Default = f.exDefault;
                    if (!f.IsOptional)
                        field.Enforce = true;
                    field.Comment = f.Annotation;
                }
                typeSpec = $"%{form.exName}{{{form.exFields.JoinStrings(", ", f => $"{f.exName}: {f.exType}")}}}";
            }
            else if (structType.IsTuple)
            {
                typeSpec = $"{{{form.exFields.JoinStrings(", ", f => f.exType)}}}";
            }
            else if (structType.IsMap)
            {
                typeSpec = $"%{{{form.exFields.JoinStrings(", ", f => f.exMapTypeSpec)}}}";
            }
            else // if (structType.IsRecord)
            {
                var details = form.exFields.Any() ? "\n" + form.exFields.JoinStrings(",\n", f => f.exDetailedTypeSpec).Indent(4) : "";
                typeSpec = $"#{form.exRecordName}{{{details}}}";
            }
            var genericArgs = form.Arity > 0 ? form.Args.JoinStrings(", ", arg => arg.exName).Quoted("(", ")") : "";
            ex.Type($"t{genericArgs}", typeSpec); // form.erlLocalTypeName, form.Args.JoinStrings(", ", arg => (shadowGenericArgs ? "_" : "") + arg.erlName), typeSpec);*/

            if (form.IsException && form.exFields.All(f => f.exName != "message"))
            {
                ex.Function($@"@spec message({form.exLocalType}) :: String.t()
def message(_), do: ""{form.exExceptionMessage}""");
            }
        }

        private void GenVariant(ExModule protocolModule, VariantForm variantForm)
        {
            var ex = protocolModule.Module(variantForm.exName);
            if (variantForm.Annotation != null)
                ex.Annotation = variantForm.Annotation;
            ex.Type("t", variantForm.Descendants.JoinStrings(" | ", d => d.exRemoteType));
        }

        private void GenUnion(ExModule protocolModule, UnionForm unionForm)
        {
            var ex = protocolModule.Module(unionForm.exName);
            if (unionForm.Annotation != null)
                ex.Annotation = unionForm.Annotation;
            ex.Type("t", unionForm.Clauses.JoinStrings(" | ", d => d.exType));
        }

        private void GenDefine(ExModule ex, DefineForm defineForm)
        {
            if (defineForm.exAlias == null)
            {
                var genericArgs = defineForm.Arity > 0 ? defineForm.Args.JoinStrings(", ", arg => arg.exName).Quoted("(", ")") : "";
                var targetType = Helper.ExType(defineForm.Type, false);
                ex.Type(defineForm.exLocalTypeName + genericArgs, targetType);
            }
        }
    }
}
