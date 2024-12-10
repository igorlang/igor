using Igor.Lua.AST;
using Igor.Lua.Model;

namespace Igor.Lua
{
    public interface ILuaGenerator
    {
        void Generate(LuaModel model, Module mod);
    }
}
