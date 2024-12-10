using Igor.Lua.AST;
using Igor.Lua.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Lua
{
    internal class LuaTypeGenerator : ILuaGenerator
    {
        public void Generate(LuaModel model, Module mod)
        {
            var file = model.File(mod.luaFileName);
            foreach (var script in mod.luaRequires)
                file.Require(script);
            foreach (var type in mod.Types)
            {
                if (type.luaGenerateDeclaration)
                {
                    if (type.luaNamespace != null)
                        file.Namespace(type.luaNamespace);
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

        private void GenerateEnum(LuaModel model, EnumForm enumForm)
        {
            var e = model.TypeOf(enumForm);
            e.Namespace = enumForm.luaNamespace;
            e.Style = enumForm.luaEnumStyle;
            foreach (var field in enumForm.Fields)
            {
                var fieldModel = e.Field(field.luaName);
                fieldModel.Value = field.Value;
            }
        }

        public void GenerateClass(LuaModel model, StructForm structForm)
        {
            var c = model.TypeOf(structForm);
            c.Style = structForm.luaRecordStyle;

            if (structForm.Ancestor != null)
                c.Super = structForm.Ancestor.luaName;

            c.Namespace = structForm.luaNamespace;

            if (structForm.luaRecordStyle == RecordStyle.Class)
            {
                var inits = structForm.Fields.Where(p => p.HasDefault && !p.IsInherited || p.IsTag).Select(p => $"    self.{p.luaFieldName} = {p.luaDefault}").JoinLines();
                var super = structForm.Ancestor == null ? "" : $"    self.super:init({structForm.Ancestor.luaName})";

                c.Function($@"
function {c.Name}:init()
{super}
{inits}
end
");

                foreach (var f in structForm.Fields.Where(f => !f.IsInherited))
                {
                    c.Function($@"
function {c.Name}:get_{f.luaName}()
    return self{f.luaIndexer}
end
");
                    if (!f.IsTag)
                    {
                        c.Function($@"
function {c.Name}:set_{f.luaName}({f.luaName})
    self{f.luaIndexer} = {f.luaName}
end
");
                    }
                }
            }
        }
    }
}
