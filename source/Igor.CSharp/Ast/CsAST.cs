using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp.AST
{
    public partial class Statement
    {
        public TypeContext csTypeContext =>
            new TypeContext
            {
                ListImplementation = Attribute(CsAttributes.ListImplementation, ListTypeImplementation.List),
                DictImplementation = Attribute(CsAttributes.DictImplementation, DictTypeImplementation.Dictionary),
            };
    }

    public partial class Module
    {
        public string csFileName => $"{csName}.cs";
        public string csName => Attribute(CsAttributes.Name, Name.Format(Notation.UpperCamel));
    }

    public partial class GenericArgument
    {
        public string csName => Name.Format(Notation.UpperCamel);
        public CsType csType => new CsGenericArgument(this);
        public string csVarName => Name.Format(Notation.LowerCamel);
    }

    public partial class Form
    {
        public string csName => Attribute(CsAttributes.Name, Name.Format(Notation.UpperCamel));
        public string csNamespace => Attribute(CsAttributes.Namespace, Module.csName);
        public bool csEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class TypeForm
    {
        public bool csJsonTestEnabled => Attribute(CsAttributes.JsonTest, false);
        public string csFullTypeName => csAlias ?? CsName.Combine(csNamespace, csName);
        public bool csGenerateDeclaration => (csEnabled && csAlias == null);
        public abstract CsType csType { get; }
        public CsType[] csArgs => Args.Select(arg => Helper.TargetType(arg, csTypeContext)).ToArray();
        public string csAlias => Attribute(CsAttributes.Alias, null);
        public bool csReference => Attribute(CsAttributes.Class, !csStructAttribute);
        private bool csStructAttribute => Attribute(CsAttributes.Struct, !csDefaultReference);
        protected virtual bool csDefaultReference => true;
        public bool csEquality => Attribute(CsAttributes.Equality, false);
        protected virtual bool csDefaultEquality => false;
        public string csRequire(string paramName) =>
            csType.isReference ? $@"    if ({paramName} == null)
        throw new System.ArgumentNullException({CsVersion.NameOf(paramName)});" : "";
    }

    public partial class EnumField
    {
        public string csName => Name.Format(Notation.UpperCamel);
        public string csJsonString => jsonKey.Quoted();

        public string csQualifiedName(string ns) => CsName.Combine(Enum.csType.relativeName(ns), csName);
    }

    public partial class EnumForm
    {
        public bool csEnumBaseTypes => Attribute(CsAttributes.EnumBaseTypes, false);
        public string csIntType => Helper.PrimitiveTypeString(Primitive.FromInteger(IntType));
        public override CsType csType => new CsEnumType(this);
        public CsEnumFlagsType csFlagsType => new CsEnumFlagsType(this);

        public bool csEqualityComparer => Attribute(CsAttributes.EqualityComparer, false);
        public string csEqualityComparerName => csName + "EqualityComparer";

        public string csEqualityComparerInstance(string ns) => CsName.RelativeName(csNamespace, csEqualityComparerName, ns) + ".Instance";
    }

    public partial class RecordField
    {
        public Notation csFieldNotation => Attribute(CsAttributes.FieldNotation, Notation.UpperCamel);
        public string csName => Attribute(CsAttributes.Name, Name.Format(csFieldNotation));
        public string csNotNull => csCanBeNull ? (csType.isReference ? $"{csName} != null" : $"{csName}.HasValue") : "true";
        public string csVarName => Helper.ShadowName(csName.Format(Notation.LowerCamel));
        public CsType csType
        {
            get
            {
                var t = Helper.TargetType(Type, csTypeContext);
                if (Struct.IsPatch)
                    t = t.isReference ? (CsType)new CsOptionalType(t) : new CsNullableType(t);
                return t;
            }
        }
        public string csTypeName => csType.relativeName(Struct.csNamespace);
        public bool csEqualsIgnore => Attribute(CsAttributes.EqualsIgnore, false);
        public bool csSetupCtorIgnore => Attribute(CsAttributes.SetupCtorIgnore, false);
        public bool csReadOnly => Attribute(CsAttributes.ReadOnly, Struct.csImmutable);

        public CsPropertyInfo csProperty
        {
            get
            {
                if (IsTag && Struct is VariantForm)
                    return new CsPropertyInfo { Attributes = csAttributes, Name = csName, Type = csType, IsReadOnly = true, PropertyType = PropertyType.Abstract, IgnoreEquals = csEqualsIgnore, IgnoreSetupCtor = true, IsInherited = IsInherited, Summary = Annotation };
                else if (IsTag)
                    return new CsPropertyInfo { Attributes = csAttributes, Name = csName, Type = csType, IsReadOnly = true, PropertyType = PropertyType.Override, Expression = csDefault, IgnoreEquals = csEqualsIgnore, IgnoreSetupCtor = true, IsInherited = IsInherited, Summary = Annotation };
                else
                    return new CsPropertyInfo { Attributes = csAttributes, Name = csName, Type = csType, IsReadOnly = csReadOnly, Value = csDefault, IgnoreEquals = csEqualsIgnore, IgnoreSetupCtor = csSetupCtorIgnore, IsInherited = IsInherited, IsParentSetup = IsInherited && Struct.Ancestor.csSetupCtor, Summary = Annotation };
            }
        }

        public string csDefault => HasDefault ? csType.FormatValue(Default, Struct.csNamespace, Location) : null;
        public bool csCanBeNull => csType.canBeNull;

        public IReadOnlyList<string> csAttributes => ListAttribute(CsAttributes.Attribute);
    }

    public partial class StructForm
    {
        public bool csImmutable => Attribute(CsAttributes.Immutable, false);
        public bool csSetupCtor => csImmutable || Attribute(CsAttributes.SetupCtor, false);
        public bool csDefaultCtor => !csImmutable && Attribute(CsAttributes.DefaultCtor, csReference);
        public bool csPartial => Attribute(CsAttributes.Partial, false);
        public bool csSealed => Attribute(CsAttributes.Sealed, this is RecordForm);
        public override CsType csType => new CsClassType(this, csArgs);
        public bool csGenerateEquals => Attribute(CsAttributes.Equals, csEquality || (Ancestor != null && Ancestor.csGenerateEquals));
    }

    public partial class UnionForm
    {
        public override CsType csType => throw new System.NotImplementedException();
    }

    public partial class DefineForm
    {
        public CsType csTargetType => Helper.TargetType(Type, csTypeContext);
        public override CsType csType => new CsDefineType(this, csTargetType, csArgs);
        protected override bool csDefaultReference => csTargetType.isReference;
        protected override bool csDefaultEquality => csTargetType.allowEquality;
    }

    public partial class FunctionArgument
    {
        public CsType csType => Helper.TargetType(Type, Function.csTypeContext);
        public string csTypeName => csType.relativeName(Function.Service.csNamespace);
        public string csName => Name.Format(Notation.UpperCamel);
        public string csArgName => Helper.ShadowName(Name.Format(Notation.LowerCamel));
        public string csShadowName => "_" + csName;
        public string csTypeAndName => $"{csTypeName} {csArgName}";

        public CsPropertyInfo csProperty => new CsPropertyInfo { Name = csName, Type = csType };
    }

    public partial class FunctionThrow
    {
        public string csTypeName => Exception.csType.relativeName(Function.Service.csNamespace);
    }

    public partial class ServiceFunction
    {
        public string csName => Name.Format(Notation.UpperCamel);
        public string csTypedArgs => Arguments.JoinStrings(", ", arg => arg.csTypeAndName);
        public string csArgs => Arguments.JoinStrings(", ", arg => arg.csArgName);
        public string csArgsComma => Arguments.JoinStrings(arg => arg.csArgName + ", ");
        public string csTypedArgsComma => Arguments.JoinStrings(arg => arg.csTypeAndName + ", ");
        public string csReturns => ReturnArguments.JoinStrings(", ", arg => arg.csArgName);
        public string csTypedReturns => ReturnArguments.JoinStrings(", ", arg => arg.csTypeAndName);
        public string csRpcResultClassName => $"{csName}Result";
        public string csRpcQualifiedResultClassName => $"{Service.csName}.{csName}Result";
        public string csRpcTaskClass => ReturnArguments.Any() ? $"{CsVersion.TaskClass}<{csRpcQualifiedResultClassName}>" : CsVersion.TaskClass;
    }

    public partial class ServiceForm
    {
        public string csClassName(Direction direction) => direction == Direction.ServerToClient ? $"{csName}Server" : $"{csName}Client";

        public string csInterfaceName(Direction direction) => $"I{csName}{direction}";

        public bool csBinaryClient => Attribute(CoreAttributes.Client, false) && binaryEnabled;
        public bool csBinaryServer => Attribute(CoreAttributes.Server, false) && binaryEnabled;
        public bool csJsonClient => Attribute(CoreAttributes.Client, false) && jsonEnabled;
        public bool csJsonServer => Attribute(CoreAttributes.Server, false) && jsonEnabled;
        public bool csMessages => Attribute(CsAttributes.Messages, false);
        public bool csGenerateEquals => Attribute(CsAttributes.Equals, false);
    }
}
