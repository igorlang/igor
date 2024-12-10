using Igor.Text;
using Igor.UE4.Model;
using System.Text;

namespace Igor.UE4.AST
{
    public partial class WebServiceForm
    {
        internal override string ueDefaultName => "F" + uePrefix + base.ueDefaultName;
        public string ueBaseUrl => Attribute(UeAttributes.HttpBaseUrl, null);
    }

    public partial class WebResource
    {
        public string uePath
        {
            get
            {
                var sb = new StringBuilder();
                var varIndex = 0;
                foreach (var segment in Path)
                {
                    sb.Append("/");
                    if (segment.IsStatic)
                    {
                        sb.Append(segment.StaticValue);
                    }
                    else
                    {
                        sb.Append($@"{{{varIndex}}}");
                        varIndex++;
                    }
                }
                return sb.ToString();
            }
        }

        public string ueName => Attribute(UeAttributes.Name, Name.Format(Notation.UpperCamel));
        public UeType ueRequestBodyType => Helper.TargetType(RequestBodyType);
        public UeType ueResponseBodyType => Helper.TargetType(ResponseBodyType);

        public UeHttpClientRequestSetup uePathSetup => Attribute(UeAttributes.HttpClientPathSetup, UeHttpClientRequestSetup.Args);
        public UeHttpClientRequestSetup ueQuerySetup => Attribute(UeAttributes.HttpClientQuerySetup, UeHttpClientRequestSetup.Args);
        public UeHttpClientRequestSetup ueHeaderSetup => Attribute(UeAttributes.HttpClientHeaderSetup, UeHttpClientRequestSetup.Args);
        public UeHttpClientRequestSetup ueContentSetup => Attribute(UeAttributes.HttpClientContentSetup, UeHttpClientRequestSetup.Args);
        public bool ueLazy => Attribute(UeAttributes.HttpClientLazy, false);
    }

    public partial class WebContent
    {
        public UeType ueType => Helper.TargetType(Type);

        public string ueName(string @default) => Var == null ? @default : Var.ueName;
    }

    public partial class WebVariable
    {
        public bool QueryUnfold => Attribute(CoreAttributes.HttpUnfold, false);
        public string QuerySeparator => Attribute(CoreAttributes.HttpSeparator, ",");

        public string ueName => Name.Format(Notation.UpperCamel);
        public UeType ueType => Helper.TargetType(Type);

        public string ueRequireNotNull => $@"check({ueName} != nullptr)";

        public string ueStringValue => ueType.VarToString(ueName);

        public UeHttpClientRequestSetup ueSetup => Attribute(UeAttributes.HttpClientSetup, ueDefaultSetup);

        private UeHttpClientRequestSetup ueDefaultSetup
        {
            get
            {
                switch (ParameterType)
                {
                    case WebParameterType.Content: return Resource.ueContentSetup;
                    case WebParameterType.Path: return Resource.uePathSetup;
                    case WebParameterType.Query: return Resource.ueQuerySetup;
                    case WebParameterType.Header: return Resource.ueHeaderSetup;
                    default: return UeHttpClientRequestSetup.Args;
                }
            }
        }
    }
}
