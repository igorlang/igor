using Igor.Elixir.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Elixir
{
    public abstract class SerializationTag
    {
        public abstract SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args);

        public abstract string Tag { get; }

        public override string ToString() => Tag;
    }

    public static class SerializationTags
    {
        public static readonly SerializationTag Bool = new Primitive(PrimitiveType.Bool);
        public static readonly SerializationTag Binary = new Primitive(PrimitiveType.Binary);
        public static readonly SerializationTag Byte = new Primitive(PrimitiveType.Byte);
        public static readonly SerializationTag UShort = new Primitive(PrimitiveType.UShort);
        public static readonly SerializationTag Int = new Primitive(PrimitiveType.Int);
        public static readonly SerializationTag String = new Primitive(PrimitiveType.String);
        public static readonly SerializationTag Atom = new Primitive(PrimitiveType.Atom);

        public class Primitive : SerializationTag
        {
            public PrimitiveType Type { get; }

            public override string Tag => Helper.PrimitiveTag(Type);

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;

            public Primitive(PrimitiveType type) => Type = type;
        }

        public class Json : SerializationTag
        {
            public override string Tag => ":json";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;
        }

        public class List : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag => $"{{:list, {ItemType.Tag}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args));

            public List(SerializationTag itemType) => ItemType = itemType;
        }

        public class Dict : SerializationTag
        {
            public SerializationTag KeyType { get; }
            public SerializationTag ValueType { get; }

            public override string Tag => $"{{:map, {KeyType.Tag}, {ValueType.Tag}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Dict(KeyType.Instantiate(args), ValueType.Instantiate(args));

            public Dict(SerializationTag keyType, SerializationTag valueType)
            {
                KeyType = keyType;
                ValueType = valueType;
            }
        }

        public class Optional : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag => $"{{:option, {ItemType.Tag}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Optional(ItemType.Instantiate(args));

            public Optional(SerializationTag itemType) => ItemType = itemType;
        }

        public class Flags : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag => $"{{:flags, {ItemType.Tag}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args));

            public Flags(SerializationTag itemType) => ItemType = itemType;
        }

        public class Enum : SerializationTag
        {
            public IntegerType IntType { get; }
            public string Module { get; }

            public override string Tag
            {
                get
                {
                    var intTag = Helper.PrimitiveTag(Igor.Primitive.FromInteger(IntType));
                    return $"{{:enum, {intTag}, {Module}}}";
                }
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;

            public Enum(IntegerType intType, string module)
            {
                IntType = intType;
                Module = module;
            }
        }

        public class Custom : SerializationTag
        {
            public string Module { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag
            {
                get
                {
                    if (Args.Count == 0)
                        return $"{{:custom, {Module}}}";
                    else
                        return $"{{:custom, {Module}, {{{Args.JoinStrings(", ", a => a.Tag)}}}}}";
                }
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new Custom(Module, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public Custom(string module, IReadOnlyList<SerializationTag> args)
            {
                Module = module;
                Args = args;
            }
        }

        public class Var : SerializationTag
        {
            public GenericArgument Arg { get; }

            public override string Tag => Arg.exTypeTagVarName;

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                return args.GetValueOrDefault(Arg, this);
            }

            public Var(GenericArgument arg) => Arg = arg;
        }
    }
}
