using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang
{
    public enum ExceptionType
    {
        Throw,
        Error,
        Exit,
        Return,
    }

    public enum WebResult
    {
        Content,
        Full,
    }

    public static class ErlAttributes
    {
        public static readonly StringAttributeDescriptor SrcPath = new StringAttributeDescriptor("src_path", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor IncludePath = new StringAttributeDescriptor("include_path", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor HrlFile = new StringAttributeDescriptor("hrl_file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Tuple = new BoolAttributeDescriptor("tuple", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor Map = new BoolAttributeDescriptor("map", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor Tagged = new BoolAttributeDescriptor("tagged", IgorAttributeTargets.UnionClause, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor EnumToInteger = new BoolAttributeDescriptor("enum_to_integer", IgorAttributeTargets.Enum, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor AllowMatchSpec = new BoolAttributeDescriptor("allow_match_spec", IgorAttributeTargets.Record);
        public static readonly EnumAttributeDescriptor<PrimitiveType> Type = new EnumAttributeDescriptor<PrimitiveType>("type", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor Include = new StringAttributeDescriptor("include", IgorAttributeTargets.Module | IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor IncludeLib = new StringAttributeDescriptor("include_lib", IgorAttributeTargets.Module | IgorAttributeTargets.Service);
        public static readonly BoolAttributeDescriptor JsonCompatible = new BoolAttributeDescriptor("json.compatible", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor JsonPacker = new StringAttributeDescriptor("json.packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor JsonParser = new StringAttributeDescriptor("json.parser", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor BinaryPacker = new StringAttributeDescriptor("binary.packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor BinaryParser = new StringAttributeDescriptor("binary.parser", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor XmlPacker = new StringAttributeDescriptor("xml.packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor XmlParser = new StringAttributeDescriptor("xml.parser", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor StringPacker = new StringAttributeDescriptor("string.packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor StringParser = new StringAttributeDescriptor("string.parser", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor HttpQueryPacker = new StringAttributeDescriptor("http.query_packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor HttpQueryParser = new StringAttributeDescriptor("http.query_parser", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor HttpUriPacker = new StringAttributeDescriptor("http.uri_packer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor HttpUriParser = new StringAttributeDescriptor("http.uri_parser", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor InterfaceRecords = new BoolAttributeDescriptor("interface_records", IgorAttributeTargets.Interface, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor RecordFieldTypes = new BoolAttributeDescriptor("record_field_types", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor RecordTypeFields = new BoolAttributeDescriptor("record_type_fields", IgorAttributeTargets.Record, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor RecordFieldErrors = new BoolAttributeDescriptor("record_field_errors", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor HttpCowboyReq = new BoolAttributeDescriptor("http.cowboy_req", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor HttpCowboyParseHeader = new BoolAttributeDescriptor("http.cowboy_parse_header", IgorAttributeTargets.WebResource);
        public static readonly StringAttributeDescriptor HttpBaseUrl = new StringAttributeDescriptor("http.base_url", IgorAttributeTargets.WebService);
        public static readonly StringAttributeDescriptor HttpCallback = new StringAttributeDescriptor("http.callback", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpHandler = new StringAttributeDescriptor("http.handler", IgorAttributeTargets.WebService);
        public static readonly StringAttributeDescriptor HttpBodyReader = new StringAttributeDescriptor("http.body_reader", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<ExceptionType> HttpExceptionType = new EnumAttributeDescriptor<ExceptionType>("http.exception_type", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<WebResult> HttpResult = new EnumAttributeDescriptor<WebResult>("http.result", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpHandlerPrefix = new StringAttributeDescriptor("http.handler_prefix", IgorAttributeTargets.WebService);
        public static readonly StringAttributeDescriptor HttpDefaultClientHeaders = new StringAttributeDescriptor("http.default_client_headers", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpClientContext = new StringAttributeDescriptor("http.client_context", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpHttpcStream = new StringAttributeDescriptor("http.httpc_stream", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpClientHttpOptions = new StringAttributeDescriptor("http.client_http_options", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor ServerFile = new StringAttributeDescriptor("server_file", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ClientFile = new StringAttributeDescriptor("client_file", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor Dispatcher = new StringAttributeDescriptor("dispatcher", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ServerDispatcher = new StringAttributeDescriptor("server_dispatcher", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ClientDispatcher = new StringAttributeDescriptor("client_dispatcher", IgorAttributeTargets.Service);
        public static readonly BoolAttributeDescriptor HttpClientLog = new BoolAttributeDescriptor("http.client.log", IgorAttributeTargets.WebService, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static ErlAttributes()
        {
            var props = typeof(ErlAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
