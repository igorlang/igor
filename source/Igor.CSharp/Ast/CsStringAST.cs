namespace Igor.CSharp.AST
{
    public partial class GenericArgument
    {
        public string csStringSerializerType => $"Igor.Serialization.IStringSerializer<{csName}>";
    }

    public partial class TypeForm
    {
        public string csStringCustomSerializerInstance => Attribute(CsAttributes.StringSerializer, null);
        public string csStringNamespace => Attribute(CsAttributes.StringNamespace, csNamespace);

        public string csStringGeneratedSerializerClass => $"{csName}StringSerializer";

        protected virtual bool csStringSupportedInherently => false;

        public bool csStringSupported => csStringSupportedInherently || csStringCustomSerializerInstance != null;

        public bool csStringGenerateSerializer => (csEnabled && stringEnabled && csStringCustomSerializerInstance == null && csStringSupportedInherently);

        public string csStringSerializerInstance(string ns)
        {
            if (!stringEnabled)
                throw new EFormatDisabled(Location, Name, "string");
            else if (csStringCustomSerializerInstance != null)
                return CsName.RelativeName(csStringCustomSerializerInstance, ns);
            else if (IsGeneric)
                return $"new {CsName.RelativeName(csStringNamespace, csStringGeneratedSerializerClass, ns)}";
            else
                return $"{CsName.RelativeName(csStringNamespace, csStringGeneratedSerializerClass, ns)}.Instance";
        }
    }

    public partial class EnumForm
    {
        protected override bool csStringSupportedInherently => true;
    }

}
