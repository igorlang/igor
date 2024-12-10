using Igor.Erlang.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Http
{
    public static class QuerySerializationTags
    {
        public class FromJson : SerializationTag
        {
            public SerializationTag JsonTag { get; }

            public override string Tag(SerializationMode mode) => JsonTag is SerializationTags.Json ? "json" : $"{{json, {JsonTag.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new FromJson(JsonTag.Instantiate(args));

            public FromJson(SerializationTag jsonTag) => JsonTag = jsonTag;
        }

        public class List : SerializationTag
        {
            public SerializationTag ItemType { get; }
            public bool Unfold { get; }
            public bool UnfoldIndex { get; }
            public string Separator { get; }

            public override string Tag(SerializationMode mode)
            {
                if (UnfoldIndex)
                    return $"{{list_index, {ItemType.Tag(mode)}}}";
                else if (Unfold)
                    return $"{{list, {ItemType.Tag(mode)}}}";
                else
                    return $@"{{list, <<""{Separator}"">>, {ItemType.Tag(mode)}}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args), Unfold, UnfoldIndex, Separator);

            public List(SerializationTag itemType, bool unfold, bool unfoldIndex, string separator)
            {
                ItemType = itemType;
                Unfold = unfold;
                UnfoldIndex = unfoldIndex;
                Separator = separator;
            }
        }

        public class CustomQuery : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                if (Args.Count == 0)
                    return $"{{custom_query, fun {fun}/1}}";
                else
                    return $"{{custom_query, fun(V) -> {fun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomQuery(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomQuery(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }
    }
}
