using Igor.Erlang.AST;
using Igor.Text;

namespace Igor.Erlang.Xml
{
    public enum XmlTagType
    {
        SimpleType,
        ComplexType,
        Element,
        Content,
        Repeated,
        Generic,
    }

    public static class XmlSerialization
    {
        private static T GetAttr<T>(Statement field, AttributeDescriptor<T> attribute, T @default)
        {
            if (field == null)
                return @default;
            else
                return field.Attribute(attribute, @default);
        }

        public static XmlTagType TagType(this SerializationTag tag)
        {
            switch (tag)
            {
                case SerializationTags.Primitive _: return XmlTagType.SimpleType;
                case SerializationTags.Optional t: return t.TagType();
                case SerializationTags.List _: return XmlTagType.Content;
                case SerializationTags.Var _: return XmlTagType.Generic;
                case XmlSerializationTags.CustomComplexType _: return XmlTagType.ComplexType;
                case XmlSerializationTags.CustomContent _: return XmlTagType.Content;
                case XmlSerializationTags.CustomSimpleType _: return XmlTagType.SimpleType;
                case XmlSerializationTags.CustomElement _: return XmlTagType.Element;
                case XmlSerializationTags.Element _: return XmlTagType.Element;
                case XmlSerializationTags.Repeated _: return XmlTagType.Repeated;
                default: throw new EUnknownType(tag.ToString());
            }
        }

        public static SerializationTag XmlTag(IType type, Statement referrer, Statement field)
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
                    if (GetAttr(field, CoreAttributes.XmlFlat, false))
                        return new XmlSerializationTags.Repeated(XmlTag(list.ItemType, referrer, null));
                    else
                    {
                        var itemTag = XmlTag(list.ItemType, referrer, null);
                        var itemName = Helper.AtomName(GetAttr(field, CoreAttributes.XmlItemName, "item"));
                        if (itemTag.TagType() != XmlTagType.Element)
                            itemTag = new XmlSerializationTags.Element(itemTag, itemName);
                        return new SerializationTags.List(itemTag);
                    }
                case BuiltInType.Dict dict:
                    {
                        var keyTag = XmlTag(dict.KeyType, referrer, null);
                        var valueTag = XmlTag(dict.ValueType, referrer, null);
                        var keyName = Helper.AtomName(GetAttr(field, CoreAttributes.XmlKeyName, "key"));
                        var valueName = Helper.AtomName(GetAttr(field, CoreAttributes.XmlValueName, "value"));
                        if (GetAttr(field, CoreAttributes.XmlKVList, false))
                        {
                            if (keyTag.TagType() != XmlTagType.Element)
                                keyTag = new XmlSerializationTags.Element(keyTag, keyName);
                            if (valueTag.TagType() != XmlTagType.Element)
                                valueTag = new XmlSerializationTags.Element(valueTag, valueName);
                            return new XmlSerializationTags.KVList(keyTag, valueTag);
                        }
                        else
                        {
                            var itemName = Helper.AtomName(GetAttr(field, CoreAttributes.XmlItemName, "item"));
                            var key = GetAttr(field, CoreAttributes.XmlKey, XmlPairItemType.Element);
                            var value = GetAttr(field, CoreAttributes.XmlValue, XmlPairItemType.Element);
                            SerializationTag GetTagByType(XmlPairItemType t, SerializationTag tag, string name)
                            {
                                switch (t)
                                {
                                    case XmlPairItemType.Content: return tag;
                                    case XmlPairItemType.Attribute: return new XmlSerializationTags.Attribute(tag, name);
                                    default: return new XmlSerializationTags.Subelement(tag, name);
                                }
                            }
                            var pairTag = new XmlSerializationTags.Pair(GetTagByType(key, keyTag, keyName), GetTagByType(value, valueTag, valueName));
                            if (GetAttr(field, CoreAttributes.XmlFlat, false))
                                return new XmlSerializationTags.Repeated(pairTag);
                            else
                                return new SerializationTags.List(new XmlSerializationTags.Element(pairTag, itemName));
                        }
                    }
                case BuiltInType.Optional opt: return new SerializationTags.Optional(XmlTag(opt.ItemType, referrer, field));
                case BuiltInType.Flags flags:
                    if (GetAttr(field, CoreAttributes.XmlFlat, false))
                        return new XmlSerializationTags.Repeated(XmlTag(flags.ItemType, referrer, null));
                    else
                        return new SerializationTags.List(new XmlSerializationTags.Element(XmlTag(flags.ItemType, referrer, null), Helper.AtomName(GetAttr(field, CoreAttributes.XmlItemName, "item"))));
                case TypeForm f: return f.erlXmlTag(referrer, field);
                case GenericArgument f: return new SerializationTags.Var(f);
                case GenericType f: return f.erlXmlTag(referrer, field);
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string ParseXml(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case XmlSerializationTags.CustomElement c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_xml:parse_element({value}, {tag.ParseTag})";
            }
        }

        public static string ParseXmlContent(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case XmlSerializationTags.CustomContent c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_xml:parse_content({value}, {tag.ParseTag})";
            }
        }

        public static string ParseXmlComplexType(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case XmlSerializationTags.CustomContent c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_xml:parse_complex_type({value}, {tag.ParseTag})";
            }
        }

        public static string ParseXmlSimpleType(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case XmlSerializationTags.CustomSimpleType c:
                    var fun = StringHelper.RelativeName(c.ParseFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    return $"igor_xml:parse_simple_type({value}, {tag.ParseTag})";
            }
        }

        public static string PackXmlElement(this SerializationTag tag, string value, string module)
        {
            switch (tag)
            {
                case XmlSerializationTags.CustomElement c:
                    var fun = StringHelper.RelativeName(c.PackFun, module, ':');
                    return $"{fun}({value}{c.Args.JoinStrings(arg => $", {arg}")})";
                default:
                    throw new EInternal($"Cannot pack {tag.PackTag} as an element.");
                    // return $"igor_xml:pack_element({value}, {tag})";
            }
        }
    }
}
