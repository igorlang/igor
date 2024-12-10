using Igor.Text;
using System.Collections.Generic;

namespace Igor.Go.AST
{
    public partial class Statement
    {
        public string goComment => Attribute(CoreAttributes.Annotation, null);
    }

    public partial class Module
    {
        public string goFileName => Attribute(GoAttributes.File, Name.Format(Notation.LowerUnderscore) + ".go");
        public string goPackage => Attribute(GoAttributes.Package, "protocol");
        public IReadOnlyList<string> goImports => ListAttribute(GoAttributes.Import);
    }

    public partial class Form
    {
        public string goName => Attribute(GoAttributes.Name, Name.Format(Notation.UpperCamel, upperInitialisms: true));
        public string goAlias => Attribute(GoAttributes.Alias, null);
        public string goOptAlias => Attribute(GoAttributes.OptionalAlias, goAlias);
        public bool goEnabled => Attribute(CoreAttributes.Enabled, true);
        public IReadOnlyList<string> goImports => ListAttribute(GoAttributes.Import);
    }

    public partial class GenericArgument
    {
        public string goName => Name.Format(Notation.UpperCamel, upperInitialisms: true);
        public GoType goType => new GoGenericArgument(this);
    }

    public partial class TypeForm
    {
        public virtual GoType goType => goAlias == null ? new GoUserType(this) : new GoAliasType(this, false);
        public string goShortVarName => goName.Format(Notation.FirstLetterLastWord);
        public bool goJsonEnabled => goEnabled && jsonEnabled;
    }

    public partial class EnumField
    {
        public Notation goFieldNotation => Attribute(GoAttributes.FieldNotation, Notation.UpperCamel);
        public bool goEnumTypePrefix => Attribute(GoAttributes.EnumTypePrefix, false);
        public string goPrefix => Attribute(GoAttributes.Prefix, goEnumTypePrefix ? Enum.goName : "");
        public string goName => Attribute(GoAttributes.Name, goPrefix + Name.Format(goFieldNotation, upperInitialisms: true));
        public string goJsonString => jsonKey.Quoted();
    }

    public partial class EnumForm
    {
        public string goBaseType => goStringEnum ? "string" : Helper.GoIntType(IntType);
        public bool goStringEnum => Attribute(GoAttributes.StringEnum, false);
    }

    public partial class RecordField
    {
        public Notation goFieldNotation => Attribute(GoAttributes.FieldNotation, Notation.UpperCamel);
        public string goName => Attribute(GoAttributes.Name, Helper.ShadowName(Name.Format(goFieldNotation, upperInitialisms: true)));
        public string goVarName => Attribute(GoAttributes.Name, Name.Format(Notation.LowerCamel, upperInitialisms: true));
        public GoType goType => Helper.TargetType(Type);
        public bool goJsonOmitempty => Attribute(GoAttributes.JsonOmitEmpty, false);
        public bool goPtr => Attribute(GoAttributes.Ptr, false);
        // public string goDefault => HasDefault ? Helper.GoValue(Default, Type) : "nil";
    }

    public partial class StructForm
    {
    }

    public partial class RecordForm
    {
    }

    public partial class VariantForm
    {
        public string goInterface => Attribute(GoAttributes.Interface);
    }

    public partial class DefineForm
    {
        public GoType goTypeDefinition => Helper.TargetType(Type);
        // public override GoType goType => Helper.TargetType(Type);
    }

    public partial class UnionForm : TypeForm
    {
    }
}
