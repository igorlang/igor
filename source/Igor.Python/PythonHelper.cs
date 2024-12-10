using Igor.Python.AST;

namespace Igor.Python
{
    public static class Helper
    {
        public static string ShadowName(string name) => name;

        public static string PyValue(Value value, IType type)
        {
            switch (value)
            {
                case Value.Bool b when b.Value: return "True";
                case Value.Bool b when !b.Value: return "False";
                case Value.Integer i:
                    return i.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.Float f:
                    return f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.String s: return $@"<<""{s.Value}"">>";
                case Value.List list when type is BuiltInType.List listType: return "{}";
                case Value.Dict dict when type is BuiltInType.Dict dictType: return "{}";
                case Value.Enum e: return $"{e.Field.Enum.pyName}.{e.Field.pyName}";
                case Value v when type is BuiltInType.Optional opt: return PyValue(v, opt.ItemType);
                default:
                    throw new EInternal($"invalid value {value} for type {type}");
            }
        }
    }
}
