using Igor.Go.AST;
using Igor.Go.Model;
using Igor.Text;
using System.Globalization;
using System.Linq;

namespace Igor.Go
{
    internal class GoTypeGenerator : IGoGenerator
    {
        public void Generate(GoModel model, Module mod)
        {
            var file = model.FileOf(mod);
            foreach (var script in mod.goImports)
                file.Import(script);
            foreach (var type in mod.Types)
            {
                if (type.goEnabled && type.goAlias == null)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenerateEnum(model, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenerateStruct(model, recordForm);
                            break;

                        case VariantForm variantForm:
                            GenerateStruct(model, variantForm);
                            break;

                        case DefineForm defineForm:
                            GenerateDefine(model, defineForm);
                            break;

                        case InterfaceForm interfaceForm:
                            GenerateStruct(model, interfaceForm);
                            break;
                    }
                }
            }
        }

        private void GenerateDefine(GoModel model, DefineForm defineForm)
        {
            var typedef = model.TypeOf(defineForm);
            typedef.Comment = defineForm.goComment;
            typedef.Type = defineForm.goTypeDefinition.Name;

            if (defineForm.IsGeneric)
                typedef.GenericArgs = defineForm.Args.ToDictionary(a => a.Name, a => Helper.GenericTypeConditional(defineForm.goTypeDefinition));
        }

        private void GenerateEnum(GoModel model, EnumForm enumForm)
        {
            var e = model.TypeOf(enumForm);
            e.BaseType = enumForm.goBaseType;
            e.Comment = enumForm.goComment;
            e.Group = enumForm;
            foreach (var field in enumForm.Fields)
            {
                var fieldModel = e.Field(field.goName);
                fieldModel.Comment = field.goComment;
                fieldModel.Value = enumForm.goStringEnum ? field.Name.Quoted() : field.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void GenerateStruct(GoModel model, StructForm structForm)
        {
            var file = model.FileOf(structForm);
            var c = model.TypeOf(structForm);

            if (structForm.Ancestor != null)
                c.Embed(structForm.Ancestor.goName);

            foreach (var intf in structForm.Interfaces)
            {
                var goIntf = Helper.TargetInterface(intf);
                c.Embed(goIntf.Name);
            }

            if (structForm.IsGeneric)
                c.GenericArgs = structForm.Args.Select(a => a.Name).ToList();

            c.Comment = structForm.goComment;
            c.Group = structForm;

            foreach (var f in structForm.Fields.Where(f => f.IsLocal))
            {
                var prop = c.Property(f.goName);
                prop.Type = (f.goPtr ? "*" : "") + f.goType.Name;
                prop.Comment = f.goComment;
                foreach (var import in f.goType.Imports)
                {
                    file.Import(import);
                }
            }
        }
    }
}
