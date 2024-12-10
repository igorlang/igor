using Igor.Text;
using Igor.UE4.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Json;

namespace Igor.UE4.AST
{
    public partial class Statement
    {
        public string ueLogCategory => Attribute(UeAttributes.LogCategory, "LogTemp");
        public string ueApiMacro => Attribute(UeAttributes.ApiMacro, null);
    }

    public partial class Module
    {
        public bool ueEnabled => Attribute(CoreAttributes.Enabled, true);
        internal string ueName => Attribute(UeAttributes.Name, Name.Format(Notation.UpperCamel));

        /// <summary>
        /// File path of the cpp file generated for this module
        /// </summary>
        public string ueCppFile => Attribute(UeAttributes.CppFile, Path.Combine(ueCppPath, ueName + ".cpp"));

        /// <summary>
        /// File path of the header file generated for this module
        /// </summary>
        public string ueHFile => Attribute(UeAttributes.HFile,  Path.Combine(ueHPath, ueName + ".h"));

        /// <summary>
        /// File path of the cpp file generated for this module
        /// </summary>
        public string ueCppPath => Attribute(UeAttributes.CppPath, "");

        /// <summary>
        /// File path of the header file generated for this module
        /// </summary>
        public string ueHPath => Attribute(UeAttributes.HPath,  "");
    }

    public partial class GenericArgument
    {
        public string ueName => Name.Format(Notation.UpperCamel);
    }

    public partial class Form
    {
        /// <summary>
        /// Path to the Igor standard library, used for all Igor includes. 
        /// Value of igor_path attribute.
        /// Empty by default
        /// </summary>
        public string ueIgorPath => Attribute(UeAttributes.IgorPath, string.Empty);
        public virtual bool ueEnabled => Attribute(CoreAttributes.Enabled, true);
        public string ueName => Attribute(UeAttributes.Name, ueDefaultName);
        public string ueVarName => Name.Format(Notation.UpperCamel);
        internal virtual string ueDefaultName => Name.Format(Notation.UpperCamel);

        /// <summary>
        /// C++ namespace for this declaration (nested namespaces are separated with ::)
        /// </summary>
        public string ueNamespace => Attribute(UeAttributes.Namespace, null);

        /// <summary>
        /// Project prefix for all generated types
        /// </summary>
        public string uePrefix => Attribute(UeAttributes.Prefix, string.Empty);
        public IReadOnlyList<string> ueHIncludes => ListAttribute(UeAttributes.HInclude);
    }

    public partial class TypeForm
    {
        public abstract UeType ueType { get; }
        public UeType[] ueArgs => Args.Select(Helper.TargetType).ToArray();
        public string ueAlias => Attribute(UeAttributes.Alias);
        public virtual bool ueGenerated => ueEnabled && ueAlias == null;
    }

    public partial class EnumField
    {
        public string ueName => Attribute(UeAttributes.Name, Name.Format(Notation.UpperCamel));
        public string ueRelativeQualifiedName(string ns) => $"{Enum.ueType.RelativeName(ns)}::{ueName}";
        public ImmutableJsonObject ueMeta => Attribute(UeAttributes.Meta, ImmutableJsonObject.Empty);
    }

    public partial class EnumForm
    {
        public override UeType ueType => new UeEnumType(this);
        internal override string ueDefaultName => "E" + uePrefix + base.ueDefaultName;
        public string ueIntType => Helper.UeIntType(IntType);
        public bool ueBlueprintType => Attribute(UeAttributes.BlueprintType, false);
        public bool ueUEnum => Attribute(UeAttributes.UEnum, false);
    }

    public partial class RecordField
    {
        public bool ueIgnore => Attribute(UeAttributes.Ignore, false);
        private string ueVarPrefix => ueType.VarPrefix;
        public string ueName => Attribute(UeAttributes.Name, ueVarPrefix + Name.Format(Notation.UpperCamel));
        public string ueCategory => Attribute(UeAttributes.Category);
        public UeType ueType => Helper.TargetType(Type);
        public string ueValue(string ns) => ueType.FormatValue(Default, ns);
        public string ueTagGetter => $"Get{ueName}";

        public bool ueUProperty => Attribute(UeAttributes.UProperty, false);
        public bool ueBlueprintReadWrite => Attribute(UeAttributes.BlueprintReadWrite, false);
        public bool ueBlueprintReadOnly => Attribute(UeAttributes.BlueprintReadOnly, false);
        public bool ueEditAnywhere => Attribute(UeAttributes.EditAnywhere, false);
        public bool ueEditDefaultsOnly => Attribute(UeAttributes.EditDefaultsOnly, false);
        public bool ueVisibleAnywhere => Attribute(UeAttributes.VisibleAnywhere, false);
        public bool ueVisibleDefaultsOnly => Attribute(UeAttributes.VisibleDefaultsOnly, false);
    }

    public partial class StructForm
    {
        public string ueBaseType => Attribute(UeAttributes.BaseType, null);

        /// <summary>
        /// Generate USTRUCT macro for this struct
        /// </summary>
        public bool ueUStruct => Attribute(UeAttributes.UStruct, false);

        /// <summary>
        /// Generate UCLASS macro for this class
        /// </summary>
        public bool ueUClass => Attribute(UeAttributes.UClass, false);
        public bool uePtr => Attribute(UeAttributes.Ptr, true);
        public bool ueBlueprintType => Attribute(UeAttributes.BlueprintType, false);
        public override UeType ueType => uePtr ? (UeType)new UeStructPtrType(this, ueArgs) : new UeStructType(this, ueArgs);
    }

    public partial class InterfaceForm
    {
        public override bool ueEnabled => base.ueEnabled && Attribute(UeAttributes.Interfaces, false);
    }

    public partial class RecordForm
    {
        internal override string ueDefaultName => IsGeneric ? "T" + uePrefix + base.ueDefaultName.RemovePrefix("T") : "F" + uePrefix + base.ueDefaultName;
    }

    public partial class VariantForm
    {
        internal override string ueDefaultName => "F" + uePrefix + base.ueDefaultName;
    }

    public partial class UnionForm
    {
        public override UeType ueType => throw new NotImplementedException();
    }

    public partial class DefineForm
    {
        public bool ueTypedef => Attribute(UeAttributes.Typedef, true);
        public override bool ueGenerated => base.ueGenerated && ueTypedef;
        public override UeType ueType => ueTypedef || ueAlias != null ? new UeTypedefType(this) : ueTargetType;
        internal override string ueDefaultName => IsGeneric ? "T" + uePrefix + base.ueDefaultName.RemovePrefix("T") : "F" + uePrefix + base.ueDefaultName;
        public UeType ueTargetType => Helper.TargetType(Type);
    }
}
