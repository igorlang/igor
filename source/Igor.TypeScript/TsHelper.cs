using Igor.TypeScript.AST;
using System.Collections.Generic;
using System.Linq;

namespace Igor.TypeScript
{
    public static class Helper
    {
        public static TsType TargetType(IType astType, Statement host = null)
        {
            switch (astType)
            {
                case BuiltInType.Bool _: return new TsBoolType();
                case BuiltInType.Integer t: return new TsNumberType(Primitive.FromInteger(t.Type));
                case BuiltInType.Float t: return new TsNumberType(Primitive.FromFloat(t.Type));
                case BuiltInType.String _: return new TsStringType();
                case BuiltInType.Binary _: return new TsStringType();
                case BuiltInType.Atom _: return new TsStringType();
                case BuiltInType.Json _: return new TsJsonType();
                case BuiltInType.List t: return new TsArrayType(TargetType(t.ItemType), host);
                case BuiltInType.Dict t: return new TsDictType(TargetType(t.KeyType), TargetType(t.ValueType));
                case BuiltInType.Optional t: return new TsOptionalType(TargetType(t.ItemType, host));
                case BuiltInType.Flags t: return ((EnumForm)t.ItemType).tsFlagsType;
                case TypeForm form: return form.tsType;
                case GenericType type:
                    {
                        var args = type.Prototype.Args.ZipDictionary(type.Args.Select(t => TargetType(t)));
                        return TargetType(type.Prototype).Substitute(args);
                    }
                case GenericArgument type: return type.tsType;
                default: throw new EUnknownType(astType.ToString());
            }
        }

        public static TsType TargetInterface(IInterface astInterface)
        {
            switch (astInterface)
            {
                case InterfaceForm form: return form.tsType;
                case GenericInterface type:
                    {
                        var args = type.Prototype.Args.ZipDictionary(type.Args.Select(t => TargetType(t)));
                        return TargetType(type.Prototype).Substitute(args);
                    }
                default: throw new EUnknownType(astInterface.ToString());
            }
        }

        public static string PrimitiveTypeString(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool:
                    return "boolean";
                case PrimitiveType.SByte:
                case PrimitiveType.Byte:
                case PrimitiveType.Short:
                case PrimitiveType.UShort:
                case PrimitiveType.Int:
                case PrimitiveType.UInt:
                case PrimitiveType.Long:
                case PrimitiveType.ULong:
                case PrimitiveType.Float:
                case PrimitiveType.Double:
                    return "number";
                case PrimitiveType.String:
                case PrimitiveType.Binary:
                case PrimitiveType.Atom:
                    return "string";
                case PrimitiveType.Json:
                    return "Igor.Json.JsonValue";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string JsonPrimitiveSerializer(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool:
                    return "Igor.Json.Bool";
                case PrimitiveType.SByte:
                case PrimitiveType.Byte:
                case PrimitiveType.Short:
                case PrimitiveType.UShort:
                case PrimitiveType.Int:
                case PrimitiveType.UInt:
                case PrimitiveType.Long:
                case PrimitiveType.ULong:
                case PrimitiveType.Float:
                case PrimitiveType.Double:
                    return "Igor.Json.Number";
                case PrimitiveType.String:
                case PrimitiveType.Binary:
                case PrimitiveType.Atom:
                    return "Igor.Json.String";
                case PrimitiveType.Json:
                    return "Igor.Json.Json";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string PrimitiveFromJson(PrimitiveType type, string value)
        {
            switch (type)
            {
                case PrimitiveType.Bool:
                case PrimitiveType.SByte:
                case PrimitiveType.Byte:
                case PrimitiveType.Short:
                case PrimitiveType.UShort:
                case PrimitiveType.Int:
                case PrimitiveType.UInt:
                case PrimitiveType.Long:
                case PrimitiveType.ULong:
                case PrimitiveType.Float:
                case PrimitiveType.Double:
                case PrimitiveType.String:
                case PrimitiveType.Binary:
                case PrimitiveType.Atom:
                    return TsCodeUtils.As(value, PrimitiveTypeString(type));
                case PrimitiveType.Json:
                    return value;
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string ShadowName(string word)
        {
            if (IsReservedWord(word))
                return "_" + word;
            else
                return word;
        }

        private static readonly HashSet<string> ReservedWords = new HashSet<string>
        {
            "break", "case", "catch", "class", "const", "continue", "debugger", "default", "delete", "do", "else",
            "enum", "export", "extends", "false", "finally", "for", "function", "if", "import", "in", "instanceof",
            "new", "null", "return", "super", "switch", "this", "throw", "true", "try", "typeof", "var", "void", "while", "with",
            "as", "implements", "interface", "let", "package", "private", "protected", "public", "static", "yield"
        };

        public static bool IsReservedWord(string word)
        {
            return ReservedWords.Contains(word);
        }

        public static string EscapeString(string value)
        {
            return value.Replace("'", "\\'");
        }
    }
}
