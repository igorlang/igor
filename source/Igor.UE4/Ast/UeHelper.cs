using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4.AST
{
    public static class Helper
    {
        public static string UeIntType(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.SByte:
                    return "int8";
                case IntegerType.Byte:
                    return "uint8";
                case IntegerType.Short:
                    return "int16";
                case IntegerType.UShort:
                    return "uint16";
                case IntegerType.Int:
                default:
                    return "int32";
                case IntegerType.UInt:
                    return "uint32";
                case IntegerType.Long:
                    return "int64";
                case IntegerType.ULong:
                    return "uint64";
            }
        }

        public static string PrimitiveTypeString(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "bool";
                case PrimitiveType.SByte: return "int8";
                case PrimitiveType.Byte: return "uint8";
                case PrimitiveType.Short: return "int16";
                case PrimitiveType.UShort: return "uint16";
                case PrimitiveType.Int: return "int32";
                case PrimitiveType.UInt: return "uint32";
                case PrimitiveType.Long: return "int64";
                case PrimitiveType.ULong: return "uint64";
                case PrimitiveType.Float: return "float";
                case PrimitiveType.Double: return "double";
                case PrimitiveType.String: return "FString";
                case PrimitiveType.Binary: return "FBufferArchive";
                case PrimitiveType.Atom: return "FName";
                case PrimitiveType.Json: return "TSharedPtr<FJsonValue>";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static UeType TargetType(IType astType)
        {
            switch (astType)
            {
                case BuiltInType.Bool _: return new UeBoolType();
                case BuiltInType.Integer i: return new UeIntegerType(i.Type);
                case BuiltInType.Float f: return new UeFloatType(f.Type);
                case BuiltInType.String _: return new UeStringType();
                case BuiltInType.Binary _: return new UeBinaryType();
                case BuiltInType.Atom _: return new UeNameType();
                case BuiltInType.Json _: return new UeJsonType();
                case BuiltInType.List l: return new UeListType(TargetType(l.ItemType));
                case BuiltInType.Dict d: return new UeDictType(TargetType(d.KeyType), TargetType(d.ValueType));
                case TypeForm form: return form.ueType;
                case BuiltInType.Optional opt: return new UeOptionalType(TargetType(opt.ItemType));
                case GenericArgument genericArgument: return new UeGenericArgument(genericArgument);
                case GenericType genericType:
                    {
                        var args = genericType.Prototype.Args.ZipDictionary(genericType.Args.Select(TargetType));
                        return TargetType(genericType.Prototype).Substitute(args);
                    }
                default: throw new EUnknownType(astType.ToString());
            }
        }
    }
}
