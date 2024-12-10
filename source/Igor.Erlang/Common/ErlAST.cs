using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.AST
{
    public partial class Module
    {
        private string erlDefaultName => Attribute(ErlAttributes.Name, Name.Format(Notation.LowerUnderscore));

        public string erlFileName => Attribute(ErlAttributes.File, erlDefaultName);
        public string erlName => System.IO.Path.GetFileNameWithoutExtension(erlFileName);
        public string hrlFileName => Attribute(ErlAttributes.HrlFile, erlDefaultName) + ".hrl";
    }

    public partial class Form
    {
        public string erlName => Attribute(ErlAttributes.Name, Name.Format(Notation.LowerUnderscore));
        public bool erlEnabled => Attribute(CoreAttributes.Enabled, true);
    }

    public partial class TypeForm
    {
        public string erlAlias => Attribute(ErlAttributes.Alias, null);
        public PrimitiveType erlPrimitiveType => Attribute(ErlAttributes.Type, PrimitiveType.None);
        public string erlLocalType => $"{erlLocalTypeName}({erlArgs})";
        public string erlRemoteType => $"{erlRemoteTypeName}({erlArgs})";
        public string erlLocalTypeName => erlAlias ?? Helper.AtomName(erlName);
        public string erlRemoteTypeName => erlAlias ?? $"{Module.erlName}:{Helper.AtomName(erlName)}";

        public string erlLocalTypeInstance(IEnumerable<IType> args) => $"{erlLocalTypeName}({args.JoinStrings(", ", t => Helper.ErlType(t, false))})";

        public string erlRemoteTypeInstance(IEnumerable<IType> args, bool shadowGenericArgs) => $"{erlRemoteTypeName}({args.JoinStrings(", ", t => Helper.ErlType(t, shadowGenericArgs))})";

        public List<SerializationTag> erlArgTags => Args.Select(arg => new SerializationTags.Var(arg)).Cast<SerializationTag>().ToList();
        public string erlArgs => Args.JoinStrings(", ", arg => arg.erlName);

        public abstract string erlGuard(string value);
    }

    public partial class GenericArgument
    {
        public string erlName => Name.Format(Notation.UpperCamel);
    }

    partial class GenericType
    {
        public string erlRemoteType(bool shadowGenericArgs) => Prototype.erlRemoteTypeInstance(Args, shadowGenericArgs);

        public IDictionary<GenericArgument, SerializationTag> PrepareArgs(System.Func<IType, Statement, SerializationTag> tagger, Statement referrer)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer)));
        }

        public IDictionary<GenericArgument, SerializationTag> PrepareArgs<T>(System.Func<IType, Statement, T, SerializationTag> tagger, Statement referrer, T arg)
        {
            return Prototype.Args.ZipDictionary(Args.Select(tag => tagger(tag, referrer, arg)));
        }
    }

    public partial class EnumField
    {
        public string erlName => Helper.AtomName(Attribute(ErlAttributes.Name, erlDefaultName));

        public string erlDefaultName => Name.Format(Notation.LowerUnderscore);
    }

    public partial class EnumForm
    {
        public string erlIntType => Helper.ErlIntType(IntType);

        public override string erlGuard(string value) => Fields.JoinStrings("; ", f => $"{value} =:= {f.erlName}");

        public bool erlEnumToInteger => Attribute(ErlAttributes.EnumToInteger, erlBinaryIsSerializerGenerated);
    }

    public partial class RecordField
    {
        public bool erlGenTypeSpec => Attribute(ErlAttributes.RecordFieldTypes, true);
        public bool erlRecordFieldErrors => Attribute(ErlAttributes.RecordFieldErrors, false);

        public string erlName => Helper.AtomName(Name.Format(Notation.LowerUnderscore));
        public string erlVarName => Helper.ShadowName(Name.Format(Notation.UpperCamel));
        public string erlType => Helper.ErlType(Type, false);
        public string erlFieldType => Struct.erlAllowMatchSpec ? Helper.ErlType(Type, true, Struct.IsPatch) + " | '_'" : Helper.ErlType(Type, true, Struct.IsPatch);
        public string erlValue => HasDefault ? erlDefault : "undefined";
        public string erlDefault => HasDefault ? Helper.ErlValue(Default, Type) : $"erlang:error({{required, {erlName}}})";

        public string erlDetailedTypeSpec => $"{erlName} :: {erlType}";
        public string erlMapTypeSpec => IsOptional ? $"{erlName} => {Helper.ErlType(NonOptType, false)}" : $"{erlName} := {erlType}";
    }

    public partial class StructForm
    {
        public string erlVarName => erlName.Format(Notation.UpperCamel);
        public PrimitiveType erlDefaultType => Attribute(ErlAttributes.Type, PrimitiveType.None);
        public bool erlAllowMatchSpec => Attribute(ErlAttributes.AllowMatchSpec, false);
        private bool erlTuple => Attribute(ErlAttributes.Tuple, false);
        private bool erlMap => Attribute(ErlAttributes.Map, false);
        public StructType erlStructType => erlTuple ? StructType.Tuple.Instance : erlMap ? StructType.Map.Instance : new StructType.Record(erlRecordName);
        public string erlRecordName => Helper.AtomName(erlName);
        public bool erlGenDetailedTypeSpec => Attribute(ErlAttributes.RecordTypeFields, false);
        public bool erlInterfaceRecords => Attribute(ErlAttributes.InterfaceRecords, false);
        public IEnumerable<RecordField> erlFields => Fields.Where(f => !f.IsTag);
        public string erlDetailedTypeSpec => erlFields.Any() ? "\n" + erlFields.JoinStrings(",\n", f => f.erlDetailedTypeSpec).Indent(4) : "";
        public string erlTupleTypeSpec => erlFields.JoinStrings(", ", f => f.erlType);
        public string erlMapTypeSpec => $"#{{{erlFields.JoinStrings(", ", f => f.erlMapTypeSpec)}}}";

        public override string erlGuard(string value)
        {
            if (erlTuple)
                return $"is_tuple({value})";
            else if (erlMap)
                return $"is_map({value})";
            else
                return $"is_record({value}, {erlRecordName})";
        }
    }

    public partial class VariantForm
    {
        public override string erlGuard(string value) => Records.JoinStrings("; ", r => r.erlGuard(value));
    }

    public partial class DefineForm
    {
        public override string erlGuard(string value) => Helper.ErlGuard(Type, value);
    }

    public partial class UnionClause
    {
        public string erlName => Name.Format(Notation.LowerUnderscore);
        public string erlTag => Helper.AtomName(erlName);
        public string erlVarName => Helper.ShadowName(Name.Format(Notation.UpperCamel));
        public string erlValueType => Helper.ErlType(Type, false);
        public string erlType => IsSingleton ? Helper.QuotedAtomName(erlName) : (erlTagged ? $"{{{Helper.QuotedAtomName(erlName)}, {erlValueType}}}" : erlValueType);
        public bool erlTagged => Attribute(ErlAttributes.Tagged, true);

        public string erlGuard(string value)
        {
            if (IsSingleton)
                return $"{value} =:= {erlTag}";
            else if (erlTagged)
                return $"element(1, {value}) =:= {erlTag}";
            else
                return Helper.ErlGuard(Type, value);
        }

        public string erlGenericArgs
        {
            get
            {
                if (!Union.IsGeneric)
                    return "";
                string GenericArgVar(GenericArgument arg)
                {
                    if (Helper.HasGenericArg(Type, arg))
                        return arg.erlName;
                    else
                        return "_" + arg.erlName;
                }
                return Union.Args.JoinStrings(arg => $", {GenericArgVar(arg)}");
            }
        }
    }

    public partial class UnionForm
    {
        public override string erlGuard(string value) => Clauses.JoinStrings("; ", r => r.erlGuard(value));
    }
}
