using Igor.Compiler;
using Igor.Text;
using Igor.TypeScript.AST;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Igor.TypeScript
{
    public abstract class TsType
    {
        public virtual IEnumerable<TsType> nestedTypes => Enumerable.Empty<TsType>();

        public virtual TsType nonOptType => this;

        public abstract string jsonSerializer(string ns);

        public virtual string fromJson(string json, string ns) => $"{jsonSerializer(ns)}.fromJson({json})";

        public virtual string toJson(string value, string ns) => $"{jsonSerializer(ns)}.toJson({value})";
        public virtual string toString(string value, string ns) => toJson(value, ns).Quoted("`${", "}`");

        public virtual string fromUri(string str, string ns) => fromJson(str, ns);

        public virtual string toUri(string value, string ns) => toJson(value, ns);

        public virtual bool isOptional => false;

        public abstract string relativeName(string ns);

        public virtual string FormatValue(Value value, string ns, Location location)
        {
            Context.Instance.CompilerOutput.Error(location, $"Unsupported TypeScript value: {value.ToString()}", ProblemCode.TargetSpecificProblem);
            return null;
        }

        public virtual string writeValue(string arg) => arg;

        public virtual TsType Substitute(IDictionary<GenericArgument, TsType> args) => this;

        public virtual TsModule module => null;
    }

    public abstract class TsPrimitiveType : TsType
    {
        public abstract PrimitiveType primitive { get; }

        public override string relativeName(string ns) => Helper.PrimitiveTypeString(primitive);

        public override string jsonSerializer(string ns) => Helper.JsonPrimitiveSerializer(primitive);

        public override string fromJson(string json, string ns) => Helper.PrimitiveFromJson(primitive, json);

        public override string toJson(string value, string ns) => value;
    }

    public class TsBoolType : TsPrimitiveType
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

    public class TsNumberType : TsPrimitiveType
    {
        private readonly PrimitiveType numberType;

        public TsNumberType(PrimitiveType numberType)
        {
            this.numberType = numberType;
        }

        public override PrimitiveType primitive => numberType;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Integer v: return v.Value.ToString();
                case Value.Float f: return f.Value.ToString(CultureInfo.InvariantCulture);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsStringType : TsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.String;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.String v: return v.Value.Quoted();
                default: return base.FormatValue(value, ns, location);
            }
        }

        public override string toString(string value, string ns) => value;
    }

    public class TsJsonType : TsPrimitiveType
    {
        public override PrimitiveType primitive => PrimitiveType.Json;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.List v: return "[]";
                case Value.EmptyObject v: return "{}";
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsArrayType : TsType
    {
        public TsType valueType { get; }
        protected Statement Host { get; }

        public TsArrayType(TsType valueType, Statement host)
        {
            this.valueType = valueType;
            this.Host = host;
        }

        public override IEnumerable<TsType> nestedTypes => new[] { valueType };

        public override string relativeName(string ns) => $"Array<{(valueType.relativeName(ns))}>";

        public override string jsonSerializer(string ns) => $"Igor.Json.List({valueType.jsonSerializer(ns)})";

        public override TsType Substitute(IDictionary<GenericArgument, TsType> args) => new TsArrayType(valueType.Substitute(args), Host);

        public override string toUri(string value, string ns)
        {
            if (Host != null && Host.Attribute(CoreAttributes.HttpSeparator) != null)
                return $@"{value}.join('{Host.Attribute(CoreAttributes.HttpSeparator)}')";
            else
                return base.toUri(value, ns);
        }

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.List v:
                    {
                        return "[]";
                    }
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsDictType : TsType
    {
        public TsType keyType { get; }
        public TsType valueType { get; }

        public override IEnumerable<TsType> nestedTypes => new[] { keyType, valueType };

        public TsDictType(TsType keyType, TsType valueType)
        {
            this.keyType = keyType;
            this.valueType = valueType;
        }

        public override string relativeName(string ns) => $"{{[key: string]: {valueType.relativeName(ns)}}}";

        public override string jsonSerializer(string ns) =>
            $"Igor.Json.Dict({valueType.jsonSerializer(ns)})";

        public override TsType Substitute(IDictionary<GenericArgument, TsType> args) => new TsDictType(keyType.Substitute(args), valueType.Substitute(args));

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.List _: return "{}";
                case Value.Dict _: return "{}";
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsOptionalType : TsType
    {
        public TsType valueType { get; }

        public TsOptionalType(TsType valueType)
        {
            this.valueType = valueType;
        }

        public override IEnumerable<TsType> nestedTypes => new[] { valueType };

        public override bool isOptional => true;

        public override string relativeName(string ns) => valueType.relativeName(ns) + " | null";

        public override string jsonSerializer(string ns) => $"Igor.Json.Optional({valueType.jsonSerializer(ns)})";

        public override string fromJson(string json, string ns) => valueType is TsPrimitiveType ? valueType.fromJson(json, ns) : $"{json} != null ? {valueType.fromJson(json, ns)} : null";

        public override string toJson(string value, string ns) => valueType is TsPrimitiveType ? valueType.toJson(value, ns) : $"{value} != null ? {valueType.toJson(value, ns)} : null";

        public override TsType nonOptType => valueType;

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case null: return "null";
                default: return valueType.FormatValue(value, ns, location);
            }
        }
    }

    public abstract class TsUserType : TsType
    {
        public TypeForm form { get; }

        protected TsUserType(TypeForm form)
        {
            this.form = form;
        }

        public override string relativeName(string ns) => TsName.RelativeName(form.tsFullTypeName, ns);

        public override string jsonSerializer(string ns) => form.tsJsonSerializerInstance(ns, null);

        public override TsModule module => form.Module.tsModule;
    }

    public class TsEnumType : TsUserType
    {
        public TsEnumType(EnumForm form)
            : base(form)
        {
        }

        public EnumForm Enum => (EnumForm)form;
        public IntegerType intType => Enum.IntType;
        public PrimitiveType primitive => Primitive.FromInteger(intType);
        public string intTypeString => Helper.PrimitiveTypeString(primitive);

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Enum v: return v.Field.tsQualifiedName(ns);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsEnumFlagsType : TsUserType
    {
        public TsEnumFlagsType(EnumForm form)
            : base(form)
        {
        }

        public EnumForm Enum => (EnumForm)form;
        public IntegerType intType => Enum.IntType;
        public PrimitiveType primitive => Primitive.FromInteger(intType);
        public string intTypeString => Helper.PrimitiveTypeString(primitive);

        public override string jsonSerializer(string ns) => $"JsonSerializer.EnumFlags({form.tsJsonSerializerInstance(ns, null)})";

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                case Value.Enum v: return v.Field.tsQualifiedName(ns);
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsClassType : TsUserType
    {
        public TsType[] GenericArgs { get; }

        public override IEnumerable<TsType> nestedTypes => GenericArgs;

        public TsClassType(TypeForm form, TsType[] genericArgs)
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

        public override TsType Substitute(IDictionary<GenericArgument, TsType> args) => new TsClassType(form, GenericArgs.Select(arg => arg.Substitute(args)).ToArray());

        public override string jsonSerializer(string ns)
        {
            return form.tsJsonSerializerInstance(ns, GenericArgs);
        }

        public override string toJson(string value, string ns)
        {
            if (form.tsJsonGenerateSerializer)
                return $"{value}.toJson({GenericArgs.JoinStrings(", ", arg => arg.jsonSerializer(ns))})";
            else
                return base.toJson(value, ns);
        }

        public override string FormatValue(Value value, string ns, Location location)
        {
            switch (value)
            {
                //                | Record(v,form) =>
                //                    def name = form.csType.relativeName(ns);
                //                    $<#new $name() { ..$(v; ", "; f => $"$(f.Key.csName) = $(FormatValue(f.Value, f.Key.type, location))") }#>
                case Value.Float v:
                    switch (form.tsAlias)
                    {
                        case "float": return v.Value.ToString(CultureInfo.InvariantCulture) + "f";
                        case "double": return v.Value.ToString(CultureInfo.InvariantCulture);
                        default: return base.FormatValue(value, ns, location);
                    }
                default: return base.FormatValue(value, ns, location);
            }
        }
    }

    public class TsGenericArgument : TsType
    {
        public GenericArgument Arg { get; }

        public TsGenericArgument(GenericArgument arg)
        {
            this.Arg = arg;
        }

        public override string relativeName(string ns) => Arg.tsName;

        public override string jsonSerializer(string ns) => Arg.tsVarName;

        public override TsType Substitute(IDictionary<GenericArgument, TsType> args) => args.GetValueOrDefault(Arg, this);
    }
}
