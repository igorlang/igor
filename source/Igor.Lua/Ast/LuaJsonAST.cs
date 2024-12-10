using Igor.Lua.Json;

namespace Igor.Lua.AST
{
    public partial class TypeForm
    {
        public bool luaJsonGenerateSerializer => luaEnabled && jsonEnabled && luaJsonUserSerializer == null;

        private string luaJsonUserSerializer => Attribute(LuaAttributes.JsonSerializer, null);

        protected virtual ISerializationTag luaJsonGenTag => luaNamespace == null ? new SerializationTag.Custom(luaName, luaArgTags) : new SerializationTag.Custom($"{luaNamespace}.{luaName}", luaArgTags);

        public ISerializationTag luaJsonTag(Statement referrer)
        {
            if (!jsonEnabled)
                Error($"JSON serialization is not enabled but required for {referrer}. Use json.enabled attribute to enable JSON serialization.");

            if (luaJsonUserSerializer != null)
                return new SerializationTag.Custom(luaJsonUserSerializer, luaArgTags);
            else
                return luaJsonGenTag;
        }
    }

    public partial class GenericType
    {
        public ISerializationTag luaJsonTag(Statement referrer) => Prototype.luaJsonTag(referrer).Instantiate(PrepareArgs(JsonSerialization.Tag, referrer));
    }

    public partial class EnumForm
    {
    }

    public partial class RecordField
    {
        public ISerializationTag luaJsonTag => JsonSerialization.Tag(Type, this);
    }

    public partial class VariantForm
    {
        public string luaJsonDeserializerLookupTable => $"{luaName.ToUpperInvariant()}_FROM_JSON_LOOKUP";
        public string luaJsonSerializerLookupTable => $"{luaName.ToUpperInvariant()}_TO_JSON_LOOKUP";
    }

    partial class DefineForm
    {
        protected override ISerializationTag luaJsonGenTag => luaEnumAlias != null ? base.luaJsonGenTag : JsonSerialization.Tag(Type, this);
    }

    public partial class FunctionArgument
    {
        public ISerializationTag luaJsonTag => JsonSerialization.Tag(Type, Function);
    }
}
