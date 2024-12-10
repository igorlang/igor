using Igor.Text;
using System.Linq;

namespace Igor.Elixir.AST
{
    public partial class TypeForm
    {
        private string exJsonCustom => Attribute(ExAttributes.JsonCustom, null);
        private bool exJsonCompatible => Attribute(ExAttributes.JsonCompatible, false);

        public bool exJsonIsSerializerGenerated => jsonEnabled && exJsonCustom == null && !exJsonCompatible;

        public string exJsonGenPackerName => $"{exName}.to_json!";
        public string exJsonGenParserName => $"{exName}.from_json!";

        protected virtual SerializationTag exJsonGenTag => new SerializationTags.Custom($"{Module.exName}.{exName}", exArgTags);

        public SerializationTag exJsonTag(Statement referrer)
        {
            if (!jsonEnabled)
                Error($"JSON serialization is not enabled but required for {referrer}. Use json.enabled attribute to enable JSON serialization.");

            if (exJsonCustom != null)
                return new SerializationTags.Custom(exJsonCustom, exArgTags);
            else if (exJsonCompatible)
                return new SerializationTags.Json();
            else
                return exJsonGenTag;
        }
    }

    public partial class GenericType
    {
        public SerializationTag exJsonTag(Statement referrer) => Prototype.exJsonTag(referrer).Instantiate(PrepareArgs(JsonSerialization.JsonTag, referrer));
    }

    public partial class EnumForm
    {
    }

    public partial class RecordField
    {
        public SerializationTag exJsonTag => Struct.IsPatch ? JsonSerialization.JsonTag(Type, this) : JsonSerialization.JsonTag(NonOptType, this);
    }

    public partial class UnionClause
    {
        public SerializationTag exJsonTag => JsonSerialization.JsonTag(Type, this);
    }

    partial class DefineForm
    {
        protected override SerializationTag exJsonGenTag => JsonSerialization.JsonTag(Type, this);
    }
    /*
    public partial class ServiceFunction
    {
        internal string exJsonMaybeParams => Arguments.Any() ? @", <<""params"">> := Params" : "";
        internal string exJsonMaybeResult => ReturnArguments.Any() ? @", <<""result"">> := Result" : @", <<""result"">> := []";

        internal string exJsonParseParams =>
            Arguments.Any() ?
$@"    [{Arguments.Select(arg => arg.exVarName + "Json").JoinStrings(", ")}] = Params,
{Arguments.Select(arg => $"    {arg.exVarName} = {JsonSerialization.ParseJson(arg.exJsonTag, arg.exVarName + "Json")},").JoinLines()}"
            : "";

        internal string exJsonParseResult =>
            ReturnArguments.Any() ?
$@"    [{ReturnArguments.Select(arg => arg.exVarName + "Json").JoinStrings(", ")}] = Result,
{ReturnArguments.Select(arg => $"    {arg.exVarName} = {JsonSerialization.ParseJson(arg.exJsonTag, arg.exVarName + "Json")},").JoinLines()}"
            : "";
    }

    public partial class FunctionArgument
    {
        public SerializationTag exJsonTag => JsonSerialization.JsonTag(Type, Function);
    }*/
}
