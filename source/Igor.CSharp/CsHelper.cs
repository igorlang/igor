using Igor.CSharp.AST;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Igor.CSharp
{
    public struct TypeContext
    {
        public ListTypeImplementation ListImplementation { get; set; }

        public DictTypeImplementation DictImplementation { get; set; }
    }

    public static class Helper
    {
        public static CsType TargetType(IType astType, TypeContext typeContext)
        {
            switch (astType)
            {
                case BuiltInType.Bool _: return new CsBoolType();
                case BuiltInType.Integer t: return new CsIntegerType(t.Type);
                case BuiltInType.Float t: return new CsFloatType(t.Type);
                case BuiltInType.String _: return new CsStringType();
                case BuiltInType.Binary _: return new CsBinaryType();
                case BuiltInType.Atom _: return new CsStringType();
                case BuiltInType.Json _: return new CsJsonType();
                case BuiltInType.List t:
                    switch (typeContext.ListImplementation)
                    {
                        case ListTypeImplementation.ReadOnly:
                            return new CsReadOnlyListType(TargetType(t.ItemType, typeContext));
                        default:
                            return new CsListType(TargetType(t.ItemType, typeContext));
                    }
                case BuiltInType.Dict t:
                    switch (typeContext.DictImplementation)
                    {
                        case DictTypeImplementation.ReadOnly:
                            return new CsReadOnlyDictType(TargetType(t.KeyType, typeContext), TargetType(t.ValueType, typeContext));
                        default:
                            return new CsDictType(TargetType(t.KeyType, typeContext), TargetType(t.ValueType, typeContext));
                    }
                case BuiltInType.Optional t:
                    {
                        var inner = TargetType(t.ItemType, typeContext);
                        return inner.isReference ? (CsType)new CsOptionalType(inner) : new CsNullableType(inner);
                    }
                case BuiltInType.Flags t: return ((EnumForm)t.ItemType).csFlagsType;
                case BuiltInType.OneOf oneOf: return new CsOneOfType(oneOf.Types.Select(t => TargetType(t, typeContext)).ToList());
                case TypeForm form: return form.csType;
                case GenericType type:
                    {
                        var args = type.Prototype.Args.ZipDictionary(type.Args.Select(t => TargetType(t, typeContext)));
                        return TargetType(type.Prototype, typeContext).Substitute(args);
                    }
                case GenericArgument type: return type.csType;
                default: throw new EUnknownType(astType.ToString());
            }
        }

        public static CsType TargetInterface(IInterface astType)
        {
            switch (astType)
            {
                case InterfaceForm form: return form.csType;
                case GenericInterface type:
                    {
                        var args = type.Prototype.Args.ZipDictionary(type.Args.Select(t => TargetType(t, type.Prototype.csTypeContext)));
                        return TargetType(type.Prototype, type.Prototype.csTypeContext).Substitute(args);
                    }
                default: throw new EUnknownType(astType.ToString());
            }
        }

        public static string PrimitiveTypeString(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "bool";
                case PrimitiveType.SByte: return "sbyte";
                case PrimitiveType.Byte: return "byte";
                case PrimitiveType.Short: return "short";
                case PrimitiveType.UShort: return "ushort";
                case PrimitiveType.Int: return "int";
                case PrimitiveType.UInt: return "uint";
                case PrimitiveType.Long: return "long";
                case PrimitiveType.ULong: return "ulong";
                case PrimitiveType.Float: return "float";
                case PrimitiveType.Double: return "double";
                case PrimitiveType.String: return "string";
                case PrimitiveType.Binary: return "byte[]";
                case PrimitiveType.Atom: return "string";
                case PrimitiveType.Json: return "Json.ImmutableJson";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static bool PrimitiveIsReference(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.String: return true;
                case PrimitiveType.Binary: return true;
                case PrimitiveType.Atom: return true;
                case PrimitiveType.Json: return true;
                default: return false;
            }
        }

        public static string PrimitiveSerializer(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "IgorSerializer.Bool";
                case PrimitiveType.SByte: return "IgorSerializer.SByte";
                case PrimitiveType.Byte: return "IgorSerializer.Byte";
                case PrimitiveType.Short: return "IgorSerializer.Short";
                case PrimitiveType.UShort: return "IgorSerializer.UShort";
                case PrimitiveType.Int: return "IgorSerializer.Int";
                case PrimitiveType.UInt: return "IgorSerializer.UInt";
                case PrimitiveType.Long: return "IgorSerializer.Long";
                case PrimitiveType.ULong: return "IgorSerializer.ULong";
                case PrimitiveType.Float: return "IgorSerializer.Float";
                case PrimitiveType.Double: return "IgorSerializer.Double";
                case PrimitiveType.String: return "IgorSerializer.String";
                case PrimitiveType.Binary: return "IgorSerializer.Binary";
                case PrimitiveType.Atom: return "IgorSerializer.String";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string JsonPrimitiveSerializer(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "JsonSerializer.Bool";
                case PrimitiveType.SByte: return "JsonSerializer.SByte";
                case PrimitiveType.Byte: return "JsonSerializer.Byte";
                case PrimitiveType.Short: return "JsonSerializer.Short";
                case PrimitiveType.UShort: return "JsonSerializer.UShort";
                case PrimitiveType.Int: return "JsonSerializer.Int";
                case PrimitiveType.UInt: return "JsonSerializer.UInt";
                case PrimitiveType.Long: return "JsonSerializer.Long";
                case PrimitiveType.ULong: return "JsonSerializer.ULong";
                case PrimitiveType.Float: return "JsonSerializer.Float";
                case PrimitiveType.Double: return "JsonSerializer.Double";
                case PrimitiveType.String: return "JsonSerializer.String";
                case PrimitiveType.Binary: return "JsonSerializer.Binary";
                case PrimitiveType.Atom: return "JsonSerializer.String";
                case PrimitiveType.Json: return "JsonSerializer.Json";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string StringPrimitiveSerializer(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "Igor.Serialization.StringSerializers.Bool";
                case PrimitiveType.SByte: return "Igor.Serialization.StringSerializers.SByte";
                case PrimitiveType.Byte: return "Igor.Serialization.StringSerializers.Byte";
                case PrimitiveType.Short: return "Igor.Serialization.StringSerializers.Short";
                case PrimitiveType.UShort: return "Igor.Serialization.StringSerializers.UShort";
                case PrimitiveType.Int: return "Igor.Serialization.StringSerializers.Int";
                case PrimitiveType.UInt: return "Igor.Serialization.StringSerializers.UInt";
                case PrimitiveType.Long: return "Igor.Serialization.StringSerializers.Long";
                case PrimitiveType.ULong: return "Igor.Serialization.StringSerializers.ULong";
                case PrimitiveType.Float: return "Igor.Serialization.StringSerializers.Float";
                case PrimitiveType.Double: return "Igor.Serialization.StringSerializers.Double";
                case PrimitiveType.String: return "Igor.Serialization.StringSerializers.String";
                case PrimitiveType.Atom: return "Igor.Serialization.StringSerializers.String";
                default: return null;
            }
        }

        public static string PrimitiveUriFormatter(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return "UriFormatter.Bool";
                case PrimitiveType.SByte: return "UriFormatter.SByte";
                case PrimitiveType.Byte: return "UriFormatter.Byte";
                case PrimitiveType.Short: return "UriFormatter.Short";
                case PrimitiveType.UShort: return "UriFormatter.UShort";
                case PrimitiveType.Int: return "UriFormatter.Int";
                case PrimitiveType.UInt: return "UriFormatter.UInt";
                case PrimitiveType.Long: return "UriFormatter.Long";
                case PrimitiveType.ULong: return "UriFormatter.ULong";
                case PrimitiveType.Float: return "UriFormatter.Float";
                case PrimitiveType.Double: return "UriFormatter.Double";
                case PrimitiveType.String: return "UriFormatter.String";
                case PrimitiveType.Binary: return "UriFormatter.Binary";
                case PrimitiveType.Atom: return "UriFormatter.String";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string GetHttpMethod(HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.GET: return "HttpMethod.Get";
                case HttpMethod.PUT: return "HttpMethod.Put";
                case HttpMethod.POST: return "HttpMethod.Post";
                case HttpMethod.DELETE: return "HttpMethod.Delete";
                case HttpMethod.HEAD: return "HttpMethod.Head";
                case HttpMethod.OPTIONS: return "HttpMethod.Options";
                case HttpMethod.PATCH: return "HttpMethod.Patch";
                default: throw new ArgumentException($"Unsupported HTTP method {method}");
            }
        }

        public static string GetHttpStatusCode(int code)
        {
            if (Enum.IsDefined(typeof(System.Net.HttpStatusCode), code))
                return $"HttpStatusCode.{((System.Net.HttpStatusCode) code).ToString()}";
            else
                return $"(HttpStatusCode){code.ToString(CultureInfo.InvariantCulture)}";
        }

        public static string ShadowName(string word)
        {
            if (IsReservedWord(word))
                return "@" + word;
            else
                return word;
        }

        private static readonly HashSet<string> ReservedWords = new HashSet<string>
            { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
                "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false",
                "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
                "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
                "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
                "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
                "void", "volatile", "while" };

        public static bool IsReservedWord(string word)
        {
            return ReservedWords.Contains(word);
        }
    }
}
