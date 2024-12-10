using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;

namespace Igor.Erlang.Strings
{
    internal class ErlStringGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);

            foreach (var type in mod.Types)
            {
                switch (type)
                {
                    case EnumForm enumForm:
                        if (enumForm.erlStringGenSerializer)
                        {
                            GenEnumPacker(erl, enumForm);
                            GenEnumParser(erl, enumForm);
                        }
                        break;
                }
            }
        }

        private void GenEnumPacker(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlStringGenPackName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlStringGenPackName}({enumForm.erlLocalType}) -> binary().

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlStringGenPackName}({field.erlName}) -> <<""{field.stringValue}"">>")}.
", enumForm);
        }

        private void GenEnumParser(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlStringGenParseName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlStringGenParseName}(binary()) -> {enumForm.erlLocalType}.

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlStringGenParseName}(<<""{field.stringValue}"">>) -> {field.erlName}")}.
", enumForm);
        }
    }
}
