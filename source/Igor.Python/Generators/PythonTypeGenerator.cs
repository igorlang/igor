using Igor.Python.AST;
using Igor.Python.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Python
{
    internal class PythonTypeGenerator : IPythonGenerator
    {
        public void Generate(PythonModel model, Module mod)
        {
            var file = model.File(mod.pyFileName);
            foreach (var script in mod.pyImports)
                file.Import(script);
            foreach (var type in mod.Types)
            {
                if (type.pyGenerateDeclaration)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenerateEnum(model, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenerateClass(model, recordForm);
                            break;

                        case VariantForm variantForm:
                            GenerateClass(model, variantForm);
                            break;
                    }
                }
            }
        }

        private void GenerateEnum(PythonModel model, EnumForm enumForm)
        {
            model.FileOf(enumForm).Import("enum").Types("Enum");
            var e = model.TypeOf(enumForm);
            foreach (var field in enumForm.Fields)
            {
                var fieldModel = e.Field(field.pyName);
                fieldModel.Value = field.Value;
            }
        }

        public void GenerateClass(PythonModel model, StructForm structForm)
        {
            var c = model.TypeOf(structForm);

            if (structForm.Ancestor != null)
                c.BaseClass = structForm.Ancestor.pyName;

            var initFields = structForm.Fields.Where(p => !p.IsInherited || (p.IsTag && structForm.Ancestor == null));
            var inits = initFields.Select(p => $"    self.{p.pyFieldName} = {p.pyDefault}").JoinLines();
            var super = structForm.Ancestor == null ? "" : "    super().__init__()";
            var pass = structForm.Ancestor == null && !initFields.Any() ? "    pass" : "";

            c.Function($@"def __init__(self):
{super}
{inits}
{pass}
");
            /*
            foreach (var f in structForm.Fields.Where(f => !f.IsInherited))
            {
                c.Function($@"
function {c.Name}:get_{f.pyName}()
    return self{f.pyIndexer}
end
");
                if (!f.IsTag)
                {
                    c.Function($@"
function {c.Name}:set_{f.pyName}({f.pyName})
    self{f.pyIndexer} = {f.pyName}
end
");
                }
            }*/
        }
    }
}
