using Igor.Erlang.Json;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.AST
{
    public partial class TypeForm
    {
        private string erlJsonUserPacker => Attribute(ErlAttributes.JsonPacker, null);
        private string erlJsonUserParser => Attribute(ErlAttributes.JsonParser, null);
        private bool erlJsonUserSerializer => erlJsonUserParser != null && erlJsonUserPacker != null;
        private bool erlJsonCompatible => Attribute(ErlAttributes.JsonCompatible, false);

        public bool erlJsonIsSerializerGenerated => jsonEnabled && !erlJsonUserSerializer && (erlPrimitiveType == PrimitiveType.None) && !erlJsonCompatible;

        public string erlJsonGenPackerName => $"{erlName}_to_json";
        public string erlJsonGenParserName => $"{erlName}_from_json";

        protected virtual SerializationTag erlJsonGenTag => new SerializationTags.Custom($"{Module.erlName}:{erlJsonGenPackerName}", $"{Module.erlName}:{erlJsonGenParserName}", erlArgTags);

        public SerializationTag erlJsonTag(Statement referrer)
        {
            if (!jsonEnabled)
                Error($"JSON serialization is not enabled but required for {referrer}. Use json.enabled attribute to enable JSON serialization.");

            if (erlJsonUserSerializer)
                return new SerializationTags.Custom(erlJsonUserPacker, erlJsonUserParser, erlArgTags);
            else if (erlJsonCompatible)
                return new SerializationTags.Json();
            else if (erlPrimitiveType != PrimitiveType.None)
                return new SerializationTags.Primitive(erlPrimitiveType);
            else
                return erlJsonGenTag;
        }
    }

    public partial class GenericType
    {
        public SerializationTag erlJsonTag(Statement referrer) => Prototype.erlJsonTag(referrer).Instantiate(PrepareArgs(JsonSerialization.JsonTag, referrer));
    }

    public partial class EnumForm
    {
    }

    public partial class RecordField
    {
        public SerializationTag erlJsonTag => JsonSerialization.JsonTag(NonOptType, this).NullableIf(Struct.IsPatch && Type is BuiltInType.Optional);
        public string erlJsonName => $@"<<""{jsonKey}"">>";
    }

    partial class DefineForm
    {
        protected override SerializationTag erlJsonGenTag => JsonSerialization.JsonTag(Type, this);
    }

    public partial class ServiceFunction
    {
        internal string erlJsonMaybeParams => Arguments.Any() ? @", <<""params"">> := Params" : "";
        internal string erlJsonMaybeResult => ReturnArguments.Any() ? @", <<""result"">> := Result" : @", <<""result"">> := []";

        internal string erlJsonParseParams =>
            Arguments.Any() ?
$@"    [{Arguments.Select(arg => arg.erlVarName + "Json").JoinStrings(", ")}] = Params,
{Arguments.Select(arg => $"    {arg.erlVarName} = {JsonSerialization.ParseJson(arg.erlJsonTag, arg.erlVarName + "Json")},").JoinLines()}"
            : "";

        internal string erlJsonParseResult =>
            ReturnArguments.Any() ?
$@"    [{ReturnArguments.Select(arg => arg.erlVarName + "Json").JoinStrings(", ")}] = Result,
{ReturnArguments.Select(arg => $"    {arg.erlVarName} = {JsonSerialization.ParseJson(arg.erlJsonTag, arg.erlVarName + "Json")},").JoinLines()}"
            : "";
    }

    public partial class FunctionArgument
    {
        public SerializationTag erlJsonTag => JsonSerialization.JsonTag(Type, Function);
    }
}
