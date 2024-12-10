using Igor.Python.Json;

namespace Igor.Python.AST
{
    public partial class TypeForm
    {
        public bool pyJsonGenerateSerializer => pyEnabled && jsonEnabled && pyJsonUserSerializer == null;

        private string pyJsonUserSerializer => Attribute(PythonAttributes.JsonSerializer, null);

        protected virtual ISerializationTag pyJsonGenTag => new SerializationTag.Custom(pyName, pyArgTags);

        public ISerializationTag pyJsonTag(Statement referrer)
        {
            if (!jsonEnabled)
                Error($"JSON serialization is not enabled but required for {referrer}. Use json.enabled attribute to enable JSON serialization.");

            if (pyJsonUserSerializer != null)
                return new SerializationTag.Custom(pyJsonUserSerializer, pyArgTags);
            else
                return pyJsonGenTag;
        }
    }

    public partial class GenericType
    {
        public ISerializationTag pyJsonTag(Statement referrer) => Prototype.pyJsonTag(referrer).Instantiate(PrepareArgs(JsonSerialization.Tag, referrer));
    }

    public partial class EnumForm
    {
    }

    public partial class RecordField
    {
        public ISerializationTag pyJsonTag => JsonSerialization.Tag(Type, this);
    }

    partial class DefineForm
    {
        protected override ISerializationTag pyJsonGenTag => pyEnumAlias != null ? base.pyJsonGenTag : JsonSerialization.Tag(Type, this);
    }

    public partial class FunctionArgument
    {
        public ISerializationTag pyJsonTag => JsonSerialization.Tag(Type, Function);
    }
}
