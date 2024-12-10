using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Python.AST
{
    public partial class Statement
    { }

    public partial class Module
    {
        public IReadOnlyList<string> pyImports => ListAttribute(PythonAttributes.Import);
        public string pyFileName => Attribute(PythonAttributes.File, Name.Format(Notation.LowerUnderscore) + ".py");
    }

    public partial class Form
    {
        public string pyName => Attribute(PythonAttributes.Name, Name.Format(Notation.UpperCamel));
        public bool pyEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class GenericArgument
    {
        public string pyName => Name.Format(Notation.LowerCamel);
    }

    partial class GenericType
    {
        public IDictionary<GenericArgument, ISerializationTag> PrepareArgs(System.Func<IType, Statement, ISerializationTag> tagger, Statement referrer)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer)));
        }

        public IDictionary<GenericArgument, ISerializationTag> PrepareArgs<T>(System.Func<IType, Statement, T, ISerializationTag> tagger, Statement referrer, T arg)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer, arg)));
        }
    }

    public partial class TypeForm
    {
        public string pyEnumAlias => Attribute(PythonAttributes.EnumAlias);

        public string pyTypeName => pyName ?? pyEnumAlias;

        public virtual bool pyGenerateDeclaration => pyEnabled && pyEnumAlias == null;

        public List<ISerializationTag> pyArgTags => Args.Select(arg => new SerializationTag.Var(arg)).Cast<ISerializationTag>().ToList();
    }

    public partial class EnumField
    {
        public Notation pyFieldNotation => Attribute(PythonAttributes.FieldNotation, Notation.LowerUnderscore);
        public string pyName => Name.Format(pyFieldNotation);
        public string pyJsonString => jsonKey.Quoted();
    }

    public partial class EnumForm
    {
    }

    public partial class RecordField
    {
        public Notation pyFieldNotation => Attribute(PythonAttributes.FieldNotation, Notation.LowerUnderscore);
        public string pyName => Attribute(PythonAttributes.Name, Name.Format(pyFieldNotation));
        public string pyFieldName => pyName;
        public string pyDefault => HasDefault ? Helper.PyValue(Default, Type) : "None";
    }

    public partial class StructForm
    {
        public override bool pyGenerateDeclaration => base.pyGenerateDeclaration && Attribute(PythonAttributes.Classes, true);
    }

    public partial class DefineForm
    {
    }

    public partial class UnionForm : TypeForm
    {
    }

    public partial class FunctionArgument
    {
        public string pyName => Helper.ShadowName(Name.Format(Notation.LowerUnderscore));
    }

    public partial class ServiceFunction
    {
        public string pyName => Name.Format(Notation.LowerUnderscore);
        public string pyArgs => Arguments.JoinStrings(", ", arg => arg.pyName);
        public string erlRets => ReturnArguments.JoinStrings(", ", arg => arg.pyName);
        public string pyArgComma => Arguments.Count != 0 ? ", " : "";
        public string pyRetComma => ReturnArguments.Count != 0 ? ", " : "";

        public string erlFailFunName => $"fail_{pyName}";
        public string erlReplyFunName => $"reply_{pyName}";
    }

    public partial class ServiceForm
    {
        public string pyFileName => Attribute(PythonAttributes.File, pyName.Format(Notation.LowerUnderscore) + ".py");
        public bool pyClient => Attribute(CoreAttributes.Client, false);
        public bool erlServer => Attribute(CoreAttributes.Server, false);

        public string pyServiceName(Direction direction) => pyName + (direction == Direction.ClientToServer ? "Client" : "Server");
    }
}
