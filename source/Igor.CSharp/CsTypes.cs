using Igor.Compiler;
using Igor.CSharp.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp
{
    public abstract class CsType
    {
        public virtual bool isReference => true;

        public abstract string binarySerializer(string ns);

        public virtual CsType nonOptType => this;

        public abstract string jsonSerializer(string ns);

        public virtual string stringSerializer(string ns) => null;

        public virtual string uriFormatter(string ns, WebVariable v) => $"UriFormatter.FromStringSerializer({stringSerializer(ns)})";

        public virtual bool isOptional => false;
        public virtual bool canBeNull => isReference;
        public virtual bool csNotNullRequired => isReference;
        public virtual bool allowEquality => !isReference;
        public virtual bool isLiteral => false;

        public virtual string getHashCode(string value, string ns) => isReference ? $"({value} == null ? 0 : {value}.GetHashCode())" : $"{value}.GetHashCode()";

        public virtual string equalityComparer(string ns) => $"EqualityComparer<{relativeName(ns)}>.Default";

        public virtual bool requireEqualityComparer => false;

        public virtual string equals(string value1, string value2) => allowEquality ? $"{value1} == {value2}" : $"object.Equals({value1}, {value2})";

        public virtual string notEquals(string value1, string value2) => allowEquality ? $"{value1} != {value2}" : $"!object.Equals({value1}, {value2})";

        public abstract string relativeName(string ns);

        public virtual string FormatValue(Value value, string ns, Location location)
        {
            Context.Instance.CompilerOutput.Error(location, $"Unsupported C# value: {value.ToString()}", ProblemCode.TargetSpecificProblem);
            return null;
        }

        public virtual string writeValue(string arg) => arg;

        public virtual CsType Substitute(IDictionary<GenericArgument, CsType> args) => this;
    }

    public abstract class CsPrimitiveType : CsType
    {
        public abstract PrimitiveType primitive { get; }

        public override string relativeName(string ns) => Helper.PrimitiveTypeString(primitive);

        public override bool isReference => Helper.PrimitiveIsReference(primitive);

        public override bool isLiteral => true;

        public override string binarySerializer(string ns) => Helper.PrimitiveSerializer(primitive);

        public override string jsonSerializer(string ns) => Helper.JsonPrimitiveSerializer(primitive);

        public override string stringSerializer(string ns) => Helper.StringPrimitiveSerializer(primitive);

        public override string uriFormatter(string ns, WebVariable v) => Helper.PrimitiveUriFormatter(primitive);
    }

    public class CsBoolType : CsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.Bool;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Bool v when v.Value: return "true";
                case Value.Bool v when !v.Value: return "false";
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsIntegerType : CsPrimitiveType
    {
        public IntegerType intType { get; }

        public CsIntegerType(IntegerType intType)
        {
            this.intType = intType;
        }

        public override PrimitiveType primitive => Primitive.FromInteger(intType);

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Integer v: return v.Value.ToString();
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsFloatType : CsPrimitiveType
    {
        public FloatType floatType { get; }

        public CsFloatType(FloatType floatType)
        {
            this.floatType = floatType;
        }

        public override PrimitiveType primitive => Primitive.FromFloat(floatType);

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Integer v: return v.Value.ToString();
                case Value.Float v when floatType == FloatType.Float: return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "f";
                case Value.Float v: return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsStringType : CsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.String;
        public override bool allowEquality => true;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.String v: return v.Value.Quoted();
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsBinaryType : CsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.Binary;

        public override string getHashCode(string value, string ns) => $"Igor.Comparisons.HashCodes.Binary({value})";

        public override bool requireEqualityComparer => true;

        public override string equalityComparer(string ns) => "Igor.Comparisons.BinaryEqualityComparer.Instance";

        public override string equals(string value1, string value2) => $"Igor.Comparisons.Equality.Equals({value1}, {value2})";

        public override string notEquals(string value1, string value2) => $"!Igor.Comparisons.Equality.Equals({value1}, {value2})";
    }

    public class CsJsonType : CsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.Json;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.EmptyObject _:
                    return "Json.ImmutableJson.EmptyObject";
                default:
                    return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsOneOfType : CsType
    {
        public IReadOnlyList<CsType> Options { get; }

        public CsOneOfType(IReadOnlyList<CsType> options)
        {
            Options = options;
        }

        public override string binarySerializer(string ns) => $"OneOfBinarySerializer.Instance({Options.JoinStrings(", ", opt => opt.jsonSerializer(ns))})";

        public override string jsonSerializer(string ns) => $"Json.OneOfJsonSerializer.Instance({Options.JoinStrings(", ", opt => opt.jsonSerializer(ns))})";

        public override string relativeName(string ns) => $"OneOf.OneOf<{Options.JoinStrings(", ", opt => opt.relativeName(ns))}>";

        public override bool isReference => false;
    }

    public abstract class CsBaseListType : CsType
    {
        public CsType valueType { get; }

        public override string equals(string value1, string value2) => $"System.Linq.Enumerable.SequenceEqual({value1}, {value2})";

        public override bool requireEqualityComparer => true;

        protected CsBaseListType(CsType valueType)
        {
            this.valueType = valueType;
        }
    }

    public class CsListType : CsBaseListType
    {
        public CsListType(CsType valueType) : base(valueType)
        {
        }

        public override string relativeName(string ns) => $"List<{valueType.relativeName(ns)}>";

        public override string binarySerializer(string ns) => $"IgorSerializer.List({valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) => $"JsonSerializer.List({valueType.jsonSerializer(ns)})";

        public override string equalityComparer(string ns) => $"new Igor.Comparers.ListEqualityComparer<{valueType.relativeName(ns)}>({valueType.equalityComparer(ns)})";

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsListType(valueType.Substitute(args));

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.List v:
                    {
                        var items = v.Value.JoinStrings(", ", i => valueType.FormatValue(i, ns, location));
                        if (items.Any())
                            return $"new {relativeName(ns)}() {{ {items} }}";
                        else
                            return $"new {relativeName(ns)}()";
                    }
                default: return base.FormatValue(value, ns, location);
            }
        }

        public override string uriFormatter(string ns, WebVariable v)
        {
            var unfold = v == null ? false : v.QueryUnfold;
            var separator = v == null ? "," : v.QuerySeparator;
            if (unfold)
                return $@"UriFormatter.List({valueType.uriFormatter(ns, v)}, null)";
            else
                return $@"UriFormatter.List({valueType.uriFormatter(ns, v)}, ""{separator}"")";
        }
    }

    public class CsReadOnlyListType : CsBaseListType
    {
        public CsReadOnlyListType(CsType valueType) : base(valueType)
        {
        }

        public override string relativeName(string ns) => $"IReadOnlyList<{valueType.relativeName(ns)}>";

        public override string binarySerializer(string ns) => $"IgorSerializer.ReadOnlyList({valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) => $"JsonSerializer.ReadOnlyList({valueType.jsonSerializer(ns)})";

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsReadOnlyListType(valueType.Substitute(args));

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.List v:
                    {
                        return CsVersion.EmptyArray(valueType.relativeName(ns));
                    }
                default: return base.FormatValue(value, ns, location);
            }
        }

        public override string uriFormatter(string ns, WebVariable v)
        {
            var unfold = v == null ? false : v.QueryUnfold;
            var separator = v == null ? "," : v.QuerySeparator;
            if (unfold)
                return $@"UriFormatter.List({valueType.uriFormatter(ns, v)}, null)";
            else
                return $@"UriFormatter.List({valueType.uriFormatter(ns, v)}, ""{separator}"")";
        }
    }

    public abstract class CsBaseDictType : CsType
    {
        public CsType keyType { get; }
        public CsType valueType { get; }

        protected CsBaseDictType(CsType keyType, CsType valueType)
        {
            this.keyType = keyType;
            this.valueType = valueType;
        }
    }

    public class CsDictType : CsBaseDictType
    {
        public CsDictType(CsType keyType, CsType valueType) : base(keyType, valueType)
        {
        }

        public override string relativeName(string ns) => $"Dictionary<{keyType.relativeName(ns)}, {valueType.relativeName(ns)}>";

        public override string binarySerializer(string ns) =>
            keyType.requireEqualityComparer ?
                $"IgorSerializer.Dict({keyType.binarySerializer(ns)}, {valueType.binarySerializer(ns)}, {keyType.equalityComparer(ns)})"
                : $"IgorSerializer.Dict({keyType.binarySerializer(ns)}, {valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) =>
            keyType.requireEqualityComparer ?
                $"JsonSerializer.Dict({keyType.jsonSerializer(ns)}, {valueType.jsonSerializer(ns)}, {keyType.equalityComparer(ns)})"
                : $"JsonSerializer.Dict({keyType.jsonSerializer(ns)}, {valueType.jsonSerializer(ns)})";

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsDictType(keyType.Substitute(args), valueType.Substitute(args));

        public override string FormatValue(Value value, string ns, Location location)
        {
            var equalityComparer = keyType.requireEqualityComparer ? keyType.equalityComparer(ns) : null;
            switch (value)
            {
                case Value.List v when v.Value.Count == 0:
                    return $"new {relativeName(ns)}({equalityComparer})";
                case Value.Dict v:
                    {
                        var items = v.Value.JoinStrings(", ", p => $"{{ {keyType.FormatValue(p.Key, ns, location)}, {valueType.FormatValue(p.Value, ns, location)} }}");
                        return $"new {relativeName(ns)}({equalityComparer}) {{ {items} }}";
                    }
                default:
                    return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsReadOnlyDictType : CsBaseDictType
    {
        public CsReadOnlyDictType(CsType keyType, CsType valueType) : base(keyType, valueType)
        {
        }

        public override string relativeName(string ns) => $"IReadOnlyDictionary<{keyType.relativeName(ns)}, {valueType.relativeName(ns)}>";

        public override string binarySerializer(string ns) =>
            keyType.requireEqualityComparer ?
                $"IgorSerializer.ReadOnlyDict({keyType.binarySerializer(ns)}, {valueType.binarySerializer(ns)}, {keyType.equalityComparer(ns)})"
                : $"IgorSerializer.ReadOnlyDict({keyType.binarySerializer(ns)}, {valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) =>
            keyType.requireEqualityComparer ?
                $"JsonSerializer.ReadOnlyDict({keyType.jsonSerializer(ns)}, {valueType.jsonSerializer(ns)}, {keyType.equalityComparer(ns)})"
                : $"JsonSerializer.ReadOnlyDict({keyType.jsonSerializer(ns)}, {valueType.jsonSerializer(ns)})";

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsReadOnlyDictType(keyType.Substitute(args), valueType.Substitute(args));

        public override string FormatValue(Value value, string ns, Location location)
        {
            var equalityComparer = keyType.requireEqualityComparer ? keyType.equalityComparer(ns) : null;
            switch (value)
            {
                case Value.List v when v.Value.Count == 0:
                    return $"new Dictionary<{keyType.relativeName(ns)}, {valueType.relativeName(ns)}>({equalityComparer})";
                case Value.Dict v when v.Value.Count == 0:
                    return $"new Dictionary<{keyType.relativeName(ns)}, {valueType.relativeName(ns)}>({equalityComparer})";
                default:
                    return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsNullableType : CsType
    {
        public CsType valueType { get; }

        public CsNullableType(CsType valueType)
        {
            this.valueType = valueType;
        }

        public override bool isOptional => true;

        public override string relativeName(string ns) => valueType.relativeName(ns) + "?";

        public override string binarySerializer(string ns) => $"IgorSerializer.Nullable({valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) => $"JsonSerializer.Nullable({valueType.jsonSerializer(ns)})";

        public override CsType nonOptType => valueType;
        public override bool canBeNull => true;
        public override bool csNotNullRequired => false;
        public override bool requireEqualityComparer => valueType.requireEqualityComparer;

        public override string equalityComparer(string ns) => $"new Igor.Comparisons.NullableEqualityComparer<{valueType.relativeName(ns)}>({valueType.equalityComparer(ns)})";

        public override string getHashCode(string value, string ns)
        {
            var valueHashCode = valueType.getHashCode($"{value}.Value", ns);
            return $"({value}.HasValue ? 17 + {valueHashCode} : 0)";
        }

        public override bool isReference => false;

        public override string writeValue(string arg) => $"{arg}.Value";

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case null: return "null";
                default: return valueType.FormatValue(value, ns, location);
            }
        }
    }

    public class CsOptionalType : CsType
    {
        public CsType valueType { get; }

        public CsOptionalType(CsType valueType)
        {
            this.valueType = valueType;
        }

        public override bool isOptional => true;

        public override string relativeName(string ns) =>
            CsVersion.NullableReferenceTypes ? valueType.relativeName(ns) + "?" : valueType.relativeName(ns);

        public override string binarySerializer(string ns) => $"IgorSerializer.Optional({valueType.binarySerializer(ns)})";

        public override string jsonSerializer(string ns) => $"JsonSerializer.Optional({valueType.jsonSerializer(ns)})";

        public override string getHashCode(string value, string ns) => valueType.getHashCode(value, ns);

        public override CsType nonOptType => valueType;
        public override bool canBeNull => true;
        public override bool csNotNullRequired => false;
        public override bool allowEquality => valueType.allowEquality;

        public override string equals(string value1, string value2) => valueType.equals(value1, value2);

        public override string notEquals(string value1, string value2) => valueType.notEquals(value1, value2);

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case null: return "null";
                default: return valueType.FormatValue(value, ns, location);
            }
        }
    }

    public abstract class CsUserType : CsType
    {
        public TypeForm form { get; }

        protected CsUserType(TypeForm form)
        {
            this.form = form;
        }

        public override string relativeName(string ns) => CsName.RelativeName(form.csFullTypeName, ns);

        public override bool isReference => form.csReference;

        public override string binarySerializer(string ns) => form.csBinarySerializerInstance(ns);

        public override string jsonSerializer(string ns) => form.csJsonSerializerInstance(ns);

        public override string stringSerializer(string ns) => form.csStringSerializerInstance(ns);
    }

    public class CsEnumType : CsUserType
    {
        public CsEnumType(EnumForm form)
            : base(form)
        {
        }

        public EnumForm Enum => (EnumForm)form;
        public IntegerType intType => Enum.IntType;
        public PrimitiveType primitive => Primitive.FromInteger(intType);
        public override bool isReference => false;
        public string intTypeString => Helper.PrimitiveTypeString(primitive);

        public override string equalityComparer(string ns) => Enum.csEqualityComparer ? Enum.csEqualityComparerInstance(ns) : base.equalityComparer(ns);

        public override string getHashCode(string value, string ns) => $"(int){value}";

        public override bool requireEqualityComparer => Enum.csEqualityComparer;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Enum v: return v.Field.csQualifiedName(ns);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsEnumFlagsType : CsUserType
    {
        public CsEnumFlagsType(EnumForm form)
            : base(form)
        {
        }

        public EnumForm Enum => (EnumForm)form;
        public IntegerType intType => Enum.IntType;
        public PrimitiveType primitive => Primitive.FromInteger(intType);
        public override bool isReference => false;
        public string intTypeString => Helper.PrimitiveTypeString(primitive);

        public override string jsonSerializer(string ns) => $"JsonSerializer.EnumFlags({form.csJsonSerializerInstance(ns)})";

        public override string equalityComparer(string ns) => Enum.csEqualityComparer ? Enum.csEqualityComparerInstance(ns) : base.equalityComparer(ns);

        public override bool requireEqualityComparer => Enum.csEqualityComparer;

        public override string getHashCode(string value, string ns) => $"(int){value}";

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Enum v: return v.Field.csQualifiedName(ns);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsClassType : CsUserType
    {
        public CsType[] GenericArgs { get; }

        public CsClassType(TypeForm form, CsType[] genericArgs)
            : base(form)
        {
            this.GenericArgs = genericArgs;
        }

        public override string relativeName(string ns)
        {
            var baseName = base.relativeName(ns);
            if (GenericArgs.Length == 0)
                return baseName;
            else
            {
                var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                return $"{baseName}<{args}>";
            }
        }

        public override bool allowEquality => form.csEquality;

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsClassType(form, GenericArgs.Select(arg => arg.Substitute(args)).ToArray());

        public override string binarySerializer(string ns)
        {
            if (GenericArgs.Length == 0)
                return form.csBinarySerializerInstance(ns);
            else
            {
                var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                var serArgs = GenericArgs.JoinStrings(", ", arg => arg.binarySerializer(ns));
                return $"{form.csBinarySerializerInstance(ns)}<{args}>({serArgs})";
            }
        }

        public override string jsonSerializer(string ns)
        {
            if (GenericArgs.Length == 0)
                return form.csJsonSerializerInstance(ns);
            else
            {
                var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                var serArgs = GenericArgs.JoinStrings(", ", arg => arg.jsonSerializer(ns));
                return $"{form.csJsonSerializerInstance(ns)}<{args}>({serArgs})";
            }
        }

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Float v:
                    switch (form.csAlias)
                    {
                        case "float": return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "f";
                        case "double": return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        default: return base.FormatValue(value, ns, location);
                    }
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class CsDefineType : CsUserType
    {
        public CsType[] GenericArgs { get; }
        public CsType TargetType { get; }

        public CsDefineType(DefineForm form, CsType targetType, CsType[] genericArgs)
            : base(form)
        {
            this.TargetType = targetType;
            this.GenericArgs = genericArgs;
        }
        
        public override string relativeName(string ns)
        {
            if (form.csAlias != null)
            {
                var baseName = CsName.RelativeName(form.csAlias, ns);
                if (GenericArgs.Length == 0)
                    return baseName;
                else
                {
                    var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                    return $"{baseName}<{args}>";
                }
            }
            else
            {
                return TargetType.relativeName(ns);
            }
        }

        public override bool allowEquality => form.csEquality;

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => new CsDefineType((DefineForm)form, TargetType.Substitute(args), GenericArgs.Select(arg => arg.Substitute(args)).ToArray());

        public override string binarySerializer(string ns)
        {
            if (form.csBinaryCustomSerializerInstance != null)
            {
                if (GenericArgs.Length == 0)
                    return form.csBinarySerializerInstance(ns);
                else
                {
                    var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                    var serArgs = GenericArgs.JoinStrings(", ", arg => arg.binarySerializer(ns));
                    return $"{form.csBinarySerializerInstance(ns)}<{args}>({serArgs})";
                }

            }
            else
                return TargetType.binarySerializer(ns);
        }

        public override string jsonSerializer(string ns)
        {
            if (form.csJsonCustomSerializerInstance != null)
            {
                if (GenericArgs.Length == 0)
                    return form.csJsonSerializerInstance(ns);
                else
                {
                    var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                    var serArgs = GenericArgs.JoinStrings(", ", arg => arg.jsonSerializer(ns));
                    return $"{form.csJsonSerializerInstance(ns)}<{args}>({serArgs})";
                }

            }
            else
                return TargetType.jsonSerializer(ns);
        }

        public override string stringSerializer(string ns)
        {
            if (form.csStringCustomSerializerInstance != null)
            {
                if (GenericArgs.Length == 0)
                    return form.csStringSerializerInstance(ns);
                else
                {
                    var args = GenericArgs.JoinStrings(", ", arg => arg.relativeName(ns));
                    var serArgs = GenericArgs.JoinStrings(", ", arg => arg.stringSerializer(ns));
                    return $"{form.csStringSerializerInstance(ns)}<{args}>({serArgs})";
                }

            }
            else
                return TargetType.stringSerializer(ns);
        }

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Float v:
                    switch (form.csAlias)
                    {
                        case "float": return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "f";
                        case "double": return v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        default: return TargetType.FormatValue(value, ns, location);
                    }
                default: return TargetType.FormatValue(value, ns, location);
            }
        }
    }

    public class CsGenericArgument : CsType
    {
        public GenericArgument Arg { get; }

        public CsGenericArgument(GenericArgument arg)
        {
            this.Arg = arg;
        }

        public override string relativeName(string ns) => Arg.csName;

        public override string binarySerializer(string ns) => Arg.csVarName;

        public override string jsonSerializer(string ns) => Arg.csVarName;

        public override CsType Substitute(IDictionary<GenericArgument, CsType> args) => args.GetValueOrDefault(Arg, this);
    }
}
