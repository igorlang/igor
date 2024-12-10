using Igor.Lua.AST;

namespace Igor.Lua.Model
{
    public static class LuaModelHelper
    {
        public static LuaFile FileOf(this LuaModel model, Module astModule)
        {
            return model.File(astModule.luaFileName);
        }

        public static LuaFile FileOf(this LuaModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }

        public static LuaClass TypeOf(this LuaModel model, StructForm astStruct)
        {
            return model.FileOf(astStruct).Class(astStruct.luaName);
        }

        public static LuaEnum TypeOf(this LuaModel model, EnumForm astEnum)
        {
            return model.FileOf(astEnum).Enum(astEnum.luaName);
        }
    }
}
