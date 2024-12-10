using System.Collections.Generic;
using System.Linq;

namespace Igor.Elixir
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

    public static class ExAttributes
    {
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor HttpBehaviourFile = new StringAttributeDescriptor("http.behaviour_file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor HttpBehaviour = new StringAttributeDescriptor("http.behaviour", IgorAttributeTargets.Any);

        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Tuple = new BoolAttributeDescriptor("tuple", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor Record = new BoolAttributeDescriptor("record", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor Map = new BoolAttributeDescriptor("map", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor Tagged = new BoolAttributeDescriptor("tagged", IgorAttributeTargets.UnionClause, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor ExceptionMessage = new StringAttributeDescriptor("exception_message", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor JsonCompatible = new BoolAttributeDescriptor("json.compatible", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor JsonCustom = new StringAttributeDescriptor("json.custom", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor StringCustom = new StringAttributeDescriptor("string.custom", IgorAttributeTargets.Type);
       
        public static readonly StringAttributeDescriptor HttpQueryCustom = new StringAttributeDescriptor("http.query_custom", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor HttpUriCustom = new StringAttributeDescriptor("http.uri_custom", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor HttpConn = new BoolAttributeDescriptor("http.conn", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor HttpSession = new BoolAttributeDescriptor("http.session", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpSessionKey = new StringAttributeDescriptor("http.session_key", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
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
        public static readonly StringAttributeDescriptor HttpClientHttpOptions = new StringAttributeDescriptor("http.client_http_options", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpPostProcessException = new StringAttributeDescriptor("http.postprocess_exception", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpFormat400 = new StringAttributeDescriptor("http.format_400", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpFormat500 = new StringAttributeDescriptor("http.format_500", IgorAttributeTargets.WebResource, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor ServerFile = new StringAttributeDescriptor("server_file", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ClientFile = new StringAttributeDescriptor("client_file", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor Dispatcher = new StringAttributeDescriptor("dispatcher", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ServerDispatcher = new StringAttributeDescriptor("server_dispatcher", IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor ClientDispatcher = new StringAttributeDescriptor("client_dispatcher", IgorAttributeTargets.Service);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static ExAttributes()
        {
            var props = typeof(ExAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
