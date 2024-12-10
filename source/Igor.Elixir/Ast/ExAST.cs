using System;
using Igor.Text;
using System.Linq;
using System.Collections.Generic;

namespace Igor.Elixir.AST
{
    public partial class Module
    {
        public string exFileName => Attribute(ExAttributes.File, Name.Format(Notation.LowerUnderscore) + ".ex");
        public string exName => Attribute(ExAttributes.Name, Name.Format(Notation.UpperCamel));
    }

    public partial class GenericArgument
    {
        public string exName => Helper.ShadowTypeVar(Name.Format(Notation.LowerUnderscore));
        public string exTypeTagVarName => exName + "_type";
    }

    partial class GenericType
    {
        public string exRemoteType(bool shadowGenericArgs) => Prototype.exRemoteTypeInstance(Args, shadowGenericArgs);

        public IDictionary<GenericArgument, SerializationTag> PrepareArgs(System.Func<IType, Statement, SerializationTag> tagger, Statement referrer)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer)));
        }

        public IDictionary<GenericArgument, SerializationTag> PrepareArgs<T>(System.Func<IType, Statement, T, SerializationTag> tagger, Statement referrer, T arg)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer, arg)));
        }
    }

    public partial class Form
    {
        public string exName => Attribute(ExAttributes.Name, Name.Format(Notation.UpperCamel));
        public bool exEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class TypeForm
    {
        public string exAlias => Attribute(ExAttributes.Alias, null);
        public string exLocalType => $"{exLocalTypeName}({exArgs})";
        public string exRemoteType => $"{exRemoteTypeName}({exArgs})";
        public virtual string exLocalTypeName => exAlias ?? "t"; // Helper.AtomName(exName);
        public virtual string exRemoteTypeName => exAlias ?? $"{Module.exName}.{exName}.t";

        public string exLocalTypeInstance(IEnumerable<IType> args) => $"{exLocalTypeName}({args.JoinStrings(", ", t => Helper.ExType(t, false))})";

        public string exRemoteTypeInstance(IEnumerable<IType> args, bool shadowGenericArgs) => $"{exRemoteTypeName}({args.JoinStrings(", ", t => Helper.ExType(t, shadowGenericArgs))})";

        public List<SerializationTag> exArgTags => Args.Select(arg => new SerializationTags.Var(arg)).Cast<SerializationTag>().ToList();
        public string exArgs => Args.JoinStrings(", ", arg => "term");

        public abstract string exGuard(string value);
        public virtual IEnumerable<string> exGuardRequires => Array.Empty<string>();
    }

    public partial class EnumField
    {
        public string exName => Helper.AtomName(Name.Format(Notation.LowerUnderscore));
    }

    public partial class EnumForm
    {
        public string exIntType => Helper.ExIntType(IntType);

        public override string exGuard(string value) => $"{Module.exName}.{exName}.{exGuardName}({value})";
        public override IEnumerable<string> exGuardRequires => $"{Module.exName}.{exName}".Yield();

        public string exGuardName => $"is_{exName.Format(Notation.LowerUnderscore)}";
    }

    public partial class RecordField
    {
        public string exName => Attribute(ExAttributes.Name, Name.Format(Notation.LowerUnderscore));
        public string exAtomName => ":" + exName;
        public string exVarName => Helper.ShadowName(Name.Format(Notation.LowerUnderscore));
        public string exType => Helper.ExType(Type, false);
        public string exDefault => HasDefault ? Helper.ExValue(Default, Type) : "nil";

        public string exDetailedTypeSpec => $"{exName} :: {exType}";
        public string exMapTypeSpec => IsOptional ? $"optional(:{exName}) => {Helper.ExType(NonOptType, false)}" : $":{exName} => {exType}";
    }

    public partial class StructForm
    {
        public string exExceptionMessage => Attribute(ExAttributes.ExceptionMessage, exName);
        public string exVarName => exName.Format(Notation.LowerUnderscore);
        private bool exTuple => Attribute(ExAttributes.Tuple, false);
        private bool exMap => Attribute(ExAttributes.Map, IsPatch);
        private bool exRecord => Attribute(ExAttributes.Record, false);
        public string exRecordName => exName.Format(Notation.LowerUnderscore);
        public StructType exStructType => exTuple ? StructType.Tuple.Instance : exMap ? StructType.Map.Instance : exRecord ? new StructType.Record(exRecordName) : new StructType.Struct(exName);
        public IEnumerable<RecordField> exFields => Fields.Where(f => !f.IsTag);

        public override string exGuard(string value)
        {
            if (exTuple)
                return $"is_tuple({value})";
            else if (exMap)
                return $"is_map({value})";
            else if (exRecord)
                return $"is_record({value}, {exName})";
            else
                return $"is_struct({value}, {Module.exName}.{exName})";
        }
    }

    public partial class VariantForm
    {
        public override string exGuard(string value) => Records.JoinStrings("; ", r => r.exGuard(value));
    }

    public partial class DefineForm
    {
        public override string exGuard(string value) => Helper.ExGuard(Type, value);
        public override string exLocalTypeName => exAlias ?? exName.Format(Notation.LowerUnderscore);
        public override string exRemoteTypeName => exAlias ?? $"{Module.exName}.{exLocalTypeName}";
    }

    public partial class UnionClause
    {
        public string exName => Name.Format(Notation.LowerUnderscore);
        public string exTag => Helper.AtomName(exName);
        public string exVarName => Helper.ShadowName(Name.Format(Notation.UpperCamel));
        public string exValueType => Helper.ExType(Type, false);
        public string exType => IsSingleton ? Helper.AtomName(exName) : (exTagged ? $"{{{Helper.AtomName(exName)}, {exValueType}}}" : exValueType);
        public bool exTagged => Attribute(ExAttributes.Tagged, true);

        public string exGuard(string value)
        {
            if (IsSingleton)
                return $"{value} == {exTag}";
            else if (exTagged)
                return $"elem({value}, 0) == {exTag}";
            else
                return Helper.ExGuard(Type, value);
        }
    }

    public partial class UnionForm
    {
        public override string exGuard(string value) => Clauses.JoinStrings(" or ", r => r.exGuard(value)).Quoted("(", ")");
    }
}
