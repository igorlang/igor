using Igor.Erlang.Xml;

namespace Igor.Erlang.AST
{
    public partial class TypeForm
    {
        private string erlXmlUserParser => Attribute(ErlAttributes.XmlParser, null);
        private string erlXmlUserPacker => Attribute(ErlAttributes.XmlPacker, null);
        private bool erlXmlUserSerializer => erlXmlUserParser != null && erlXmlUserPacker != null;

        public bool erlXmlGenSerializer => xmlEnabled && !erlXmlUserSerializer && (erlPrimitiveType == PrimitiveType.None);
        public string erlXmlParserName => $"{erlName}_from_xml";
        public string erlXmlPackerName => $"{erlName}_to_xml";

        public SerializationTag erlXmlTag(Statement referrer, Statement field)
        {
            if (!xmlEnabled)
                Error($"XML serialization is not enabled but required for {referrer}. Use xml.enabled attribute to enable xml serialization.");

            if (erlXmlUserSerializer)
            {
                switch (xmlType)
                {
                    case XmlType.SimpleType:
                        return new XmlSerializationTags.CustomSimpleType(erlXmlUserPacker, erlXmlUserParser, erlArgTags);
                    case XmlType.Element:
                        return new XmlSerializationTags.CustomElement(erlXmlUserPacker, erlXmlUserParser, erlArgTags);
                    case XmlType.Content:
                        return new XmlSerializationTags.CustomContent(erlXmlUserPacker, erlXmlUserParser, erlArgTags);
                    default:
                        return new XmlSerializationTags.CustomComplexType(erlXmlUserPacker, erlXmlUserParser, erlArgTags);
                }
            }
            else if (erlPrimitiveType != PrimitiveType.None)
                return new SerializationTags.Primitive(erlPrimitiveType);
            else
                return erlXmlGenTag(referrer, field);
        }

        protected abstract SerializationTag erlXmlGenTag(Statement referrer, Statement field);
    }

    public partial class GenericType
    {
        public SerializationTag erlXmlTag(Statement referrer, Statement field) => Prototype.erlXmlTag(referrer, field).Instantiate(PrepareArgs(XmlSerialization.XmlTag, referrer, field));
    }

    public partial class EnumForm
    {
        protected override SerializationTag erlXmlGenTag(Statement referrer, Statement field) => new XmlSerializationTags.CustomSimpleType($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
    }

    public partial class StructForm
    {
        protected override SerializationTag erlXmlGenTag(Statement referrer, Statement field)
        {
            switch (xmlType)
            {
                case XmlType.Element:
                    return new XmlSerializationTags.CustomElement($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
                case XmlType.Content:
                    return new XmlSerializationTags.CustomContent($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
                default:
                    return new XmlSerializationTags.CustomComplexType($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
            }
        }

        public string erlXmlElementNameAtom => Helper.AtomName(xmlElementName);
    }

    public partial class RecordField
    {
        public SerializationTag erlXmlParseTag => XmlSerialization.XmlTag(NonOptType, this, this);
        public SerializationTag erlXmlPackTag => XmlSerialization.XmlTag(xmlAttribute ? NonOptType : Type, this, this);
    }

    public partial class UnionClause
    {
        public SerializationTag erlXmlTag => XmlSerialization.XmlTag(Type, this, this);
    }

    public partial class UnionForm
    {
        protected override SerializationTag erlXmlGenTag(Statement referrer, Statement field)
        {
            switch (xmlType)
            {
                case XmlType.Element:
                    return new XmlSerializationTags.CustomElement($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
                case XmlType.Content:
                    return new XmlSerializationTags.CustomContent($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
                default:
                    return new XmlSerializationTags.CustomComplexType($"{Module.erlName}:{erlXmlPackerName}", $"{Module.erlName}:{erlXmlParserName}", erlArgTags);
            }
        }
    }

    public partial class DefineForm
    {
        protected override SerializationTag erlXmlGenTag(Statement referrer, Statement field) => XmlSerialization.XmlTag(Type, this, field);
    }
}
