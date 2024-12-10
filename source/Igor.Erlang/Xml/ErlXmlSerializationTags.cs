using Igor.Erlang.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Xml
{
    public static class XmlSerializationTags
    {
        public class Repeated : SerializationTag
        {
            public SerializationTag ItemType { get; }

            public override string Tag(SerializationMode mode) => $@"{{repeated, {ItemType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Repeated(ItemType.Instantiate(args));

            public Repeated(SerializationTag itemType)
            {
                ItemType = itemType;
            }
        }

        public class KVList : SerializationTag
        {
            public SerializationTag KeyType { get; }
            public SerializationTag ValueType { get; }

            public override string Tag(SerializationMode mode) => $"{{kvlist, {KeyType.Tag(mode)}, {ValueType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new KVList(KeyType.Instantiate(args), ValueType.Instantiate(args));

            public KVList(SerializationTag keyType, SerializationTag valueType)
            {
                KeyType = keyType;
                ValueType = valueType;
            }
        }

        public class Pair : SerializationTag
        {
            public SerializationTag KeyType { get; }
            public SerializationTag ValueType { get; }

            public override string Tag(SerializationMode mode) => $"{{pair, {KeyType.Tag(mode)}, {ValueType.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Pair(KeyType.Instantiate(args), ValueType.Instantiate(args));

            public Pair(SerializationTag keyType, SerializationTag valueType)
            {
                KeyType = keyType;
                ValueType = valueType;
            }
        }

        public class Element : SerializationTag
        {
            public SerializationTag Type { get; }
            public string Name { get; }

            public override string Tag(SerializationMode mode) => $"{{element, {Name}, {Type.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Element(Type.Instantiate(args), Name);

            public Element(SerializationTag type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        public class Attribute : SerializationTag
        {
            public SerializationTag Type { get; }
            public string Name { get; }

            public override string Tag(SerializationMode mode) => $"{{attribute, {Name}, {Type.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Attribute(Type.Instantiate(args), Name);

            public Attribute(SerializationTag type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        public class Subelement : SerializationTag
        {
            public SerializationTag Type { get; }
            public string Name { get; }

            public override string Tag(SerializationMode mode) => $"{{subelement, {Name}, {Type.Tag(mode)}}}";

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args) => new Subelement(Type.Instantiate(args), Name);

            public Subelement(SerializationTag type, string name)
            {
                Type = type;
                Name = name;
            }
        }

        public class CustomSimpleType : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                if (Args.Count == 0)
                    return $"{{custom_simple_type, fun {fun}/1}}";
                else
                    return $"{{custom_simple_type, fun(V) -> {fun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomSimpleType(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomSimpleType(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }

        public class CustomComplexType : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                if (mode == SerializationMode.Pack)
                {
                    if (Args.Count == 0)
                        return $"{{custom_complex_type, fun {PackFun}/2}}";
                    else
                        return $"{{custom_complex_type, fun(V, ElementName) -> {PackFun}(V, ElementName, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
                }
                else
                {
                    if (Args.Count == 0)
                        return $"{{custom_complex_type, fun {ParseFun}/1}}";
                    else
                        return $"{{custom_complex_type, fun(V) -> {ParseFun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
                }
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomComplexType(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomComplexType(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }

        public class CustomElement : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                if (Args.Count == 0)
                    return $"{{custom_element, fun {fun}/1}}";
                else
                    return $"{{custom_element, fun(V) -> {fun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomElement(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomElement(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }

        public class CustomContent : SerializationTag
        {
            public string PackFun { get; }
            public string ParseFun { get; }
            public IReadOnlyList<SerializationTag> Args { get; }

            public override string Tag(SerializationMode mode)
            {
                var fun = mode == SerializationMode.Pack ? PackFun : ParseFun;
                if (Args.Count == 0)
                    return $"{{custom_content, fun {fun}/1}}";
                else
                    return $"{{custom_content, fun(V) -> {fun}(V, {Args.JoinStrings(", ", a => a.Tag(mode))}) end}}";
            }

            public override SerializationTag Instantiate(IDictionary<GenericArgument, SerializationTag> args)
            {
                if (Args.Count == 0)
                    return this;
                else
                    return new CustomContent(PackFun, ParseFun, Args.Select(a => a.Instantiate(args)).ToList());
            }

            public CustomContent(string packFun, string parseFun, IReadOnlyList<SerializationTag> args)
            {
                PackFun = packFun;
                ParseFun = parseFun;
                Args = args;
            }
        }
    }
}
