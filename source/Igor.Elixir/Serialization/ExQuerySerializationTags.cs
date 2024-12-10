using Igor.Elixir.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Elixir
{
    public static class QuerySerializationTags
    {
        public class FromJson : SerializationTag
        {
            public SerializationTag JsonTag { get; }

            public override string Tag => $"{{:json, {JsonTag.Tag}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new FromJson(JsonTag.Instantiate(args));

            public FromJson(SerializationTag jsonTag) => JsonTag = jsonTag;
        }

        public class List : SerializationTag
        {
            public SerializationTag ItemType { get; }
            public bool Unfold { get; }
            public string Separator { get; }

            public override string Tag
            {
                get
                {
                    if (Unfold)
                        return $"{{:list, {ItemType.Tag}}}";
                    else
                        return $@"{{:list, ""{Separator}"", {ItemType.Tag}}}";
                }
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args), Unfold, Separator);

            public List(SerializationTag itemType, bool unfold, string separator)
            {
                ItemType = itemType;
                Unfold = unfold;
                Separator = separator;
            }
        }

        public class CustomQuery : SerializationTag
        {
            public string Module { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag
            {
                get
                {
                    if (Args.Count == 0)
                        return $"{{:custom_query, {Module}}}";
                    else
                        return $"{{:custom_query, fn(v) -> {Module}(v, {Args.JoinStrings(", ", a => a.Tag)}) end}}";
                }
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomQuery(Module, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomQuery(string module, IReadOnlyList<SerializationTag> args)
            {
                Module = module;
                Args = args;
            }
        }
    }
}
