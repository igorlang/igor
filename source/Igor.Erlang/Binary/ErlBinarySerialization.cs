using Igor.Erlang.AST;
using Igor.Text;

namespace Igor.Erlang.Binary
{
    public static class BinarySerialization
    {
        public static SerializationTag BinaryTag(IType type, Statement referrer)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list: return new SerializationTags.List(BinaryTag(list.ItemType, referrer));
                case BuiltInType.Dict dict: return new SerializationTags.Dict(BinaryTag(dict.KeyType, referrer), BinaryTag(dict.ValueType, referrer));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(BinaryTag(opt.ItemType, referrer));
                case BuiltInType.Flags flags: return new SerializationTags.Flags(BinaryTag(flags.ItemType, referrer));
                case TypeForm f: return f.erlBinaryTag(referrer);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlBinaryTag(referrer);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PackBinary(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_binary:pack_value({value}, {tag.PackTag})";
            }
        }

        public static string ParseBinary(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_binary:parse_value({value}, {tag.ParseTag})";
            }
        }
    }
}
