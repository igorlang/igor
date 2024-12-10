using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Http
{
    internal class ErlWebClientGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            foreach (var service in mod.WebServices)
            {
                if (service.erlEnabled && service.webClientEnabled)
                {
                    var erl = model.Module(service.erlFileName);
                    GenWebService(erl, service);
                }
            }
        }

        private void GenWebService(ErlModule erl, WebServiceForm service)
        {
            foreach (var include in service.erlIncludes)
                erl.Include(include);
            foreach (var includeLib in service.erlIncludeLibs)
                erl.IncludeLib(includeLib);
            foreach (var resource in service.Resources)
            {
                DefineClientFunction(erl, resource);
            }
        }

        private void DefineClientFunction(ErlModule module, WebResource resource)
        {
            var args = new List<string>();
            var specArgs = new List<string>();
            var context = resource.erlWebClientContext;
            var baseUrl = resource.WebService.erlBaseUrl;
            if (context != null)
            {
                args.Add(context);
                specArgs.Add("term()");
            }
            if (baseUrl == null)
            {
                args.Add("BaseUrl");
                specArgs.Add("iodata()");
            }
            if (resource.RequestContent != null)
            {
                args.Add(resource.RequestContent.erlRequestName);
                specArgs.Add(resource.RequestContent.erlType);
            }
            args.AddRange(resource.RequestVariables.Select(v => v.erlName));
            specArgs.AddRange(resource.RequestVariables.Select(v => v.erlType));
            var httpResult = resource.erlHttpResult;
            module.Export(resource.erlName, args.Count);

            var responseContentType =
                resource.Responses.Where(r => r.erlReturnType == ExceptionType.Return).Where(r => r.Content != null)
                    .Select(r => r.Content.erlType).Distinct().JoinStrings(" | ");

            string fullResultType()
            {
                var returnArgs = new List<(string, string)>();
                returnArgs.Add(("status_code", "integer()"));
                returnArgs.Add(("reason_phrase", "iodata()"));
                if (!string.IsNullOrEmpty(responseContentType))
                    returnArgs.Add(("content", responseContentType));
                foreach (var resp in resource.Responses)
                {
                    if (resp.erlReturnType == ExceptionType.Return)
                    {
                        foreach (var headerVar in resp.HeadersVariables)
                        {
                            returnArgs.Add((headerVar.Name, headerVar.erlType));
                        }
                    }
                }
                return "#{" + returnArgs.JoinStrings(", ", arg => $"{arg.Item1} := {arg.Item2}") + "}";
            }

            var resultType = httpResult == WebResult.Content ? string.IsNullOrEmpty(responseContentType) ? "'ok'" : responseContentType : fullResultType();

            var r = new Renderer();
            r += $"-spec {resource.erlName}({specArgs.JoinStrings(", ")}) -> {resultType}.";
            r.EmptyLine();
            r += $"{resource.erlName}({args.JoinStrings(", ")}) ->";
            r++;
            if (resource.erlHttpClientLog)
            {
                module.IncludeLib("kernel/include/logger.hrl");
                r += $"?LOG_INFO(#{{what => http_request, service => {module.Name}, resource => {resource.erlName}}}),";
            }

            var method = resource.Method.ToString().ToLower();
            string PathSegment(WebPathSegment segment) => segment.IsStatic ? $"/{segment.StaticValue}" : "/~s";
            
            string formatVars = new string[] { baseUrl ?? "BaseUrl" }.Concat(resource.PathVariables.Select(v => v.erlName)).JoinStrings(", ");
            var urlFormat = "~s" + resource.Path.JoinStrings(PathSegment);

            if (resource.Query.Any())
            {
                urlFormat += "?~s";
                formatVars += ", Query";
                string Parameter(WebQueryParameter p)
                {
                    var parameter = System.Net.WebUtility.UrlEncode(System.Net.WebUtility.UrlDecode(p.Parameter));
                    if (p.IsStatic)
                        return $"{{\"{parameter}\", \"{p.StaticValue}\"}}";
                    else
                    {
                        var v = p.Var;
                        var httpQueryTag = HttpSerialization.HttpQueryTag(v.Type, v, v, p.Format);
                        if (v.DefaultValue == null)
                            return $"{{\"{parameter}\", {v.erlName}, {httpQueryTag.PackTag}}}";
                        else
                            return $"{{\"{parameter}\", {v.erlName}, {httpQueryTag.PackTag}, {Helper.ErlValue(v.DefaultValue, v.Type)}}}";
                    }
                }
                r += $"Query = igor_http:compose_query([{resource.Query.JoinStrings(", ", Parameter)}]),";
            }
            var url = $@"lists:flatten(io_lib:format(""{urlFormat}"", [{formatVars}]))";

            var requestHeaders = "[" + resource.RequestHeaders.JoinStrings(", ", h => $@"{{""{h.Name}"", {h.erlStringValue}}}") + "]";
            var plusRequestHeaders = resource.RequestHeaders.Any() ? $" ++ {requestHeaders}" : "";
            var clientHeaders = resource.erlDefaultClientHeaders != null ? $"{resource.erlDefaultClientHeaders}{plusRequestHeaders}" : requestHeaders;
            string request;
            if (resource.RequestContent != null)
            {
                var encodeRequest = ErlHttpUtils.EncodeContent(resource.RequestContent, resource, resource.RequestContent.erlRequestName, module.Name);
                var contentType = HttpUtils.GetDefaultContentType(resource.RequestContent.Format, false);
                request = $@"{{Url, {clientHeaders}, ""{contentType}"", {encodeRequest}}}";
            }
            else if (resource.Method == HttpMethod.POST)
            {
                request = $@"{{Url, {clientHeaders}, ""text/plain"", <<>>}}";
            }
            else
            {
                request = $@"{{Url, {clientHeaders}}}";
            }

            var httpOptions = resource.erlClientHttpOptions;
            var httpcOptions = new List<string> { "{body_format, binary}" };
            var stream = resource.erlHttpcStream;
            if (stream != null)
            {
                httpcOptions.Add($"{{stream, {stream}}}");
                httpcOptions.Add("{sync, false}");
            }

            r += $"Url = {url},";
            r += $"Request = {request},";
            r += $"case httpc:request({method}, Request, {httpOptions}, [{httpcOptions.JoinStrings(", ")}]) of";
            r++;

            foreach (var response in resource.Responses)
            {
                var returnType = response.erlReturnType;

                if (httpResult == WebResult.Content && returnType != ExceptionType.Return && response.Content == null)
                    continue;

                var exceptionPrefix = returnType == ExceptionType.Return ? "" : returnType.ToString().ToLower() + "(";
                var exceptionPostfix = returnType == ExceptionType.Return ? "" : ")";

                var headersVar = response.HeadersVariables.Any() && httpResult == WebResult.Full ? "Headers" : "_Headers";
                var bodyVar = response.Content == null ? "_Body" : "Body";
                var reasonPhraseVar = httpResult == WebResult.Full ? "ReasonPhrase" : "_ReasonPhrase";
                if (response.Status == null)
                {
                    r += $@"{{ok, {{{{_HttpVersion, StatusCode, {reasonPhraseVar}}}, {headersVar}, {bodyVar}}}}} when StatusCode < 300 ->";
                    r++;
                    if (resource.erlHttpClientLog)
                        r += $"?LOG_INFO(#{{what => http_request_completed, service => {module.Name}, resource => {resource.erlName}, status_code => StatusCode}}),";
                }
                else
                {
                    r += $@"{{ok, {{{{_HttpVersion, {response.Status.Code}, {reasonPhraseVar}}}, {headersVar}, {bodyVar}}}}} ->";
                    r++;
                    if (resource.erlHttpClientLog)
                        r += $"?LOG_INFO(#{{what => http_request_completed, service => {module.Name}, resource => {resource.erlName}, status_code => {response.Status.Code}}}),";
                }

                if (httpResult == WebResult.Full)
                {
                    var results = new List<(string, string)>();

                    if (response.Status == null)
                        results.Add(("status_code", "StatusCode"));
                    else
                        results.Add(("status_code", response.Status.Code.ToString()));
                    results.Add(("reason_phrase", reasonPhraseVar));

                    foreach (var responseHeader in response.Headers.Where(h => !h.IsStatic))
                    {
                        results.Add((responseHeader.Var.erlAtomName, $"iolist_to_binary(proplists:get_value({responseHeader.erlLowerNameString}, Headers))"));
                    }

                    if (response.Content != null)
                        results.Add((response.Content.erlAtomName("content"), ErlHttpUtils.DecodeContent(response.Content, resource, "Body", module.Name)));

                    CodeUtils.Group(r, $"{exceptionPrefix}#{{", results.Select(keyval => $"{keyval.Item1} => {keyval.Item2}"), $"}}{exceptionPostfix};\n", 1);
                }
                else
                {
                    if (response.Content != null)
                    {
                        r += $"{exceptionPrefix}{ErlHttpUtils.DecodeContent(response.Content, resource, "Body", module.Name)}{exceptionPostfix};";
                    }
                    else
                    {
                        r += "ok;";
                    }
                }

                r--;
            }
            if (stream == null)
            {
                r += @"{ok, {{_HttpVersion, StatusCode, _ReasonPhrase}, _Headers, Body}} ->";
                r++;
                if (resource.erlHttpClientLog)
                    r += $"?LOG_ERROR(#{{what => http_request_failed, service => {module.Name}, resource => {resource.erlName}, status_code => StatusCode}}),";
                r += "error({http_error, StatusCode, Body});";
                r--;
            }
            else
            {
                r += @"{ok, RequestId} ->
    RequestId;";
            }

            r += @"{error, Reason} ->";
            r++;
            if (resource.erlHttpClientLog)
                r += $"?LOG_ERROR(#{{what => http_request_failed, service => {module.Name}, resource => {resource.erlName}, error => Reason}}),";
            r += "error(Reason)";
            r--;
            r--;
            r += "end.";
            module.Function(r.Build());
        }
    }
}
