using Igor.Erlang.AST;
using Igor.Text;

namespace Igor.Erlang.Json
{
    public static class JsonSerialization
    {
        public static SerializationTag JsonTag(IType type, Statement referrer)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list: return new SerializationTags.List(JsonTag(list.ItemType, referrer));
                case BuiltInType.Dict dict: return new SerializationTags.Dict(JsonTag(dict.KeyType, referrer), JsonTag(dict.ValueType, referrer));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(JsonTag(opt.ItemType, referrer));
                case BuiltInType.Flags flags: return new SerializationTags.List(JsonTag(flags.ItemType, referrer));
                case BuiltInType.Json _: return new SerializationTags.Json();
                case TypeForm f: return f.erlJsonTag(referrer);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlJsonTag(referrer);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PackJson(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Json _:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg.PackTag}")})";
                default:
                    return $"igor_json:pack_value({value}, {tag.PackTag})";
            }
        }

        public static string ParseJson(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Json _:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg.ParseTag}")})";
                default:
                    return $"igor_json:parse_value({value}, {tag.ParseTag})";
            }
        }

        public static string ParseJsonFun(this SerializationTag tag, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Custom c when c.Args.Count == 0:
                    {
                        var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                        return $"fun {fun}/1";
                    }
                case SerializationTags.Custom c:
                    {
                        var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                        return $"fun(V) -> {fun}(V, {c.Args.JoinStrings(arg => $", {arg}")}) end";
                    }
                default:
                    return $"fun(V) -> igor_json:parse_value(V, {tag.ParseTag}) end";
            }
        }
    }
}
