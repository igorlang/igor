using Igor.Erlang.Strings;

namespace Igor.Erlang.AST
{
    public partial class TypeForm
    {
        private string erlStringUserPacker => Attribute(ErlAttributes.StringPacker, null);
        private string erlStringUserParser => Attribute(ErlAttributes.StringParser, null);
        private bool erlStringUserSerializer => erlStringUserParser != null && erlStringUserPacker != null;

        public bool erlStringGenSerializer => stringEnabled && !erlStringUserSerializer && (erlPrimitiveType == PrimitiveType.None);

        public string erlStringGenPackName => $"{erlName}_to_string";
        public string erlStringGenParseName => $"{erlName}_from_string";

        protected virtual SerializationTag erlStringGenTag(Statement referrer)
        {
            Error($"String serialization is not supported for this type but required for {referrer}. Consider providing custom serializer using string.packer and string.parser attributes.");
            return SerializationTags.String;
        }

        public SerializationTag erlStringTag(Statement referrer)
        {
            if (!stringEnabled)
                Error($"String serialization is not enabled but required for {referrer}. Use string.enabled attribute to enable string serialization.");

            if (erlStringUserSerializer)
                return new SerializationTags.Custom(erlStringUserPacker, erlStringUserParser, erlArgTags);
            else if (erlPrimitiveType != PrimitiveType.None)
                return new SerializationTags.Primitive(erlPrimitiveType);
            else
                return erlStringGenTag(referrer);
        }
    }

    public partial class GenericType
    {
        public SerializationTag erlStringTag(Statement referrer) => Prototype.erlStringTag(referrer).Instantiate(PrepareArgs(StringSerialization.StringTag, referrer));
    }

    public partial class EnumForm
    {
        protected override SerializationTag erlStringGenTag(Statement referrer) => new SerializationTags.Custom($"{Module.erlName}:{erlStringGenPackName}", $"{Module.erlName}:{erlStringGenParseName}", erlArgTags);
    }

    partial class DefineForm
    {
        protected override SerializationTag erlStringGenTag(Statement referrer) => StringSerialization.StringTag(Type, this);
    }
}
