using Igor.Text;
using Igor.UE4.AST;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Igor.UE4.Model
{
    public abstract class UeType
    {
        public virtual string Namespace => null;
        public virtual string RelativeName(string ns) => RelativeBaseName(ns);
        public virtual string RelativeConstName(string ns) => RelativeName(ns);
        protected abstract string RelativeBaseName(string ns);
        public string QualifiedConstName => RelativeConstName(null);
        public string QualifiedBaseName => RelativeBaseName(null);
        public string QualifiedName => RelativeName(null);
        public virtual IEnumerable<string> HIncludes => Enumerable.Empty<string>();

        public virtual string FormatValue(Value value, string ns) => null;
        public virtual string VarToString(string varName) => varName;
        public virtual string VarPrefix => "";

        public virtual UeType Substitute(IDictionary<GenericArgument, UeType> args) => this;
        public virtual IEnumerable<UeType> UsedTypes => this.Yield();
    }

    public abstract class UePrimitiveType : UeType
    {
        public abstract PrimitiveType Primitive { get; }
        protected override string RelativeBaseName(string ns) => Helper.PrimitiveTypeString(Primitive);
    }

    public class UeBoolType : UePrimitiveType
    {
        public override PrimitiveType Primitive => PrimitiveType.Bool;

        public override string FormatValue(Value value, string ns)
        {
            switch (value)
            {
                case Value.Bool val: return val.Value ? "true" : "false";
                default: return base.FormatValue(value, ns);
            }
        }

        public override string VarPrefix => "b";
        public override string VarToString(string varName)
        {
            return $@"{varName} ? TEXT(""true"") : TEXT(""false"")";
        }
    }

    public class UeIntegerType : UePrimitiveType
    {
        public IntegerType IntType { get; }

        public UeIntegerType(IntegerType intType)
        {
            this.IntType = intType;
        }

        public override PrimitiveType Primitive => Igor.Primitive.FromInteger(IntType);

        public override string FormatValue(Value value, string ns)
        {
            switch (value)
            {
                case Value.Integer val: return val.Value.ToString(CultureInfo.InvariantCulture);
                default: return base.FormatValue(value, ns);
            }
        }

        public override string VarToString(string varName)
        {
            return $"FString::FromInt({varName})";
        }
    }

    public class UeFloatType : UePrimitiveType
    {
        public FloatType FloatType { get; }

        public UeFloatType(FloatType floatType)
        {
            this.FloatType = floatType;
        }

        public override PrimitiveType Primitive => Igor.Primitive.FromFloat(FloatType);

        public override string FormatValue(Value value, string ns)
        {
            string floatize(string txt)
            {
                if (txt.Contains('.') || txt.Contains('e') || txt.Contains('E'))
                    return txt + "f";
                else
                    return txt + ".0f";
            }

            switch (value)
            {
                case Value.Float val when FloatType == FloatType.Float: return floatize(val.Value.ToString(CultureInfo.InvariantCulture));
                case Value.Float val when FloatType == FloatType.Double: return val.Value.ToString(CultureInfo.InvariantCulture);
                case Value.Integer val: return val.Value.ToString(CultureInfo.InvariantCulture);
                default: return base.FormatValue(value, ns);
            }
        }
    }

    public class UeStringType : UePrimitiveType
    {
        public override PrimitiveType Primitive => PrimitiveType.String;

        public override string FormatValue(Value value, string ns)
        {
            switch (value)
            {
                case Value.String str: return $@"TEXT(""{str.Value}"")";
                default: return base.FormatValue(value, ns);
            }
        }
    }

    public class UeBinaryType : UePrimitiveType
    {
        public override PrimitiveType Primitive => PrimitiveType.Binary;
    }

    public class UeNameType : UePrimitiveType
    {
        public override PrimitiveType Primitive => PrimitiveType.Atom;
    }

    public class UeJsonType : UePrimitiveType
    {
        public override PrimitiveType Primitive => PrimitiveType.Json;
    }

    public abstract class UeUserType : UeType
    {
        public abstract TypeForm Form { get; }
        public override string Namespace => Form.ueAlias == null ? Form.ueNamespace : null;

        protected override string RelativeBaseName(string ns) => UeName.RelativeName(Namespace, Form.ueAlias ?? Form.ueName, ns);
        public override IEnumerable<string> HIncludes => Form.Module.ueHFile.Yield().Concat(Form.ueHIncludes);
    }

    public class UeEnumType : UeUserType
    {
        public EnumForm Enum { get; }

        public UeEnumType(EnumForm form)
        {
            this.Enum = form;
        }

        public override TypeForm Form => Enum;

        public override string FormatValue(Value value, string ns)
        {
            switch (value)
            {
                case Value.Enum field: return field.Field.ueRelativeQualifiedName(ns);
                default: return base.FormatValue(value, ns);
            }
        }

        public override string VarToString(string varName)
        {
            return $"Igor::IgorWriteJsonKey({varName})";
        }
    }

    public class UeTypedefType : UeUserType
    {
        public DefineForm Define { get; }

        public UeTypedefType(DefineForm form)
        {
            Define = form;
        }

        public override TypeForm Form => Define;

        public override string FormatValue(Value value, string ns)
        {
            return Define.ueTargetType.FormatValue(value, ns);
        }

        protected override string RelativeBaseName(string ns) => UeName.RelativeName(Namespace, Form.ueAlias ?? Form.ueName, ns);
    }

    public class UeStructPtrType : UeUserType
    {
        public StructForm Struct { get; }
        public IReadOnlyList<UeType> GenericArgs { get; }

        public UeStructPtrType(StructForm form, IReadOnlyList<UeType> genericArgs)
        {
            this.Struct = form;
            this.GenericArgs = genericArgs;
        }

        public override TypeForm Form => Struct;
        protected override string RelativeBaseName(string ns)
        {
            var baseName = base.RelativeBaseName(ns);
            if (GenericArgs.Any())
                baseName = $"{baseName}<{GenericArgs.JoinStrings(", ", arg => arg.RelativeName(ns))}>";
            return baseName;
        }
        public override string RelativeName(string ns) => $"TSharedPtr<{RelativeBaseName(ns)}>";
        public override string RelativeConstName(string ns) => $"TSharedPtr<const {RelativeBaseName(ns)}>";

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => new UeStructPtrType(Struct, GenericArgs.Select(arg => arg.Substitute(args)).ToArray());

    }

    public class UeStructType : UeUserType
    {
        public StructForm Struct { get; }
        public IReadOnlyList<UeType> GenericArgs { get; }

        public UeStructType(StructForm form, IReadOnlyList<UeType> genericArgs)
        {
            this.Struct = form;
            this.GenericArgs = genericArgs;
        }

        public override TypeForm Form => Struct;

        protected override string RelativeBaseName(string ns)
        {
            var baseName = base.RelativeBaseName(ns);
            if (GenericArgs.Any())
                baseName = $"{baseName}<{GenericArgs.JoinStrings(", ", arg => arg.RelativeName(ns))}>";
            return baseName;
        }

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => new UeStructPtrType(Struct, GenericArgs.Select(arg => arg.Substitute(args)).ToArray());
    }

    public class UeListType : UeType
    {
        public UeType ItemType { get; }

        public UeListType(UeType itemType)
        {
            ItemType = itemType;
        }

        protected override string RelativeBaseName(string ns) => $"TArray<{ItemType.RelativeName(ns)}>";

        public override IEnumerable<string> HIncludes => ItemType.HIncludes;
        public override IEnumerable<UeType> UsedTypes => base.UsedTypes.Concat(ItemType.UsedTypes);

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => new UeListType(ItemType.Substitute(args));
    }

    public class UeDictType : UeType
    {
        public UeType KeyType { get; }
        public UeType ValueType { get; }

        public UeDictType(UeType keyType, UeType valueType)
        {
            KeyType = keyType;
            ValueType = valueType;
        }

        protected override string RelativeBaseName(string ns) => $"TMap<{KeyType.RelativeName(ns)}, {ValueType.RelativeName(ns)}>";

        public override IEnumerable<string> HIncludes => KeyType.HIncludes.Concat(ValueType.HIncludes);
        public override IEnumerable<UeType> UsedTypes => base.UsedTypes.Concat(KeyType.UsedTypes).Concat(ValueType.UsedTypes);

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => new UeDictType(KeyType.Substitute(args), ValueType.Substitute(args));
    }

    public class UeOptionalType : UeType
    {
        public UeType Type { get; }

        public UeOptionalType(UeType type)
        {
            Type = type;
        }

        protected override string RelativeBaseName(string ns) => $"TOptional<{Type.RelativeName(ns)}>";

        public override string FormatValue(Value value, string ns)
        {
            return Type.FormatValue(value, ns);
        }

        public override IEnumerable<string> HIncludes => Type.HIncludes;
        public override IEnumerable<UeType> UsedTypes => base.UsedTypes.Concat(Type.UsedTypes);

        public override string VarPrefix => Type.VarPrefix;

        public override string VarToString(string varName) => Type.VarToString($"{varName}.GetValue()");

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => new UeOptionalType(Type.Substitute(args));
    }

    public class UeGenericArgument : UeType
    {
        public GenericArgument Arg { get; }

        public UeGenericArgument(GenericArgument arg) => Arg = arg;

        protected override string RelativeBaseName(string ns) => Arg.ueName;

        public override UeType Substitute(IDictionary<GenericArgument, UeType> args) => args.GetValueOrDefault(Arg, this);
    }
}
