using Igor.Erlang.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang
{
    public enum SerializationMode
    {
        Pack,
        Parse,
    }

    public abstract class SerializationTag
    {
        public abstract SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args);

        public abstract string Tag(SerializationMode mode);

        public string PackTag => Tag(SerializationMode.Pack);
        public string ParseTag => Tag(SerializationMode.Parse);

        public SerializationTag NullableIf(bool nullable) => nullable ? new SerializationTags.Nullable(this) : this;
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

            public override string Tag(SerializationMode mode) => Helper.PrimitiveTag(Type);

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;

            public Primitive(PrimitiveType type) => Type = type;
        }

        public class Json : SerializationTag
        {
            public override string Tag(SerializationMode mode) => "json";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;
        }

        public class List : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag(SerializationMode mode) => $"{{list, {ItemType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args));

            public List(SerializationTag itemType) => ItemType = itemType;
        }

        public class Dict : SerializationTag
        {
            public SerializationTag KeyType { get; }
            public SerializationTag ValueType { get; }

            public override string Tag(SerializationMode mode) => $"{{dict, {KeyType.Tag(mode)}, {ValueType.Tag(mode)}}}";

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

            public override string Tag(SerializationMode mode) => $"{{option, {ItemType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Optional(ItemType.Instantiate(args));

            public Optional(SerializationTag itemType) => ItemType = itemType;
        }

        public class Nullable : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag(SerializationMode mode) => $"{{nullable, {ItemType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Nullable(ItemType.Instantiate(args));

            public Nullable(SerializationTag itemType) => ItemType = itemType;
        }

        public class Flags : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag(SerializationMode mode) => $"{{flags, {ItemType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new List(ItemType.Instantiate(args));

            public Flags(SerializationTag itemType) => ItemType = itemType;
        }

        public class Enum : SerializationTag
        {
            public IntegerType IntType { get; }
            public string PackFun { get; }
            public string ParseFun { get; }

            public override string Tag(SerializationMode mode)
            {
                var intTag = Helper.PrimitiveTag(Igor.Primitive.FromInteger(IntType));
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                return $"{{enum, {intTag}, fun {fun}/1}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => this;

            public Enum(IntegerType intType, string packFun, string parseFun)
            {
                IntType = intType;
                PackFun = packFun;
                ParseFun = parseFun;
            }
        }

        public class Custom : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                if (Args.Count == 0)
                    return $"{{custom, fun {fun}/1}}";
                else
                    return $"{{custom, fun(V) -> {fun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new Custom(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public Custom(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }

        public class Var : SerializationTag
        {
            public GenericArgument Arg { get; }

            public override string Tag(SerializationMode mode) => Arg.erlName;

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                return args.GetValueOrDefault(Arg, this);
            }

            public Var(GenericArgument arg) => Arg = arg;
        }
    }
}
