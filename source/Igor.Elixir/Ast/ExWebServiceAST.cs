using System;
using System.Collections.Generic;
using System.Text;
using Igor.Text;

namespace Igor.Elixir.AST
{
    public partial class WebServiceForm
    {
        public string exFileName => Attribute(ExAttributes.File, Name.Format(Notation.LowerUnderscore) + ".ex");
    }

    public partial class WebVariable
    {
        public string exName => Attribute(ExAttributes.Name, Name.Format(Notation.LowerUnderscore));
        public string exType => Helper.ExType(Type, true);
    }

    public partial class WebHeader
    {
        public string exStaticString => $@"""{Helper.EscapeString(StaticValue)}""";
        public string exValue => IsStatic ? exStaticString : Var.exName;
        public string exStringValue => IsStatic ? exStaticString : $"{Var.exName}";
        public string exLowerNameString => $@"""{Name.ToLower()}""";
    }

    public partial class WebServiceForm
    {
        public string exBaseUrl => Attribute(ExAttributes.HttpBaseUrl, null);
        public string exHttpBehaviour => Attribute(ExAttributes.HttpBehaviour, $"{Module.exName}.{exName}");
        public string exHttpBehaviourFile => Attribute(ExAttributes.HttpBehaviourFile, exFileName);
    }

    public partial class WebResource
    {
        public bool exConn => Attribute(ExAttributes.HttpConn, false);
        public bool exHttpSession => Attribute(ExAttributes.HttpSession, false);
        public string exHttpSessionKey => Attribute(ExAttributes.HttpSessionKey, null);
        public string exName => Attribute(ExAttributes.Name, Name.Format(Notation.LowerUnderscore));
        public string exHttpServerCallback => Attribute(ExAttributes.HttpCallback, "WebHandler");
        public string exDefaultClientHeaders => Attribute(ExAttributes.HttpDefaultClientHeaders, null);
        public string exWebClientContext => Attribute(ExAttributes.HttpClientContext, null);
        public string exClientHttpOptions => Attribute(ExAttributes.HttpClientHttpOptions, "[]");
        public ExceptionType exHttpExceptionType => Attribute(ExAttributes.HttpExceptionType, ExceptionType.Throw);
        public WebResult exHttpResult => Attribute(ExAttributes.HttpResult, WebResult.Content);
        public string exPostprocessException => Attribute(ExAttributes.HttpPostProcessException);
        public string exFormat400 => Attribute(ExAttributes.HttpFormat400);
        public string exFormat500 => Attribute(ExAttributes.HttpFormat500);
    }

    public partial class WebResponse
    {
    }

    public partial class WebVariable
    {
        public string exAtomName => Helper.AtomName(Attribute(ExAttributes.Name, Name.Format(Notation.LowerUnderscore)));
        public string exDefault => Helper.ExValue(DefaultValue, Type);
        public bool exCowboyParseHeader => Attribute(ExAttributes.HttpCowboyParseHeader, false);
    }

    public partial class WebContent
    {
        public string exRequestName => exVarName("request_content");

        public string exType => Helper.ExType(Type, false);

        public string exVarName(string defaultName) => Var == null ? defaultName : Var.exName;
    }
}
