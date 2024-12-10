using Igor.Go.AST;
using System.Linq;

namespace Igor.Go
{
    public static class Helper
    {
        public static string GoIntType(IntegerType type)
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
                    return "int";
                case IntegerType.UInt:
                    return "uint";
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
                case PrimitiveType.Int: return "int";
                case PrimitiveType.UInt: return "uint";
                case PrimitiveType.Long: return "int64";
                case PrimitiveType.ULong: return "uint64";
                case PrimitiveType.Float: return "float32";
                case PrimitiveType.Double: return "float64";
                case PrimitiveType.String: return "string";
                case PrimitiveType.Binary: return "[]byte";
                case PrimitiveType.Atom: return "string";
                case PrimitiveType.Json: return "interface{}";
                default: throw new EUnknownType(type.ToString());
            }
        }

        /// <summary>
        /// Adds an underscore to the end of go names when needed.
        /// Underscore has to be added at the end to comply with golang
        /// rules for exported identifiers: https://go.dev/ref/spec#Exported_identifiers.
        /// </summary>
        public static string ShadowName(string name)
        {
            if (IsCommonInterfaceMethod(name))
                return name + "_";
            return name;
        }

        public static GoType TargetType(IType astType)
        {
            switch (astType)
            {
                case BuiltInType.Bool _: return new GoPrimitiveType(PrimitiveType.Bool);
                case BuiltInType.Integer i: return new GoPrimitiveType(Primitive.FromInteger(i.Type));
                case BuiltInType.Float f: return new GoPrimitiveType(Primitive.FromFloat(f.Type));
                case BuiltInType.String _: return new GoPrimitiveType(PrimitiveType.String);
                case BuiltInType.Binary _: return new GoPrimitiveType(PrimitiveType.Binary);
                case BuiltInType.Atom _: return new GoPrimitiveType(PrimitiveType.String);
                case BuiltInType.List l: return new GoArrayType(TargetType(l.ItemType));
                case BuiltInType.Dict d: return new GoMapType(TargetType(d.KeyType), TargetType(d.ValueType));
                case TypeForm form: return form.goType;
                case BuiltInType.Optional opt: return new GoNullableType(TargetType(opt.ItemType));
                case GenericArgument type: return type.goType;
                case GenericType type:
                    return new GoGenericType(TargetType(type.Prototype), type.Args.Select(a => TargetType(a)).ToArray());
                case BuiltInType.Json _: return new GoPrimitiveType(PrimitiveType.Json);
                default: throw new EUnknownType(astType.ToString());
            }
        }

        public static GoType TargetInterface(IInterface astType)
        {
            switch (astType)
            {
                case TypeForm form: return form.goType;
                default: throw new EUnknownType(astType.ToString());
            }
        }

        public static string GoValue(Value value, IType type)
        {
            switch (value)
            {
                case Value.Bool b when b.Value: return "true";
                case Value.Bool b when !b.Value: return "false";
                case Value.Integer i:
                    return i.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.Float f:
                    return f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.String s: return $@"<<""{s.Value}"">>";
                case Value.List list when type is BuiltInType.List listType: return "{}";
                case Value.Dict dict when type is BuiltInType.Dict dictType: return "{}";
                case Value.Enum e: return $"{e.Field.goName}";
                case Value v when type is BuiltInType.Optional opt: return GoValue(v, opt.ItemType);
                default:
                    throw new EInternal($"invalid value {value} for type {type}");
            }
        }

        public static string GenericTypeConditional(GoType goType)
        {
            switch (goType)
            {
                case GoMapType _: return "comparable";
                default: return "any";
            }
        }
       
        /// <summary>
	/// Get http method name constant as defined in go `net/http` package.
        /// To follow established conventions when using the `net/http` package.
	/// </summary>
	public static string GetHttpMethod(HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.GET: return "http.MethodGet";
                case HttpMethod.PUT: return "http.MethodPut";
                case HttpMethod.POST: return "http.MethodPost";
                case HttpMethod.DELETE: return "http.MethodDelete";
                default: throw new EInternal($"invalid HTTP method {method}");
            }
        }

        /// <summary>
        /// Get http status name constant as defined in go `net/http` package.
        /// To make the generated code a bit easier to read.
        /// To follow established conventions when using the `net/http` package.
        /// </summary>
        public static string GetHttpStatusName(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "http.StatusOK";
                case 201: return "http.StatusCreated";
                case 400: return "http.StatusBadRequest";
                case 403: return "http.StatusForbidden";
                case 404: return "http.StatusNotFound";
                case 405: return "http.StatusMethodNotAllowed";
                case 418: return "http.StatusTeapot";
                case 500: return "http.StatusInternalServerError";
                default: return $"{statusCode}";
            }
        }

        /// <summary>
        /// Get string conversion function for AST types.
        /// Useful for working with headers and url queries in the `net/http` package.
        /// </summary>
	public static string GetStrconvFormat(IType astType, string name)
	{
            switch (astType)
            {
                case BuiltInType.Bool _: return $"strconv.FormatBool({name})";
                case BuiltInType.Integer _: return $"strconv.FormatInt(int64({name}), 10)";
                case TypeForm _: return $"{name}.String()"; 
                case BuiltInType.String _: return name;
                case BuiltInType.Optional opt: return GetStrconvFormat(opt.ItemType, $"*{name}");
                default: throw new EInternal($"dont know how to format {astType}");
            }
	}

        /// <summary>
        /// Check if name is a common interface method.
        /// Limited to interfaces from builtin types or the `io` package for now.
        /// Keep expanding as needed.
        /// </summary>
        public static bool IsCommonInterfaceMethod(string name)
        {
            switch(name)
            {
                case "Close": return true;
                case "Error": return true;
                case "ReadAt": return true;
                case "ReadByte": return true;
                case "ReadFrom": return true;
                case "Read": return true;
                case "ReadRune": return true;
                case "Seek": return true;
                case "String": return true;
                case "UnreadByte": return true;
                case "UnreadRune": return true;
                case "WriteAt": return true;
                case "WriteByte": return true;
                case "Write": return true;
                case "WriteString": return true;
                case "WriteTo": return true;
                default: return false;
            }
        }
    }
}
