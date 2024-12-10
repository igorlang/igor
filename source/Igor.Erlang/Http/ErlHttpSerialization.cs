using Igor.Erlang.AST;
using Igor.Text;

namespace Igor.Erlang.Http
{
    public static class HttpSerialization
    {
        public static SerializationTag UriTag(IType type, Statement referrer, Statement variable)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list: return new SerializationTags.List(UriTag(list.ItemType, referrer, variable));
                case BuiltInType.Dict dict: return new SerializationTags.Dict(UriTag(dict.KeyType, referrer, variable), UriTag(dict.ValueType, referrer, variable));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(UriTag(opt.ItemType, referrer, variable));
                case BuiltInType.Flags flags: return new SerializationTags.List(UriTag(flags.ItemType, referrer, variable));
                case TypeForm f: return f.erlHttpUriTag(referrer, variable);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlHttpUriTag(referrer, variable);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PackUri(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Primitive p when p.Type == PrimitiveType.String:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_http:format_value({value}, {tag.PackTag})";
            }
        }

        public static string ParseUri(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Primitive p when p.Type == PrimitiveType.String:
                    return value;
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_http:parse_value({value}, {tag.PackTag})";
            }
        }

        public static bool GetQueryUnfold(this Statement variable) => variable.Attribute(CoreAttributes.HttpUnfold, false);
        public static bool GetQueryUnfoldIndex(this Statement variable) => variable.Attribute(CoreAttributes.HttpUnfoldIndex, false);

        public static string GetListQuerySeparator(this Statement variable) => variable.Attribute(CoreAttributes.HttpSeparator, ",");

        public static bool GetHttpParts(this Statement variable) => variable.Attribute(CoreAttributes.HttpPathParts, false);

        public static SerializationTag HttpQueryTag(IType type, Statement referrer, Statement variable, DataFormat dataFormat)
        {
            var httpQueryTag = HttpQueryTag(type, referrer, variable);
            if (dataFormat == DataFormat.Json)
            {
                if (httpQueryTag is SerializationTags.Optional opt)
                    httpQueryTag = new SerializationTags.Optional(new QuerySerializationTags.FromJson(opt.ItemType));
                else
                    httpQueryTag = new QuerySerializationTags.FromJson(httpQueryTag);
            }
            return httpQueryTag;
        }

        public static SerializationTag HttpFormTag(IType type, Statement referrer, Statement variable)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list:
                    {
                        var unfold = variable == null ? false : variable.GetQueryUnfold();
                        var unfoldIndex = variable == null ? false : variable.GetQueryUnfoldIndex();
                        var separator = variable == null ? "," : variable.GetListQuerySeparator();
                        return new QuerySerializationTags.List(HttpQueryTag(list.ItemType, referrer, null), unfold, unfoldIndex, separator);
                    }
                case BuiltInType.Dict dict: return new SerializationTags.Dict(HttpQueryTag(dict.KeyType, referrer, null), HttpQueryTag(dict.ValueType, referrer, null));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(HttpQueryTag(opt.ItemType, referrer, null));
                case BuiltInType.Flags flags:
                    {
                        var unfold = variable == null ? false : variable.GetQueryUnfold();
                        var unfoldIndex = variable == null ? false : variable.GetQueryUnfoldIndex();
                        var separator = variable == null ? "," : variable.GetListQuerySeparator();
                        return new QuerySerializationTags.List(HttpQueryTag(flags.ItemType, referrer, null), unfold, unfoldIndex, separator);
                    }
                case TypeForm f: return f.erlHttpFormTag(referrer, variable);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlHttpQueryTag(referrer, variable);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static SerializationTag HttpQueryTag(IType type, Statement referrer, Statement variable)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return SerializationTags.Bool;
                case BuiltInType.Integer integer: return new SerializationTags.Primitive(Primitive.FromInteger(integer.Type));
                case BuiltInType.Float f: return new SerializationTags.Primitive(Primitive.FromFloat(f.Type));
                case BuiltInType.Binary _: return SerializationTags.Binary;
                case BuiltInType.String _: return SerializationTags.String;
                case BuiltInType.Atom _: return SerializationTags.Atom;
                case BuiltInType.List list:
                    {
                        var unfold = variable == null ? false : variable.GetQueryUnfold();
                        var unfoldIndex = variable == null ? false : variable.GetQueryUnfoldIndex();
                        var separator = variable == null ? "," : variable.GetListQuerySeparator();
                        return new QuerySerializationTags.List(HttpQueryTag(list.ItemType, referrer, null), unfold, unfoldIndex, separator);
                    }
                case BuiltInType.Dict dict: return new SerializationTags.Dict(HttpQueryTag(dict.KeyType, referrer, null), HttpQueryTag(dict.ValueType, referrer, null));
                case BuiltInType.Optional opt: return new SerializationTags.Optional(HttpQueryTag(opt.ItemType, referrer, null));
                case BuiltInType.Flags flags:
                    {
                        var unfold = variable == null ? false : variable.GetQueryUnfold();
                        var unfoldIndex = variable == null ? false : variable.GetQueryUnfoldIndex();
                        var separator = variable == null ? "," : variable.GetListQuerySeparator();
                        return new QuerySerializationTags.List(HttpQueryTag(flags.ItemType, referrer, null), unfold, unfoldIndex, separator);
                    }
                case BuiltInType.Json _: return new SerializationTags.Json();
                case TypeForm f: return f.erlHttpQueryTag(referrer, variable);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlHttpQueryTag(referrer, variable);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PackHttpForm(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_text:pack_value({value}, {tag.PackTag})";
            }
        }

        public static string ParseHttpForm(this SerializationTag tag, string value, string module = null)
        {
            switch (tag)
            {
                case SerializationTags.Custom c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_text:parse_value({value}, {tag.ParseTag})";
            }
        }
    }
}
