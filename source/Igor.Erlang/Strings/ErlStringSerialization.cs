using Igor.Erlang.AST;
using Igor.Text;

namespace Igor.Erlang.Strings
{
    public static class StringSerialization
    {
        public static SerializationTag StringTag(IType type, Statement referrer)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list: return new SerializationTags.List(StringTag(list.ItemType, referrer));
                case BuiltInType.Dict dict: return new SerializationTags.Dict(StringTag(dict.KeyType, referrer), StringTag(dict.ValueType, referrer));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(StringTag(opt.ItemType, referrer));
                case BuiltInType.Flags flags: return new SerializationTags.List(StringTag(flags.ItemType, referrer));
                case TypeForm f: return f.erlStringTag(referrer);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlStringTag(referrer);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PackString(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Primitive p when p.Type == PrimitiveType.String:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_string:format({value}, {tag.PackTag})";
            }
        }

        public static string ParseString(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Primitive p when p.Type == PrimitiveType.String:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_string:parse({value}, {tag.PackTag})";
            }
        }

        public static string ParseStringWithDefault(this SerializationTag tag, string value, string @default, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Primitive p when p.Type == PrimitiveType.String:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_string:parse({value}, {tag.PackTag}, {@default})";
            }
        }
    }
}
