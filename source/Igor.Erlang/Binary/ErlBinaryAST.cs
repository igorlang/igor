using Igor.Erlang.Binary;
using System.Linq;

namespace Igor.Erlang.AST
{
    public partial class Form
    {
    }

    public partial class TypeForm
    {
        private string erlBinaryUserPacker => Attribute(ErlAttributes.BinaryPacker, null);
        private string erlBinaryUserParser => Attribute(ErlAttributes.BinaryParser, null);
        private bool erlBinaryUserSerializer => erlBinaryUserParser != null && erlBinaryUserPacker != null;

        public bool erlBinaryIsSerializerGenerated => binaryEnabled && !erlBinaryUserSerializer && (erlPrimitiveType == PrimitiveType.None);

        public string erlBinaryGenPackerName => $"{erlName}_to_iodata";
        public string erlBinaryGenParserName => $"{erlName}_from_binary";

        protected virtual SerializationTag erlBinaryGenTag => new SerializationTags.Custom($"{Module.erlName}:{erlBinaryGenPackerName}", $"{Module.erlName}:{erlBinaryGenParserName}", erlArgTags);

        public SerializationTag erlBinaryTag(Statement referrer)
        {
            if (!binaryEnabled)
                Error($"Binary serialization is not enabled but required for {referrer}. Use binary.enabled attribute to enable binary serialization.");

            if (erlBinaryUserSerializer)
                return new SerializationTags.Custom(erlBinaryUserPacker, erlBinaryUserParser, erlArgTags);
            else if (erlPrimitiveType != PrimitiveType.None)
                return new SerializationTags.Primitive(erlPrimitiveType);
            else
                return erlBinaryGenTag;
        }
    }

    public partial class GenericType
    {
        public SerializationTag erlBinaryTag(Statement referrer) => Prototype.erlBinaryTag(referrer).Instantiate(PrepareArgs(BinarySerialization.BinaryTag, referrer));
    }

    public partial class EnumForm
    {
        public string erlIntTag => Helper.PrimitiveTag(Primitive.FromInteger(IntType));
        public string erlPackToIntName => $"{erlName}_to_integer";
        public string erlParseFromIntName => $"{erlName}_from_integer";
        protected override SerializationTag erlBinaryGenTag => new SerializationTags.Enum(IntType, $"{Module.erlName}:{erlPackToIntName}", $"{Module.erlName}:{erlParseFromIntName}");
    }

    public partial class RecordField
    {
        public SerializationTag erlBinaryTag => BinarySerialization.BinaryTag(Struct.binaryHeader ? NonOptType : Type, this);
    }

    public partial class StructForm
    {
        public int erlBinaryBitPadding => (8 - binarySerializedFields.Count(f => f.IsOptional) % 8) % 8;
    }

    partial class DefineForm
    {
        protected override SerializationTag erlBinaryGenTag => BinarySerialization.BinaryTag(Type, this);
    }

    partial class UnionForm
    {
    }

    public partial class FunctionArgument
    {
        public SerializationTag erlBinaryTag => BinarySerialization.BinaryTag(Type, Function);
    }
}
