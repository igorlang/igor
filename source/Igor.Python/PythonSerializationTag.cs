using Igor.Python.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Python
{
    public interface ISerializationTag
    {
        ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args);
    }

    public abstract class SerializationTag : ISerializationTag
    {
        public abstract ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args);

        public static readonly ISerializationTag Bool = new BoolTag();
        public static readonly ISerializationTag Number = new NumberTag();
        public static readonly ISerializationTag String = new StringTag();

        public class BoolTag : SerializationTag
        {
            public override string ToString() => "igor.json.Bool";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => this;
        }

        public class NumberTag : SerializationTag
        {
            public override string ToString() => "igor.json.Number";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => this;
        }

        public class StringTag : SerializationTag
        {
            public override string ToString() => "igor.json.String";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => this;
        }

        public class List : SerializationTag
        {
            public ISerializationTag ItemType { get; }

            public override string ToString() => $"igor.json.List.generic_instance({ItemType})";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => new List(ItemType.Instantiate(args));

            public List(ISerializationTag itemType) => ItemType = itemType;
        }

        public class Dict : SerializationTag
        {
            public ISerializationTag KeyType { get; }
            public ISerializationTag ValueType { get; }

            public override string ToString() => $"igor.json.Dict.generic_instance({KeyType}, {ValueType})";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => new Dict(KeyType.Instantiate(args), ValueType.Instantiate(args));

            public Dict(ISerializationTag keyType, ISerializationTag valueType)
            {
                KeyType = keyType;
                ValueType = valueType;
            }
        }

        public class Optional : SerializationTag
        {
            public ISerializationTag ItemType { get; }

            public override string ToString() => $"igor.json.Optional.generic_instance({ItemType})";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => new Optional(ItemType.Instantiate(args));

            public Optional(ISerializationTag itemType) => ItemType = itemType;
        }

        public class Flags : SerializationTag
        {
            public ISerializationTag ItemType { get; }

            public override string ToString() => $"{{flags, {ItemType}}}";

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args) => new List(ItemType.Instantiate(args));

            public Flags(ISerializationTag itemType) => ItemType = itemType;
        }

        public class Custom : SerializationTag
        {
            public string Fun { get; }
            public IReadOnlyList<ISerializationTag> Args { get; }

            public override string ToString()
            {
                if (Args.Count == 0)
                    return Fun;
                else
                    return $"{Fun}.generic_instance({Args.JoinStrings(", ", a => a.ToString())})";
            }

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new Custom(Fun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public Custom(string fun, IReadOnlyList<ISerializationTag> args)
            {
                Fun = fun;
                Args = args;
            }
        }

        public class Var : SerializationTag
        {
            public GenericArgument Arg { get; }

            public override string ToString() => Arg.pyName;

            public override ISerializationTag Instantiate(IDictionary<GenericArgument, ISerializationTag> args)
            {
                return args.GetValueOrDefault(Arg, this);
            }

            public Var(GenericArgument arg) => Arg = arg;
        }
    }
}
