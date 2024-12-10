using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Lua.AST
{
    public partial class Statement
    { }

    public partial class Module
    {
        public IReadOnlyList<string> luaRequires => ListAttribute(LuaAttributes.Require);
        public string luaFileName => Attribute(LuaAttributes.File, Name.Format(Notation.LowerUnderscore) + ".lua");
    }

    public partial class Form
    {
        public string luaName => Attribute(LuaAttributes.Name, Name.Format(Notation.UpperCamel));
        public string luaNamespace => Attribute(LuaAttributes.Namespace, null);
        public string luaQualifiedName => luaNamespace == null ? luaName : $"{luaNamespace}.{luaName}";
        public string luaRelativeName(string mod) => string.IsNullOrEmpty(mod) || mod != Module.luaFileName ? luaQualifiedName : luaName;
        public bool luaEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class GenericArgument
    {
        public string luaName => Name.Format(Notation.LowerCamel);
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
        public string luaEnumAlias => Attribute(LuaAttributes.EnumAlias);

        public string luaTypeName => luaName ?? luaEnumAlias;

        public bool luaGenerateDeclaration => luaEnabled && luaEnumAlias == null;

        public List<ISerializationTag> luaArgTags => Args.Select(arg => new SerializationTag.Var(arg)).Cast<ISerializationTag>().ToList();
    }

    public partial class EnumField
    {
        public Notation luaFieldNotation => Attribute(LuaAttributes.FieldNotation, Notation.UpperCamel);
        public string luaName => Name.Format(luaFieldNotation);
        public string luaJsonString => jsonKey.Quoted();
    }

    public partial class EnumForm
    {
        public EnumStyle luaEnumStyle => Attribute(LuaAttributes.EnumStyle, EnumStyle.Table);
    }

    public partial class RecordField
    {
        public Notation luaFieldNotation => Attribute(LuaAttributes.FieldNotation, Notation.LowerUnderscore);
        public string luaName => Attribute(LuaAttributes.Name, Name.Format(luaFieldNotation));
        public string luaFieldName => luaName;
        public string luaDefault => HasDefault ? Helper.LuaValue(Default, Type) : "nil";
        public string luaDefaultRelative(string modName) => HasDefault ? Helper.LuaValue(Default, Type, modName) : "nil";
        public int? luaIndex => Attribute(LuaAttributes.Index);
        public string luaIndexer => luaIndex.HasValue ? $"[{luaIndex.Value}]" : $".{luaFieldName}";
        public string luaConstructionKey => luaIndex.HasValue ? $"[{luaIndex.Value}]" : luaFieldName;
    }

    public partial class StructForm
    {
        public RecordStyle luaRecordStyle => Attribute(LuaAttributes.RecordStyle, RecordStyle.Class);
    }

    public partial class VariantForm
    {
        public bool luaVariantSerializerLookup => Attribute(LuaAttributes.VariantSerializerLookup, false);
    }

    public partial class DefineForm
    {
    }

    public partial class UnionForm : TypeForm
    {
    }

    public partial class FunctionArgument
    {
        public string luaName => Helper.ShadowName(Name.Format(Notation.LowerUnderscore));
    }

    public partial class ServiceFunction
    {
        public string luaName => Name.Format(Notation.LowerUnderscore);
        public string luaArgs => Arguments.JoinStrings(", ", arg => arg.luaName);
    }

    public partial class ServiceForm
    {
        public string luaFileName => Attribute(LuaAttributes.File, luaName.Format(Notation.LowerUnderscore) + ".lua");
        public bool luaClient => Attribute(CoreAttributes.Client, false);
        public bool luaServer => Attribute(CoreAttributes.Server, false);

        public string luaServiceName(Direction direction) => luaName + (direction == Direction.ClientToServer ? "Client" : "Server");
    }
}
