using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Erlang.Strings;
using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Http
{
    internal class ErlCowboyGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            foreach (var service in mod.WebServices)
            {
                if (service.webServerEnabled)
                {
                    GenerateServer(model, service);
                }
            }
        }

        private string GetPath(IReadOnlyList<WebPathSegment> path)
        {
            string GetSegment(WebPathSegment segment) => segment.IsStatic ? segment.StaticValue : $":{segment.Var.erlName}";
            return "/" + path.JoinStrings("/", GetSegment);
        }

        private string GetHandlerFileName(WebServiceForm service, IReadOnlyList<WebPathSegment> path)
        {
            var prefix = service.Attribute(ErlAttributes.HttpHandlerPrefix, service.erlName);
            return prefix + path.Where(p => p.IsStatic).JoinStrings(s => "_" + s.StaticValue);
        }

        private void GenerateServer(ErlModel model, WebServiceForm service)
        {
            var handlers = service.Resources.GroupBy(r => GetPath(r.Path));
            foreach (var handler in handlers)
            {
                var file = handler.First().Attribute(ErlAttributes.HttpHandler, GetHandlerFileName(service, handler.First().Path));
                var mod = model.Module(file);
                mod.IncludeLib("igor/include/igor_http.hrl");
                mod.Behaviour("cowboy_handler");
                mod.Export("init", 2);
                mod.Function(
@"init(Req0, Opts) ->
    Method = cowboy_req:method(Req0),
    Req = handle_method(Method, Req0),
    {ok, Req, Opts}.");

                string HandleMethod(HttpMethod method)
                {
                    bool needBody = handler.First(h => h.Method == method).RequestContent != null;
                    if (needBody)
                    {
                        return
$@"handle_method(<<""{method}"">>, Req) ->
    case cowboy_req:has_body(Req) of
        true -> handle_{method.ToString().ToLower()}(Req);
        false -> cowboy_req:reply(400, Req)
    end;";
                    }
                    else
                    {
                        return
$@"handle_method(<<""{method}"">>, Req) ->
    handle_{method.ToString().ToLower()}(Req);";
                    }
                }
                var methods = handler.Select(h => h.Method).Distinct();
                mod.Function(
$@"{methods.JoinLines(HandleMethod)}
handle_method(_, Req) ->
    ResponseHeaders = #{{<<""Allow"">> => <<""{methods.JoinStrings(", ", m => m.ToString())}"">>}},
    cowboy_req:reply(405, ResponseHeaders, Req).");

                foreach (var method in methods)
                {
                    var resource = handler.First(h => h.Method == method);
                    var r = new Renderer();
                    r += $@"handle_{method.ToString().ToLower()}(Req) ->";
                    r++;
                    r += "try";
                    r++;

                    var req1 = "Req";
                    if (resource.RequestContent != null)
                    {
                        var bodyReader = handler.First().Attribute(ErlAttributes.HttpBodyReader, "cowboy_req:read_body");
                        req1 = "Req1";
                        r.Line($"{{ok, RequestBody, {req1}}} = {bodyReader}(Req),");
                        r.Line($"{resource.RequestContent.erlRequestName} = {ErlHttpUtils.DecodeContent(resource.RequestContent, resource, "RequestBody", mod.Name)},");
                    }

                    if (resource.PathVariables.Any())
                    {
                        foreach (var v in resource.PathVariables)
                        {
                            var parseTag = HttpSerialization.UriTag(v.Type, v, v);
                            if (v.GetHttpParts() && parseTag is SerializationTags.List list)
                            {
                                if (list.ItemType is SerializationTags.Primitive primitive && primitive.Type == PrimitiveType.String)
                                    r += $@"{v.erlName} = cowboy_req:path_info({req1}),";
                                else
                                    r += $@"{v.erlName} = [ {list.ItemType.ParseUri("PathPart")} || PathPart <- cowboy_req:path_info({req1}) ],";
                            }
                            else if (v.GetHttpParts() && parseTag is SerializationTags.Primitive primitive)
                            {
                                if (primitive.Type == PrimitiveType.String)
                                    r += $@"{v.erlName} = iolist_to_binary(lists:join(<<""/"">>, cowboy_req:path_info({req1}))),";
                                else
                                    r += $@"{v.erlName} = iolist_to_binary(lists:join(<<""/"">>, [ {primitive.ParseUri("PathPart")} || PathPart <- cowboy_req:path_info({req1}) ])),";
                            }
                            else
                            {
                                r += $@"{v.erlName} = {parseTag.ParseUri($"cowboy_req:binding({v.erlAtomName}, {req1})")},";
                            }
                        }
                    }

                    if (resource.QueryVariables.Any())
                    {
                        void ParseQueryVariable(WebQueryParameter v)
                        {
                            var parseTag = HttpSerialization.HttpQueryTag(v.Var.Type, v.Var, v.Var, v.Format).ParseTag;
                            if (v.Var.DefaultValue == null)
                                r += $@"{v.Var.erlName} = igor_http:parse_query(<<""{v.Parameter}"">>, Qs, {parseTag}),";
                            else
                                r += $@"{v.Var.erlName} = igor_http:parse_query(<<""{v.Parameter}"">>, Qs, {parseTag}, {Helper.ErlValue(v.Var.DefaultValue, v.Var.Type)}),";
                        }

                        r += $"Qs = cowboy_req:parse_qs({req1}),";
                        r.ForEach(resource.Query.Where(q => !q.IsStatic), ParseQueryVariable);
                    }

                    if (resource.RequestHeadersVariables.Any())
                    {
                        void ParseHeaderVariable(WebHeader v)
                        {
                            var parseTag = StringSerialization.StringTag(v.Var.Type, v.Var).ParseTag;
                            var headerValue = $@"cowboy_req:header({v.erlLowerNameBinary}, {req1})";
                            if (v.Var.erlCowboyParseHeader)
                                r += $@"{v.Var.erlName} = cowboy_req:parse_header({v.erlLowerNameBinary}, {req1}),";
                            else if (v.Var.DefaultValue == null)
                                r += $@"{v.Var.erlName} = igor_http:parse_header({v.erlLowerNameBinary}, {headerValue}, {parseTag}),";
                            else
                                r += $@"{v.Var.erlName} = igor_http:parse_header({v.erlLowerNameBinary}, {headerValue}, {parseTag}, {v.Var.erlDefault}),";
                        }

                        r.ForEach(resource.RequestHeaders.Where(h => !h.IsStatic), ParseHeaderVariable);
                    }

                    var okResponse = resource.Responses.First();
                    var requestArgs = new List<string>();
                    var resultVars = new List<string>();
                    if (resource.RequestContent != null)
                        requestArgs.Add(resource.RequestContent.erlRequestName);
                    requestArgs.AddRange(resource.RequestVariables.Select(v => v.erlName));

                    resultVars.AddRange(okResponse.HeadersVariables.Select(h => h.erlName));
                    if (okResponse.Content != null)
                        resultVars.Add(okResponse.Content.erlVarName("ResponseContent"));

                    var req2 = req1;
                    if (resource.erlCowboyReq)
                    {
                        req2 = req1 == "Req" ? "Req1" : "Req2";
                        requestArgs.Add(req1);
                        resultVars.Add(req2);
                    }

                    string result = "";
                    if (resultVars.Count == 1)
                        result = $"{resultVars[0]} = ";
                    else if (resultVars.Count > 1)
                        result = $"{{{resultVars.JoinStrings(", ")}}} = ";

                    r.Line($"{result}{resource.erlWebCallback}:{resource.erlName}({requestArgs.JoinStrings(", ")}),");

                    void FormatBody(string bodyVarName, string sourceVarName, WebContent content)
                    {
                        r.Line($"{bodyVarName} = {ErlHttpUtils.EncodeContent(content, resource, sourceVarName, mod.Name)},");
                    }

                    void FormatHeaders(string varName, IReadOnlyList<WebHeader> headers, WebContent content)
                    {
                        if (headers.Any())
                        {
                            r += $"{varName} = #{{";
                            r++;
                            r += headers.JoinStrings(",\n", h => $@"<<""{h.Name}"">> => {h.erlValue}");
                            r--;
                            r += "},";
                        }
                        else if (content != null)
                        {
                            r.Line($@"{varName} = #{{<<""Content-Type"">> => <<""{HttpUtils.GetDefaultContentType(content.Format, true)}"">>}},");
                        }
                        else
                        {
                            r += $@"{varName} = #{{}},";
                        }
                    }

                    if (okResponse.Content != null)
                        FormatBody("Body", okResponse.Content.erlVarName("ResponseContent"), okResponse.Content);
                    FormatHeaders("ResponseHeaders", okResponse.Headers, okResponse.Content);
                    var maybeBody = okResponse.Content != null ? ", Body" : "";
                    r.Line($"cowboy_req:reply({okResponse.StatusCode}, ResponseHeaders{maybeBody}, {req2})");
                    r--;
                    r.Line("catch");
                    r++;
                    foreach (var throws in resource.Responses.Where(resp => resp.StatusCode >= 300))
                    {
                        var vars = new Dictionary<string, string>();
                        vars.Add("status_code", throws.StatusCode.ToString());
                        foreach (var header in throws.HeadersVariables)
                        {
                            vars.Add(header.erlAtomName, header.erlName);
                        }
                        var responseHeadersVar = "ResponseHeaders" + throws.StatusCode;
                        var responseContent = throws.Content != null ? throws.Content.erlVarName("ResponseContent" + throws.StatusCode) : "";
                        if (throws.Content != null)
                            vars.Add("response", responseContent);
                        r += $@"#{{{vars.JoinStrings(", ", p => $"{p.Key} := {p.Value}")}}} ->";
                        r++;
                        if (throws.Content != null)
                            FormatBody(responseContent + "Body", responseContent, throws.Content);
                        FormatHeaders(responseHeadersVar, throws.Headers, throws.Content);
                        var maybeThrowBody = throws.Content != null ? $", {responseContent + "Body"}" : "";
                        r += $@"cowboy_req:reply({throws.StatusCode}, {responseHeadersVar}{maybeThrowBody}, Req);";
                        r--;

                        var failFun = $"{resource.erlName}_{throws.StatusCode}";
                        var failVars = new List<string>();
                        failVars.AddRange(throws.HeadersVariables.Select(h => h.erlName));
                        if (throws.Content != null)
                            failVars.Add(throws.Content.erlVarName("ResponseContent" + throws.StatusCode));

                        mod.Export(failFun, failVars.Count);
                        mod.Function($@"
{failFun}({failVars.JoinStrings(", ")}) ->
    throw(#{{{vars.JoinStrings(", ", p => $"{p.Key} => {p.Value}")}}}).
");
                    }
                    r +=
@"#bad_request{} ->
    cowboy_req:reply(400, Req)";
                    r--;
                    r.Line("end.");
                    mod.Function(r.Build());
                }
            }
        }
    }
}
