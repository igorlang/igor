using Igor.Text;

namespace Igor.TypeScript.AST
{
    public partial class WebServiceForm
    {
        public string tsFileName => Attribute(TsAttributes.File, tsName.Format(Notation.LowerHyphen) + ".ts");
        public string tsBaseUrl => Attribute(TsAttributes.BaseUrl, string.Empty);
    }

    public partial class WebResource
    {
        public string tsName => Attribute(TsAttributes.Name, Name.Format(Notation.LowerCamel));
        public TsType tsRequestBodyType => Helper.TargetType(RequestBodyType);
        public TsType tsResponseBodyType => Helper.TargetType(ResponseBodyType);
    }

    public partial class WebContent
    {
        public TsType tsType => Helper.TargetType(Type);

        public string tsName => Var == null ? "requestContent" : Var.tsName;
    }

    public partial class WebHeader
    {
        public string tsHeaderValue(string ns) => IsStatic ? StaticValue.Quoted() : Var.tsType.toString(Var.tsName, ns);
    }

    public partial class WebVariable
    {
        public string tsName => Name.Format(Notation.LowerCamel);
        public TsType tsType => Helper.TargetType(Type, this);

        public string tsArg(string ns)
        {
            if (DefaultValue != null)
                return $"{tsName}: {tsType.relativeName(ns)} = {tsType.FormatValue(DefaultValue, ns, Location)}";
            else if (Type is BuiltInType.Optional)
                return $"{tsName}: {tsType.relativeName(ns)} = null";
            else
                return $"{tsName}: {tsType.relativeName(ns)}";
        }
    }
}
