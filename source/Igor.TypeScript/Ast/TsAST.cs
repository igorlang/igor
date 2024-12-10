using Igor.Text;
using System.Linq;

namespace Igor.TypeScript.AST
{
    public partial class Statement
    { }

    public partial class Module
    {
        public string tsFileName => Attribute(TsAttributes.File, tsName.Format(Notation.LowerHyphen) + ".ts");
        public string tsName => Attribute(TsAttributes.Name, Name.Format(Notation.UpperCamel));

        private TsModule cachedTsModule;

        public TsModule tsModule
        {
            get
            {
                if (cachedTsModule == null)
                    cachedTsModule = new TsModule(tsName, "./" + System.IO.Path.GetFileNameWithoutExtension(tsFileName));
                return cachedTsModule;
            }
        }
    }

    public partial class GenericArgument
    {
        public string tsName => Name.Format(Notation.UpperCamel);
        public TsType tsType => new TsGenericArgument(this);
        public string tsVarName => Name.Format(Notation.LowerCamel);
    }

    public partial class Form
    {
        public string tsName => Attribute(TsAttributes.Name, Name.Format(Notation.UpperCamel));
        public string tsNamespace => Attribute(TsAttributes.Namespace, Module.tsName);
        public bool tsEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class TypeForm
    {
        public string tsFullTypeName => tsAlias ?? TsName.Combine(tsNamespace, tsName);
        public bool tsGenerateDeclaration => (tsEnabled && tsAlias == null);
        public abstract TsType tsType { get; }
        public TsType[] tsArgs => Args.Select(t => Helper.TargetType(t)).ToArray();
        public string tsAlias => Attribute(TsAttributes.Alias, null);
    }

    public partial class EnumField
    {
        public string tsName => Name.Format(Notation.UpperCamel);
        public string tsJsonString => $@"'{jsonKey}'";

        public string tsQualifiedName(string ns) => TsName.Combine(Enum.tsType.relativeName(ns), tsName);
    }

    public partial class EnumForm
    {
        public string tsIntType => Helper.PrimitiveTypeString(Primitive.FromInteger(IntType));
        public override TsType tsType => new TsEnumType(this);
        public TsEnumFlagsType tsFlagsType => new TsEnumFlagsType(this);
    }

    public partial class RecordField
    {
        public Notation tsFieldNotation => Attribute(TsAttributes.FieldNotation, Notation.LowerCamel);
        public string tsName => Attribute(TsAttributes.Name, Name.Format(tsFieldNotation));
        public string tsVarName => Helper.ShadowName(tsName.Format(Notation.LowerCamel));
        public TsType tsType => Helper.TargetType(Type);
        public string tsTypeName => tsType.relativeName(Struct.tsNamespace);
        public string tsDefault => HasDefault ? tsType.FormatValue(Default, Struct.tsNamespace, Location) : "null";
        public bool tsParameter => Attribute(TsAttributes.Parameter, false);
        public bool tsReadonly => Attribute(TsAttributes.Readonly, false);
        public bool tsPublic => Attribute(TsAttributes.Public, false);
        public bool tsPrivate => Attribute(TsAttributes.Private, false);
        public bool tsErrorMessage => Attribute(TsAttributes.ErrorMessage, false);

        public string tsModifier
        {
            get
            {
                if (tsReadonly)
                    return "readonly ";
                else if (tsPrivate)
                    return "private ";
                else if (tsPublic)
                    return "public ";
                else
                    return null;
            }
        }
    }

    public partial class StructForm
    {
        public override TsType tsType => new TsClassType(this, tsArgs);
        public bool tsSetupCtor => Attribute(TsAttributes.SetupCtor, Fields.Any(f => f.tsParameter));
    }

    public partial class DefineForm
    {
        public override TsType tsType => tsAlias != null ? new TsClassType(this, tsArgs) : Helper.TargetType(Type);
    }

    public partial class UnionForm : TypeForm
    {
        public override TsType tsType => throw new System.NotImplementedException("Union types are not implemented");
    }

    public partial class FunctionArgument
    {
        public TsType tsType => Helper.TargetType(Type);
        public string tsTypeName => tsType.relativeName(Function.Service.tsNamespace);
        public string tsName => Name.Format(Notation.LowerCamel);
        public string tsTypeAndName => $"{tsName}: {tsTypeName}";
    }

    public partial class FunctionThrow
    {
        public TsType tsType => Exception.tsType;
        public string tsTypeName => tsType.relativeName(Function.Service.tsNamespace);
    }

    public partial class ServiceFunction
    {
        public string tsName => Name.Format(Notation.LowerCamel);
        public string tsRecvFun => "recv" + Name.Format(Notation.UpperCamel);
        public string tsTypedArgs => Arguments.JoinStrings(", ", arg => arg.tsTypeAndName);
        public string tsArgs => Arguments.JoinStrings(", ", arg => arg.tsName);
        public string tsArgsComma => Arguments.JoinStrings(arg => arg.tsName + ", ");
        public string tsTypedArgsComma => Arguments.JoinStrings(arg => arg.tsTypeAndName + ", ");
        public string tsReturns => ReturnArguments.JoinStrings(", ", arg => arg.tsName);
        public string tsTypedReturns => ReturnArguments.JoinStrings(", ", arg => arg.tsTypeAndName);
        public string tsRpcResultTypeName => ReturnArguments.Any() ? $"I{tsName.Format(Notation.UpperCamel)}Result" : "void";
        public string tsQualifiedRpcResultTypeName => ReturnArguments.Any() ? $"{Service.tsName}.{tsRpcResultTypeName}" : "void";
        public string tsResultTypeName => IsRpc ? $"Promise<{tsQualifiedRpcResultTypeName}>" : "void";
    }

    public partial class ServiceForm
    {
        public string tsClassName(Direction direction) => direction == Direction.ServerToClient ? $"{tsName}Server" : $"{tsName}Client";

        public string tsInterfaceName(Direction direction) => $"I{tsName}{direction}";

        public bool tsBinaryClient => Attribute(CoreAttributes.Client, false) && binaryEnabled;
        public bool tsBinaryServer => Attribute(CoreAttributes.Server, false) && binaryEnabled;
        public bool tsJsonClient => Attribute(CoreAttributes.Client, false) && jsonEnabled;
        public bool tsJsonServer => Attribute(CoreAttributes.Server, false) && jsonEnabled;
    }
}
