using Igor.Go.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Go
{
    public abstract class GoType
    {
        public abstract string Name { get; }

        public virtual IEnumerable<string> Imports => Enumerable.Empty<string>();
    }

    public class GoPrimitiveType : GoType
    {
        public PrimitiveType Type { get; }

        public GoPrimitiveType(PrimitiveType type)
        {
            Type = type;
        }

        public override string Name => Helper.PrimitiveTypeString(Type);
    }

    public class GoNullableType : GoType
    {
        public GoType ValueType { get; }

        public GoNullableType(GoType valueType) => ValueType = valueType;

        public override string Name => $"*{ValueType.Name}";
    }

    public class GoArrayType : GoType
    {
        public GoType ItemType { get; }

        public GoArrayType(GoType itemType) => ItemType = itemType;

        public override string Name => $"[]{ItemType.Name}";
    }

    public class GoMapType : GoType
    {
        public GoType KeyType { get; }
        public GoType ValueType { get; }

        public GoMapType(GoType keyType, GoType valueType)
        {
            KeyType = keyType;
            ValueType = valueType;
        }

        public override string Name => $"map[{KeyType.Name}]{ValueType.Name}";
    }

    public class GoUserType : GoType
    {
        public TypeForm Form { get; }

        public GoUserType(TypeForm form)
        {
            Form = form;
        }

        public override string Name => Form.goName;

        public override IEnumerable<string> Imports => Form.goImports;
    }

    public class GoAliasType : GoUserType
    {
        public bool isOptional { get; }

        public GoAliasType(TypeForm typeForm, bool isOptional) : base(typeForm)
        {
            this.isOptional = isOptional;
        }

        public override string Name => isOptional ? Form.goOptAlias : Form.goAlias;
    }

    public class GoGenericArgument : GoType
    {
        public GenericArgument Arg { get; }

        public GoGenericArgument(GenericArgument arg)
        {
            this.Arg = arg;
        }

        public override string Name => Arg.goName;
    }

    public class GoGenericType : GoType
    {
        public GoType ValueType { get; }
        public GoType[] Args { get; }

        public GoGenericType(GoType goType, GoType[] goArgs)
        {
            this.ValueType = goType;
            this.Args = goArgs;
        }

        public override string Name => $"{ValueType.Name}[{Args.JoinStrings(", ", a => a.Name)}]";
    }
}
