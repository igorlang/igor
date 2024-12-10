using Igor.Text;
using System.Collections.Generic;

namespace Igor.Erlang.AST
{
    public partial class WebHeader
    {
        public string erlStaticBinary => $@"<<""{Helper.EscapeString(StaticValue)}"">>";
        public string erlStaticString => $@"""{Helper.EscapeString(StaticValue)}""";
        public string erlValue => IsStatic ? erlStaticBinary : Var.erlName;
        public string erlStringValue => IsStatic ? erlStaticString : $"[{Var.erlName}]";
        public string erlLowerNameBinary => $@"<<""{Name.ToLower()}"">>";
        public string erlLowerNameString => $@"""{Name.ToLower()}""";
    }

    public partial class WebServiceForm
    {
        public IReadOnlyList<string> erlIncludes => ListAttribute(ErlAttributes.Include);
        public IReadOnlyList<string> erlIncludeLibs => ListAttribute(ErlAttributes.IncludeLib);
        public string erlShortFileName => Attribute(ErlAttributes.File, erlName);
        public string erlModName => System.IO.Path.GetFileNameWithoutExtension(erlShortFileName);
        public string erlFileName => erlShortFileName;
        public string erlBaseUrl => Attribute(ErlAttributes.HttpBaseUrl, null);
    }

    public partial class WebResource
    {
        public bool erlCowboyReq => Attribute(ErlAttributes.HttpCowboyReq, false);
        public string erlName => Attribute(ErlAttributes.Name, Name.Format(Notation.LowerUnderscore));
        public string erlWebCallback => Attribute(ErlAttributes.HttpCallback, "web_handler");
        public string erlDefaultClientHeaders => Attribute(ErlAttributes.HttpDefaultClientHeaders, null);
        public string erlWebClientContext => Attribute(ErlAttributes.HttpClientContext, null);
        public string erlHttpcStream => Attribute(ErlAttributes.HttpHttpcStream, null);
        public string erlClientHttpOptions => Attribute(ErlAttributes.HttpClientHttpOptions, "[]");
        public ExceptionType erlHttpExceptionType => Attribute(ErlAttributes.HttpExceptionType, ExceptionType.Throw);
        public WebResult erlHttpResult => Attribute(ErlAttributes.HttpResult, WebResult.Content);
    }

    public partial class WebResponse
    {
        public ExceptionType erlReturnType => IsSuccess? ExceptionType.Return : Resource.erlHttpExceptionType;
    }

    public partial class WebVariable
    {
        public string erlName => Attribute(ErlAttributes.Name, Name.Format(Notation.UpperCamel));
        public string erlType => Helper.ErlType(Type, true);
        public string erlAtomName => Helper.AtomName(Attribute(ErlAttributes.Name, Name.Format(Notation.LowerUnderscore)));
        public string erlDefault => Helper.ErlValue(DefaultValue, Type);
        public bool erlCowboyParseHeader => Attribute(ErlAttributes.HttpCowboyParseHeader, false);
    }

    public partial class WebContent
    {
        public string erlRequestName => erlVarName("RequestContent");
        public string erlType => Helper.ErlType(Type, true);

        public string erlVarName(string defaultName) => Var == null ? defaultName : Var.erlName;

        public string erlAtomName(string defaultName) => Var == null ? defaultName : Var.erlAtomName;
    }
}
