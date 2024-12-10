using Igor.Elixir.AST;
using Igor.Elixir.Model;
using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Igor.Elixir.Render;

namespace Igor.Elixir
{
    internal class ExHttpClientGenerator : IElixirGenerator
    {
        public void Generate(ExModel model, Module mod)
        {
            foreach (var service in mod.WebServices)
            {
                if (service.exEnabled && service.webClientEnabled)
                {
                    var ex = model.File(service.Module.exFileName).Module(service.Module.exName).Module(service.exName);
                    GenWebService(ex, service);
                }
            }
        }

        private void GenWebService(ExModule ex, WebServiceForm service)
        {
            foreach (var resource in service.Resources)
            {
                DefineClientFunction(ex, resource);
            }
        }

        private void DefineClientFunction(ExModule module, WebResource resource)
        {
            var args = new List<string>();
            var argTypes = new List<string>();
            var context = resource.exWebClientContext;
            var baseUrl = resource.WebService.exBaseUrl;
            if (context != null)
            {
                args.Add(context);
                argTypes.Add("term");
            }

            if (baseUrl == null)
            {
                args.Add("base_url");
                argTypes.Add("String.t()");
            }

            if (resource.RequestContent != null)
            {
                args.Add(resource.RequestContent.exRequestName);
                argTypes.Add(resource.RequestContent.exType);
            }

            args.AddRange(resource.RequestVariables.Select(v => v.exName));
            argTypes.AddRange(resource.RequestVariables.Select(v => v.exType));
            var httpResult = resource.exHttpResult;

            var resultType = ":ok";
            var okResponse = resource.Responses.FirstOrDefault();
            if (httpResult == WebResult.Full)
            {
                if (okResponse?.Content == null)
                    resultType = "%{status_code => integer}";
                else
                    resultType = $"%{{status_code => integer, content => {okResponse.Content.exType}}}";
            }
            else if (okResponse?.Content != null)
            {
                resultType = resource.Responses.FirstOrDefault()?.Content.exType;
            }

            var r = ExRenderer.Create();
            r += $"@spec {resource.exName}({argTypes.JoinStrings(", ")}) :: {resultType}";
            r += $"def {resource.exName}({args.JoinStrings(", ")}) do";
            r++;

            var method = resource.Method.ToString().ToLower();

            var urlBuilder = new StringBuilder();
            urlBuilder.Append("#{");
            urlBuilder.Append(baseUrl ?? "base_url");
            urlBuilder.Append("}");
            foreach (var segment in resource.Path)
            {
                if (segment.IsStatic)
                    urlBuilder.Append($"/{segment.StaticValue}");
                else
                    urlBuilder.Append($@"/#{{{segment.Var.exName}}}");
            }

            if (resource.QueryVariables.Any())
            {
                urlBuilder.Append("?#{query}");
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
                            return $"{{\"{parameter}\", {v.exName}, {httpQueryTag.Tag}}}";
                        else
                            return $"{{\"{parameter}\", {v.exName}, {httpQueryTag.Tag}, {Helper.ExValue(v.DefaultValue, v.Type)}}}";
                    }
                }
                r += $"query = Igor.Http.compose_query([{resource.Query.JoinStrings(", ", Parameter)}])";
            }
            else if (resource.Query.Any())
            {
                urlBuilder.Append("?");
                urlBuilder.Append(resource.Query.Where(q => q.IsStatic).JoinStrings("&", q => $"{q.Parameter}={q.StaticValue}"));
            }
            var url = $@"""{urlBuilder}""";

            string clientHeadersVar = "[]";
            if (resource.exDefaultClientHeaders != null || resource.RequestHeaders.Any() || resource.RequestContent != null)
            {
                clientHeadersVar = "request_headers";
                var clientHeaders = new List<(string name, string value)>();
                if (resource.RequestContent != null)
                    clientHeaders.Add(("content-type", HttpUtils.GetDefaultContentType(resource.RequestContent.Format, false).Quoted()));
                foreach (var h in resource.RequestHeaders)
                    clientHeaders.Add((h.Name, h.exStringValue));
                var clientHeadersString = clientHeaders.JoinStrings(", ", h => $@"{{""{h.name.ToLowerInvariant()}"", {h.value}}}");
                var clientHeadersValue = clientHeaders.Any() ?
                    $"[{clientHeadersString}{(resource.exDefaultClientHeaders == null ? "" : $" | ")}{resource.exDefaultClientHeaders}]" :
                    resource.exDefaultClientHeaders;
                r += $"request_headers = {clientHeadersValue}";
            }

            var requestContent = "";
            if (resource.RequestContent != null)
            {
                switch (resource.RequestContent.Format)
                {
                    case DataFormat.Text:
                        r += $@"request_body = {resource.RequestContent.exRequestName}
  |> Igor.Strings.format_value({StringSerialization.StringTag(resource.RequestContent.Type, resource)})";
                        break;
                    default:
                    r += $@"request_body = {resource.RequestContent.exRequestName}
  |> Igor.Json.pack_value({JsonSerialization.JsonTag(resource.RequestContent.Type, resource)})
  |> Igor.Json.encode!";
                        break;
                }

                requestContent = ", request_body";
            }
            else if (resource.Method == HttpMethod.POST || resource.Method == HttpMethod.PATCH)
            {
                requestContent = @", """"";
            }
            else
            {
                requestContent = null;
            }

            var httpOptions = resource.exClientHttpOptions;

            r += $"url = {url}";
            r += $"case HTTPoison.{method}(url{requestContent}, {clientHeadersVar}, {httpOptions}) do";
            r++;

            foreach (var response in resource.Responses)
            {
                var isSuccess = response.Status == null || response.Status.Code < 300;
                var exceptionType = isSuccess ? ExceptionType.Return : resource.exHttpExceptionType;

                if (httpResult == WebResult.Content && exceptionType != ExceptionType.Return && response.Content == null)
                    continue;

                var exceptionPrefix = exceptionType == ExceptionType.Return ? "" : exceptionType.ToString().ToLower() + "(";
                var exceptionPostfix = exceptionType == ExceptionType.Return ? "" : ")";

                var headersVar = response.HeadersVariables.Any() && httpResult == WebResult.Full ? "headers" : "";
                var bodyVar = response.Content == null ? "" : ", body: body";
                // var reasonPhraseVar = httpResult == WebResult.Full ? "ReasonPhrase" : "_ReasonPhrase";
                if (response.Status == null)
                    r += $@"{{:ok, %HTTPoison.Response{{status_code: status_code{headersVar}{bodyVar}}}}} when status_code < 300 ->";
                else
                    r += $@"{{:ok, %HTTPoison.Response{{status_code: {response.Status.Code}{headersVar}{bodyVar}}}}} ->";
                r++;

                if (httpResult == WebResult.Full)
                {
                    var results = new List<(string, string)>();
                    if (response.Status == null)
                        results.Add(("status_code", "status_code"));
                    else
                        results.Add(("status_code", response.Status.Code.ToString()));

                    foreach (var responseHeader in response.Headers.Where(v => !v.IsStatic))
                    {
                        results.Add((responseHeader.Var.exAtomName, $"iolist_to_binary(proplists:get_value({responseHeader.exLowerNameString}, Headers))"));
                    }

                    switch (response.Content.Format)
                    {
                        case DataFormat.Text:
                            r += $@"response_content = body
  |> Igor.Strings.parse_value({StringSerialization.StringTag(response.Content.Type, resource)})";
                            break;
                        default:
                            r += $@"response_content = body
  |> Igor.Json.decode!
  |> Igor.Json.parse_value({JsonSerialization.JsonTag(response.Content.Type, resource)})";
                            break;
                    }
                    results.Add(("content", "response_content"));
                    CodeUtils.Group(r, $"{exceptionPrefix}#{{", results.Select(keyval => $"{keyval.Item1} => {keyval.Item2}"), $"}}{exceptionPostfix};\n", 1);
                }
                else if (response.Content != null)
                {
                    switch (response.Content.Format)
                    {
                        case DataFormat.Text:
                            r += $@"body
  |> Igor.Strings.parse_value({StringSerialization.StringTag(response.Content.Type, resource)})";
                            break;
                        default:
                            r += $@"body
  |> Igor.Json.decode!
  |> Igor.Json.parse_value({JsonSerialization.JsonTag(response.Content.Type, resource)})";
                            break;
                    }
                }
                else 
                {
                    r += ":ok";
                }

                r--;
            }
            r += @"{:ok, %HTTPoison.Response{status_code: status_code, body: response_body, headers: response_headers}} ->
  raise %Igor.Http.HttpError{status_code: status_code, body: response_body, headers: response_headers}";
            r += @"{:error, %HTTPoison.Error{reason: _} = e} ->
  raise e";
            r--;
            r += "end";
            r--;
            r += "end";
            module.Function(r.Build()).Annotation = resource.Annotation;
        }
    }
}
