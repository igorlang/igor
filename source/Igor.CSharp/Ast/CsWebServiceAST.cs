using Igor.Text;
using System;
using System.Linq;
using System.Text;

namespace Igor.CSharp.AST
{
    public partial class WebResource
    {
        public string csPath
        {
            get
            {
                bool isStatic = Path.All(p => p.IsStatic);
                var sb = new StringBuilder();
                if (!isStatic)
                    sb.Append("$");
                sb.Append("\"");
                bool isFirst = true;
                foreach (var segment in Path)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append("/");
                    if (segment.IsStatic)
                        sb.Append(segment.StaticValue);
                    else
                        sb.AppendFormat(@"{{{0}}}", segment.Var.csName);
                }
                sb.Append("\"");
                return sb.ToString();
            }
        }

        public string csName => Attribute(CsAttributes.Name, Name.Format(Notation.UpperCamel) + "Async");
        public CsType csRequestBodyType => Helper.TargetType(RequestBodyType, csTypeContext);
        public CsType csResponseBodyType => Helper.TargetType(ResponseBodyType, csTypeContext);
    }

    public partial class WebContent
    {
        public CsType csType => Helper.TargetType(Type, Resource.csTypeContext);

        public string csName => Var == null ? "requestContent" : Var.csName;

        public string csRequestContentString(string ns)
        {
            switch (Format)
            {
                case DataFormat.Json:
                case DataFormat.Default:
                    return $"{csType.jsonSerializer(ns)}.Serialize({csName}).ToString()";
                default:
                    return $@"{csName}";
            }
        }
    }

    public partial class WebHeader
    {
        public string csHeaderValue => IsStatic ? StaticValue.Quoted() : Var.csName;
    }

    public partial class WebVariable
    {
        public bool QueryUnfold => Attribute(CoreAttributes.HttpUnfold, false);
        public string QuerySeparator => Attribute(CoreAttributes.HttpSeparator, ",");

        public string csName => Name.Format(Notation.LowerCamel);
        public CsType csType => Helper.TargetType(Type, csTypeContext);

        public string csRequireNotNull => $@"if ({csName} == null)
    throw new System.ArgumentNullException({CsVersion.NameOf(csName)});";

        public string csArg(string ns)
        {
            if (DefaultValue != null)
                return $"{csType.relativeName(ns)} {csName} = {csType.FormatValue(DefaultValue, ns, Location)}";
            else if (Type is BuiltInType.Optional)
                return $"{csType.relativeName(ns)} {csName} = null";
            else
                return $"{csType.relativeName(ns)} {csName}";
        }
    }

    public partial class WebQueryParameter
    {
        public string csAppendQuery(string ns)
        {
            if (IsStatic)
            {
                return $@"queryBuilder.AppendParameter(""{Parameter}"", ""{StaticValue}"");";
            }
            else
            { 
                var formatter = Format == DataFormat.Json ? "UriFormatter.String" : Var.csType.nonOptType.uriFormatter(ns, Var);
                if (Var.DefaultValue != null || Var.Type is BuiltInType.Optional)
                {
                    var value = Var.DefaultValue != null ?
                        Var.csType.FormatValue(Var.DefaultValue, ns, Var.Location) : "null";
                    var dotValue = Var.csType is CsNullableType ? ".Value" : "";
                    var val = $"{Var.csName}{dotValue}";
                    if (Format == DataFormat.Json)
                        val = $"{Var.csType.nonOptType.jsonSerializer(ns)}.Serialize({val}).ToString()";
                    return $@"if ({Var.csType.notEquals(Var.csName, value)})
{formatter}.AppendQueryParameter(queryBuilder, ""{Parameter}"", {val});";
                }
                else
                {
                    var val = Var.csName;
                    if (Format == DataFormat.Json)
                        val = $"{Var.csType.jsonSerializer(ns)}.Serialize({val}).ToString()";
                    return $@"{formatter}.AppendQueryParameter(queryBuilder, ""{Parameter}"", {val});";
                }
            }
        }
    }
}
