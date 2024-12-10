namespace Igor.Elixir.AST
{
    public partial class TypeForm
    {
        private string exStringCustom => Attribute(ExAttributes.StringCustom, null);

        public bool exStringGenSerializer => stringEnabled && exStringCustom == null;

        public string exStringGenPackName => $"{exName}.to_string";
        public string exStringGenParseName => $"{exName}.from_string";

        protected virtual SerializationTag exStringGenTag(Statement referrer)
        {
            Error($"String serialization is not supported for this type but required for {referrer}. Consider providing custom serializer using string.custom attribute.");
            return SerializationTags.String;
        }

        public SerializationTag exStringTag(Statement referrer)
        {
            if (!stringEnabled)
                Error($"String serialization is not enabled but required for {referrer}. Use string.enabled attribute to enable string serialization.");

            if (exStringCustom != null)
                return new SerializationTags.Custom(exStringCustom, exArgTags);
            else
                return exStringGenTag(referrer);
        }
    }

    public partial class GenericType
    {
        public SerializationTag exStringTag(Statement referrer) => Prototype.exStringTag(referrer).Instantiate(PrepareArgs(StringSerialization.StringTag, referrer));
    }

    public partial class EnumForm
    {
        protected override SerializationTag exStringGenTag(Statement referrer) => new SerializationTags.Custom($"{Module.exName}.{exName}", exArgTags);
    }

    partial class DefineForm
    {
        protected override SerializationTag exStringGenTag(Statement referrer) => StringSerialization.StringTag(Type, this);
    }
}
