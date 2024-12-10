using Igor.Lua.AST;

namespace Igor.Lua.Json
{
    public static class JsonSerialization
    {
        public static ISerializationTag Tag(IType type, Statement referrer)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTag.Bool;
                case BuiltInType.Integer _: return SerializationTag.Number;
                case BuiltInType.Float _: return SerializationTag.Number;
                case BuiltInType.Binary _: return SerializationTag.String;
                case BuiltInType.String _: return SerializationTag.String;
                case BuiltInType.Atom _: return SerializationTag.String;
                case BuiltInType.Json _: return SerializationTag.Json;
                case BuiltInType.List list: return new SerializationTag.List(Tag(list.ItemType, referrer));
                case BuiltInType.Dict dict: return new SerializationTag.Dict(Tag(dict.KeyType, referrer), Tag(dict.ValueType, referrer));
                case BuiltInType.Optional opt: return new SerializationTag.Optional(Tag(opt.ItemType, referrer));
                case BuiltInType.Flags flags: return new SerializationTag.List(Tag(flags.ItemType, referrer));
                case TypeForm f: return f.luaJsonTag(referrer);
                case GenericArgument f: return new SerializationTag.Var(f);
                case GenericType f: return f.luaJsonTag(referrer);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string Pack(ISerializationTag tag, string value)
        {
            switch (tag)
            {
                default:
                    return $"{tag}.to_json({value})";
            }
        }

        public static string Parse(ISerializationTag tag, string value)
        {
            switch (tag)
            {
                default:
                    return $"{tag}.from_json({value})";
            }
        }
    }
}
