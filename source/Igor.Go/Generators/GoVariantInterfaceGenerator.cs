using Igor.Go.AST;
using Igor.Go.Model;
using Igor.Text;

namespace Igor.Go
{
    internal class GoVariantInterfaceGenerator : IGoGenerator
    {
        public void Generate(GoModel model, Module mod)
        {
            foreach (var type in mod.Types)
            {
                if (type.goEnabled)
                {
                    switch (type)
                    {
                        case VariantForm variantForm:
                            if (variantForm.goInterface != null)
                            {
                                GenerateVariantInterface(model, variantForm);
                            }
                            break;
                    }
                }
            }
        }

        public void GenerateVariantInterface(GoModel model, VariantForm variantForm)
        {
            var f = model.FileOf(variantForm);
            var intf = f.Interface(variantForm.goInterface);
            var fun = $"is{variantForm.goInterface}";
            intf.Function($"{fun}()");

            var varName = variantForm.goName.Format(Notation.LowerCamel, true);

            foreach (var r in variantForm.Records)
            {
                var c = model.FileOf(r);
                c.Declare($"func ({varName} {r.goName}) {fun}() {{}}", r);
            }
        }
    }
}
