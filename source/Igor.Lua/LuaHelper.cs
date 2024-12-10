using Igor.Lua.AST;

namespace Igor.Lua
{
    public static class Helper
    {
        public static string ShadowName(string name) => name;

        public static string LuaValue(Value value, IType type, string mod = null)
        {
            switch (value)
            {
                case Value.Bool b when b.Value: return "true";
                case Value.Bool b when !b.Value: return "false";
                case Value.Integer i:
                    return i.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.Float f:
                    return f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.String s: return $@"""{s.Value}""";
                case Value.List list when type is BuiltInType.List listType: return "{}";
                case Value.Dict dict when type is BuiltInType.Dict dictType: return "{}";
                case Value.Enum e: return $"{e.Field.Enum.luaRelativeName(mod)}.{e.Field.luaName}";
                case Value v when type is BuiltInType.Optional opt: return LuaValue(v, opt.ItemType, mod);
                default:
                    throw new EInternal($"invalid value {value} for type {type}");
            }
        }
    }
}
