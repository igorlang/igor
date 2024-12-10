using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor
{
    public enum IntegerType
    {
        SByte = 2,
        Byte = 3,
        Short = 4,
        UShort = 5,
        Int = 6,
        UInt = 7,
        Long = 8,
        ULong = 9,
    }

    public enum FloatType
    {
        Float = 10,
        Double = 11,
    }

    public enum PrimitiveType
    {
        None = 0,
        Bool = 1,
        SByte = 2,
        Byte = 3,
        Short = 4,
        UShort = 5,
        Int = 6,
        UInt = 7,
        Long = 8,
        ULong = 9,
        Float = 10,
        Double = 11,
        String = 12,
        Binary = 13,
        Atom = 14,
        Json = 15,
    }

    public enum NumberBase
    {
        Decimal,
        Hex,
    }

    public static class Primitive
    {
        public static PrimitiveType FromString(string type)
        {
            switch (type)
            {
                case "bool":
                    return PrimitiveType.Bool;
                case "sbyte":
                    return PrimitiveType.SByte;
                case "byte":
                    return PrimitiveType.Byte;
                case "short":
                    return PrimitiveType.Short;
                case "ushort":
                    return PrimitiveType.UShort;
                case "int":
                    return PrimitiveType.Int;
                case "uint":
                    return PrimitiveType.UInt;
                case "long":
                    return PrimitiveType.Long;
                case "ulong":
                    return PrimitiveType.ULong;
                case "float":
                    return PrimitiveType.Float;
                case "double":
                    return PrimitiveType.Double;
                case "string":
                    return PrimitiveType.String;
                case "binary":
                    return PrimitiveType.Binary;
                case "atom":
                    return PrimitiveType.Atom;
                case "json":
                default:
                    return PrimitiveType.Json;
            }
        }

        public static PrimitiveType FromInteger(IntegerType type) => (PrimitiveType)type;

        public static PrimitiveType FromFloat(FloatType type) => (PrimitiveType)type;

        public static IntegerType BestIntegerType(IEnumerable<long> values)
        {
            var zero = new long[] { 0L };
            var max = values.Concat(zero).Max();
            var min = values.Concat(zero).Min();
            if (!values.Any())
                return IntegerType.Int;
            else if ((min >= byte.MinValue) && (max <= byte.MaxValue))
                return IntegerType.Byte;
            else if ((min >= ushort.MinValue) && (max <= ushort.MaxValue))
                return IntegerType.UShort;
            else if ((min >= uint.MinValue) && (max <= uint.MaxValue))
                return IntegerType.UInt;
            else if ((min >= sbyte.MinValue) && (max <= sbyte.MaxValue))
                return IntegerType.SByte;
            else if ((min >= short.MinValue) && (max <= short.MaxValue))
                return IntegerType.Short;
            else if ((min >= int.MinValue) && (max <= int.MaxValue))
                return IntegerType.Int;
            else
                return IntegerType.Long;
        }

        public static long MinValue(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.SByte: return sbyte.MinValue;
                case IntegerType.Byte: return byte.MinValue;
                case IntegerType.Short: return short.MinValue;
                case IntegerType.UShort: return ushort.MinValue;
                case IntegerType.Int: return int.MinValue;
                case IntegerType.UInt: return uint.MinValue;
                case IntegerType.Long: return long.MinValue;
                case IntegerType.ULong: return (long)ulong.MinValue;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static long MaxValue(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.SByte: return sbyte.MaxValue;
                case IntegerType.Byte: return byte.MaxValue;
                case IntegerType.Short: return short.MaxValue;
                case IntegerType.UShort: return ushort.MaxValue;
                case IntegerType.Int: return int.MaxValue;
                case IntegerType.UInt: return uint.MaxValue;
                case IntegerType.Long: return long.MaxValue;
                case IntegerType.ULong: return long.MaxValue; // ulong.MaxValue is too big
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
