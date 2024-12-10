using Igor.Elixir.AST;
using Igor.Elixir.Model;
using Igor.Text;

namespace Igor.Elixir
{
    internal class ExStringGenerator : IElixirGenerator
    {
        public void Generate(ExModel model, Module mod)
        {
            var ex = model.ModuleOf(mod);

            foreach (var type in mod.Types)
            {
                switch (type)
                {
                    case EnumForm enumForm:
                        if (enumForm.exStringGenSerializer)
                        {
                            GenEnum(ex, enumForm);
                        }
                        break;
                }
            }
        }

        private void GenEnum(ExModule protocolModule, EnumForm enumForm)
        {
            var ex = protocolModule.Module(enumForm.exName);

            ex.Function(
$@"@spec from_string!(String.t()) :: {enumForm.exLocalType}
{enumForm.Fields.JoinLines(field => $@"def from_string!(""{field.stringValue}""), do: {field.exName}")}
");
            ex.Function(
                $@"@spec to_string!({enumForm.exLocalType}) :: String.t()
{enumForm.Fields.JoinLines(field => $@"def to_string!({field.exName}), do: ""{field.stringValue}""")}
");

        }
    }
}
