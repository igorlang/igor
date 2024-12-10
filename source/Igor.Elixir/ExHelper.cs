using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using Igor.Elixir.AST;
using Igor.Text;

namespace Igor.Elixir
{
    public static class Helper
    {
        public static string PrimitiveTag(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool: return ":boolean";
                case PrimitiveType.SByte: return ":sbyte";
                case PrimitiveType.Byte: return ":byte";
                case PrimitiveType.Short: return ":short";
                case PrimitiveType.UShort: return ":ushort";
                case PrimitiveType.Int: return ":int";
                case PrimitiveType.UInt: return ":uint";
                case PrimitiveType.Long: return ":long";
                case PrimitiveType.ULong: return ":ulong";
                case PrimitiveType.Float: return ":float";
                case PrimitiveType.Double: return ":double";
                case PrimitiveType.Binary: return ":binary";
                case PrimitiveType.String: return ":string";
                case PrimitiveType.Atom: return ":atom";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string ExIntType(IntegerType type)
        {
            switch (type)
            {
                case IntegerType.SByte: return "integer"; // "igor_types:sbyte()";
                case IntegerType.Byte: return "byte";
                case IntegerType.Short: return "integer"; // "igor_types:short()";
                case IntegerType.UShort: return "non_neg_integer"; // "igor_types:ushort()";
                case IntegerType.Int: return "integer"; // "igor_types:int()";
                case IntegerType.UInt: return "non_neg_integer"; // "igor_types:uint()";
                case IntegerType.Long: return "integer"; // "igor_types:long()";
                case IntegerType.ULong: return "non_neg_integer"; // "igor_types:ulong()";
                default: throw new EUnknownType(type.ToString());
            }
        }

        public static string ExValue(Value value, IType type)
        {
            switch (value)
            {
                case Value.Bool b when b.Value: return "true";
                case Value.Bool b when !b.Value: return "false";
                case Value.Integer i when type is BuiltInType.Float:
                    return EnsureFloat(i.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                case Value.Integer i:
                    return i.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case Value.Float f:
                    return EnsureFloat(f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                case Value.String s: return $@"""{s.Value}""";
                case Value.EmptyObject _ when type is BuiltInType.Json: return "%{}";
                case Value.List _ when type is BuiltInType.Json: return "[]";
                case Value.List list when type is BuiltInType.List listType: return $"[{list.Value.JoinStrings(", ", i => ExValue(i, listType.ItemType))}]";
                case Value.Dict dict when type is BuiltInType.Dict dictType:
                    string dictItem(KeyValuePair<Value, Value> p) => $"{{{ExValue(p.Key, dictType.KeyType)},{ExValue(p.Value, dictType.ValueType)}}}";
                    return $"[{dict.Value.JoinStrings(", ", dictItem)}]";
                case Value.Enum e: return e.Field.exName;
                case Value v when type is BuiltInType.Optional opt: return ExValue(v, opt.ItemType);
                case Value _ when type is DefineForm defineForm: return ExValue(value, defineForm.Type);
                default:
                    throw new EInternal($"invalid value {value} for type {type}");
            }
        }

        private static string EnsureFloat(string str)
        {
            if (str.Contains('.'))
                return str;
            else
                return str + ".0";
        }

        public static string ExType(IType type, bool shadowGenericArgs)
        {
            switch (type)
            {
                case GenericArgument arg: return shadowGenericArgs ? "_" + arg.exName : arg.exName;
                case TypeForm f: return f.exRemoteType;
                case GenericType f: return f.exRemoteType(shadowGenericArgs);
                case BuiltInType.Bool _: return "boolean";
                case BuiltInType.Integer i: return ExIntType(i.Type);
                case BuiltInType.Float _: return "float";
                case BuiltInType.String _: return "String.t()";
                case BuiltInType.Binary _: return "binary";
                case BuiltInType.Atom _: return "atom";
                case BuiltInType.List list: return $"[{ExType(list.ItemType, shadowGenericArgs)}]";
                case BuiltInType.Dict dict: return $"%{{{ExType(dict.KeyType, shadowGenericArgs)} => {ExType(dict.ValueType, shadowGenericArgs)}}}";
                case BuiltInType.Optional opt: return $"{ExType(opt.ItemType, shadowGenericArgs)} | nil";
                case BuiltInType.Flags flags: return $"[{ExType(flags.ItemType, shadowGenericArgs)}]";
                case BuiltInType.Json _: return "Igor.Json.json()";
                default: throw new EUnknownType(type.GetType().ToString() + " " + type.ToString());
            }
        }

        public static bool HasGenericArg(IType type, GenericArgument arg)
        {
            switch (type)
            {
                case GenericArgument arg1: return arg1 == arg;
                case GenericType f: return f.Args.Any(a => HasGenericArg(a, arg));
                case BuiltInType.List list: return HasGenericArg(list.ItemType, arg);
                case BuiltInType.Dict dict: return HasGenericArg(dict.KeyType, arg) || HasGenericArg(dict.ValueType, arg);
                case BuiltInType.Optional opt: return HasGenericArg(opt.ItemType, arg);
                case BuiltInType.Flags flags: return HasGenericArg(flags.ItemType, arg);
                case TypeForm f: return false;
                default: return false;
            }
        }

        public static string ExGuard(IType type, string value)
        {
            switch (type)
            {
                case TypeForm f: return f.exGuard(value);
                case BuiltInType.Bool _: return $"is_boolean({value})";
                case BuiltInType.Integer i: return $"is_integer({value})";
                case BuiltInType.Float _: return $"is_float({value})";
                case BuiltInType.String _: return $"is_binary({value})";
                case BuiltInType.Binary _: return $"is_binary({value})";
                case BuiltInType.Atom _: return $"is_atom({value})";
                case BuiltInType.List list: return $"is_list({value})";
                case BuiltInType.Dict dict: return $"is_list({value})";
                case BuiltInType.Optional opt: return $"({ExGuard(opt.ItemType, value)} or {value} === nil)";
                case BuiltInType.Flags flags: return $"is_list({value})";
                default: return null;
            }
        }

        public static IEnumerable<string> GuardRequires(IType type)
        {
            switch (type)
            {
                case TypeForm f: return f.exGuardRequires;
                case BuiltInType.List list: return GuardRequires(list.ItemType);
                case BuiltInType.Dict dict: return GuardRequires(dict.KeyType).Concat(GuardRequires(dict.ValueType));
                case BuiltInType.Optional opt: return GuardRequires(opt.ItemType);
                case BuiltInType.Flags flags: return GuardRequires(flags.ItemType);
                default: return Array.Empty<string>();
            }
        }

        private static readonly HashSet<string> igorReserved = new HashSet<string> { "S", "V", "State", "RpcId", "Binary", "Handler", "Reason", "Action",
                "Tail", "Types", "Message", "OnSuccess", "OnFail", "Key", "Value", "Enum", "Record", "Result", "Content" };

        private static readonly HashSet<string> reserved = new HashSet<string> { "true", "false", "nil", "when", "and", "or", "not", "in", "fn", "do", "end", "catch", "rescue", "after", "else" };

        private static readonly HashSet<string> reservedTypeVars = new HashSet<string> { "t" };

        public static string ShadowTypeVar(string name)
        {
            if (reservedTypeVars.Contains(name))
                return "t" + name;
            else
                return name;
        }

        public static string ShadowName(string name)
        {
            if (IsReservedWord(name))
                return "var_" + name;
            if (igorReserved.Contains(name))
                return "var_" + name;
            else
                return name;
        }

        public static string AtomName(string word)
        {
            if (word == string.Empty)
                return ":\"\"";
            else
            {
                var c = word[0];
                if (char.IsUpper(c) || IsReservedWord(word))
                    return ":" + word.Quoted("\"");
                var regex = new Regex(@"^[a-zA-Z0-9@_]+$");
                if (regex.IsMatch(word))
                    return ":" +word;
                else
                    return ":" + word.Quoted("\"");
            }
        }

        public static bool IsReservedWord(string word)
        {
            return reserved.Contains(word);
        }

        public static string EscapeString(string str)
        {
            return str.Replace("\"", "\\\"");
        }
    }
}
