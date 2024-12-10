using Igor.Text;

namespace Igor.TypeScript.AST
{
    public partial class GenericArgument
    {
        public string tsJsonSerializerType => $"Json.Serialization.IJsonSerializer<{tsName}>";
    }

    public partial class TypeForm
    {
        public string tsJsonCustomSerializerInstance => Attribute(TsAttributes.JsonSerializer, null);

        public string tsJsonGeneratedSerializerClass => tsName;

        public bool tsJsonGenerateSerializer => (tsEnabled && jsonEnabled && tsJsonCustomSerializerInstance == null);

        public string tsJsonSerializerInstance(string ns, TsType[] genericArgs)
        {
            if (!jsonEnabled)
                throw new EFormatDisabled(Location, Name, "json");
            else if (tsJsonCustomSerializerInstance != null)
                return TsName.RelativeName(tsJsonCustomSerializerInstance, ns);
            else if (IsGeneric)
            {
                var args = genericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                var serArgs = genericArgs.JoinStrings(", ", arg => arg.jsonSerializer(ns));
                return $"{TsName.RelativeName(tsNamespace, tsJsonGeneratedSerializerClass, ns)}.instanceJsonSerializer<{args}>({serArgs})";
            }
            else
                return $"{TsName.RelativeName(tsNamespace, tsJsonGeneratedSerializerClass, ns)}";
        }
    }
}
