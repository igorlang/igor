namespace Igor.CSharp.AST
{
    public partial class GenericArgument
    {
        public string csJsonSerializerType => $"Json.Serialization.IJsonSerializer<{csName}>";
    }

    public partial class Form
    {
        public string csJsonNamespace => Attribute(CsAttributes.JsonNamespace, csNamespace);
    }

    public partial class TypeForm
    {
        public string csJsonCustomSerializerInstance => Attribute(CsAttributes.JsonSerializer, null);

        public string csJsonGeneratedSerializerClass => $"{csName}JsonSerializer";

        public bool csJsonGenerateSerializer => csEnabled && jsonEnabled && csJsonCustomSerializerInstance == null;

        public string csJsonSerializerInstance(string ns)
        {
            if (!jsonEnabled)
                throw new EFormatDisabled(Location, Name, "json");
            else if (csJsonCustomSerializerInstance != null)
                return CsName.RelativeName(csJsonCustomSerializerInstance, ns);
            else if (IsGeneric)
                return $"new {CsName.RelativeName(csJsonNamespace, csJsonGeneratedSerializerClass, ns)}";
            else
                return $"{CsName.RelativeName(csJsonNamespace, csJsonGeneratedSerializerClass, ns)}.Instance";
        }
    }

    public partial class RecordField
    {
        public string csJsonSerializer => IsOptional ? csType.nonOptType.jsonSerializer(Struct.csJsonNamespace) : csType.jsonSerializer(Struct.csJsonNamespace);
        public string csJsonWriteValue => csType.writeValue(csName);

        public string csJsonRequire(string paramName)
        {
            if (csCanBeNull && !IsOptional)
                return
$@"    if (value.{csName} == null)
        throw new System.ArgumentException(""Required property {csName} is null"", {CsVersion.NameOf(paramName)});";
            else
                return null;
        }
    }

    public partial class StructForm
    {
        public bool csJsonSerializable => Attribute(CsAttributes.JsonSerializable, false);
    }
}
