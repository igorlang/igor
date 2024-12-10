using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Igor.Text;
using Igor.Elixir.AST;
using Igor.Elixir.Model;
using Igor.Elixir.Render;

namespace Igor.Elixir.Generators
{
    class ExHttpServerGenerator : IElixirGenerator
    {
        public void Generate(ExModel model, Module mod)
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
            string GetSegment(WebPathSegment segment)
            {
                return segment.IsStatic ? segment.StaticValue : $":{segment.Var.exName}";
            }
            return "/" + path.JoinStrings("/", GetSegment);
        }

        private string GetHandlerFileName(IReadOnlyList<WebPathSegment> path)
        {
            return path.Where(p => p.IsStatic).JoinStrings(p => p.StaticValue.Format(Notation.UpperCamel));
        }

        private void GenerateServer(ExModel model, WebServiceForm service)
        {
            var handlers = service.Resources.GroupBy(r => GetPath(r.Path));
            var exFile = model.File(service.exFileName);
            var exBehaviourFile = model.File(service.exHttpBehaviourFile);
            var exBehaviourMod = exBehaviourFile.Module(service.exHttpBehaviour);
            var httpMod = exFile.Module(ExName.Combine(service.Module.exName, service.exName));
            if (service.Annotation != null)
                httpMod.Annotation = service.Annotation;
            foreach (var handler in handlers)
            {
                var modName = handler.First().Attribute(ExAttributes.HttpHandler, GetHandlerFileName(handler.First().Path));
                var mod = httpMod.Module(modName);
                mod.Import("Plug.Conn");
                mod.Require("Logger");
                var methods = handler.Select(h => h.Method).Distinct().ToList();

                mod.Function("def init(opts), do: opts");
                mod.Function("def call(%{method: method} = conn, _opts), do: handle_method(method, conn)");

                var r = ExRenderer.Create();
                foreach (var method in methods)
                {
                    var resource = handler.First(h => h.Method == method);
                    var requestArgs = new List<string> { "conn" };
                    if (resource.RequestContent != null)
                        requestArgs.Add(resource.RequestContent.exRequestName);
                    requestArgs.AddRange(resource.RequestVariables.Select(v => v.exName));

                    r += $@"defp handle_method(""{method}"", conn) do";
                    r++;
                    if (requestArgs.Count > 1)
                    {
                        r += "try do";
                        r++;

                        if (resource.QueryVariables.Any())
                            r += $"conn = fetch_query_params(conn)";

                        if (resource.RequestContent != null)
                        {
                            var bodyReader = handler.First().Attribute(ExAttributes.HttpBodyReader, "read_body");
                            r.Line($"{{:ok, body, conn}} = {bodyReader}(conn)");
                            r.Block(
                                $@"{resource.RequestContent.exRequestName} = body
  |> Igor.Json.decode!
  |> Igor.Json.parse_value({JsonSerialization.JsonTag(resource.RequestContent.Type, resource)})");
                        }

                        if (resource.PathVariables.Any())
                        {
                            foreach (var v in resource.PathVariables)
                            {
                                var parseTag = HttpSerialization.UriTag(v.Type, v, v);
                                /*
                                if (v.GetHttpParts() && parseTag is SerializationTags.List list)
                                {
                                    if (list.ItemType is SerializationTags.Primitive primitive && primitive.Type == PrimitiveType.String)
                                        r += $@"{v.exName} = Map.get(conn.path_params, {req1}),";
                                    else
                                        r += $@"{v.exName} = [ {list.ItemType.ParseUri("PathPart")} || PathPart <- cowboy_req:path_info({req1}) ],";
                                }
                                else 
                                if (v.GetHttpParts() && parseTag is SerializationTags.Primitive primitive)
                                {
                                    if (primitive.Type == PrimitiveType.String)
                                        r += $@"{v.exName} = iolist_to_binary(lists:join(<<""/"">>, cowboy_req:path_info({req1}))),";
                                    else
                                        r += $@"{v.exName} = iolist_to_binary(lists:join(<<""/"">>, [ {primitive.ParseUri("PathPart")} || PathPart <- cowboy_req:path_info({req1}) ])),";
                                }
                                else
                                {*/
                                r += $@"{v.exName} = Igor.Http.parse_path(""{v.exName}"", conn.path_params, {parseTag})";
                                //}
                            }
                        }

                        if (resource.QueryVariables.Any())
                        {
                            void ParseQueryVariable(WebQueryParameter v)
                            {
                                var parseTag = HttpSerialization.HttpQueryTag(v.Var.Type, v.Var, v.Var, v.Format).Tag;
                                if (v.Var.DefaultValue == null)
                                    r += $@"{v.Var.exName} = Igor.Http.parse_query(""{v.Parameter}"", conn.query_params, {parseTag})";
                                else
                                    r += $@"{v.Var.exName} = Igor.Http.parse_query(""{v.Parameter}"", conn.query_params, {parseTag}, {Helper.ExValue(v.Var.DefaultValue, v.Var.Type)})";
                            }

                            r.ForEach(resource.Query.Where(q => !q.IsStatic), ParseQueryVariable);
                        }

                        if (resource.RequestHeadersVariables.Any())
                        {
                            void ParseHeaderVariable(WebHeader v)
                            {
                                var parseTag = StringSerialization.StringTag(v.Var.Type, v.Var).Tag;
                                var headerValue = $@"get_req_header(conn, {v.exLowerNameString})";
                                // if (v.Var.exCowboyParseHeader)
                                //    r += $@"{v.Var.exName} = cowboy_req:parse_header({v.exLowerNameBinary}, {req1}),";
                                //else
                                if (v.Var.DefaultValue == null)
                                    r += $@"{v.Var.exName} = Igor.Http.parse_header({v.exLowerNameString}, {headerValue}, {parseTag})";
                                else
                                    r += $@"{v.Var.exName} = Igor.Http.parse_header({v.exLowerNameString}, {headerValue}, {parseTag}, {v.Var.exDefault})";
                            }

                            r.ForEach(resource.RequestHeaders.Where(v => !v.IsStatic), ParseHeaderVariable);
                        }

                        r += $@"{{{requestArgs.JoinStrings(", ")}}}";

                        r--;
                        r += "rescue";
                        r++;

                        var format400 = resource.exFormat400;
                        var result400 = resource.Responses.FirstOrDefault(t => t.StatusCode == 400);
                        if (format400 != null && result400 == null)
                            resource.Warning($"{ExAttributes.HttpFormat400.Name} is set for resource but response type for status 400 is not defined.");

                        void Process400(string exception)
                        {
                            r += @$"e in {exception} ->";
                            r++;
                            if (format400 != null && result400 != null)
                            {
                                r += $"body = {format400}(e)";
                                r++;
                                r += $@"|> Igor.Json.pack_value({JsonSerialization.JsonTag(result400.Content.Type, resource)})";
                                r += "|> Igor.Json.encode!";
                                r--;
                                r.Block($@"conn
  |> put_resp_content_type(""{HttpUtils.GetDefaultContentType(result400.Content.Format, false)}"")
  |> send_resp(400, body)");
                            }
                            else
                            {
                                r += @"body = Igor.Json.encode!(%{""error"" => e.message})";
                                r += @"conn
    |> put_resp_content_type(""application/json"")
    |> send_resp(400, body)";
                            }
                            r--;
                        }
                        Process400("Igor.DecodeError");
                        Process400("Igor.Http.BadRequestError");
                    
                        r--;
                        r += "else";
                        r++;
                        r += $"{{{requestArgs.JoinStrings(", ")}}} ->";
                        r++;
                        r += $"handle_{method.ToString().ToLower()}({requestArgs.JoinStrings(", ")})";
                        r--;
                        r--;
                        r += "end";
                    }
                    else
                    {
                        r += $"handle_{method.ToString().ToLower()}({requestArgs.JoinStrings(", ")})";
                    }

                    r--;

                    r += "end";
                }

                r += $@"defp handle_method(_, conn) do
  conn
    |> put_resp_header(""allow"", ""{methods.JoinStrings(", ", m => m.ToString())}"")
    |> send_resp(405, """")
end";
                mod.Function(r.Build());

                foreach (var method in methods)
                {
                    var resource = handler.First(h => h.Method == method);

                    var callArgs = new List<string> { "conn" };
                    if (resource.RequestContent != null)
                        callArgs.Add(resource.RequestContent.exRequestName);
                    callArgs.AddRange(resource.RequestVariables.Select(v => v.exName));

                    r = ExRenderer.Create();
                    r += $@"defp handle_{method.ToString().ToLower()}({callArgs.JoinStrings(", ")}) do";
                    r++;
                    
                    var okResponse = resource.Responses.First();
                    var requestArgs = new List<string>();
                    var requestTypes = new List<string>();
                    var resultVars = new List<string>();
                    var resultTypes = new List<string>();
                    if (resource.RequestContent != null)
                    {
                        requestArgs.Add(resource.RequestContent.exRequestName);
                        requestTypes.Add(resource.RequestContent.exType);
                    }

                    requestArgs.AddRange(resource.RequestVariables.Select(v => v.exName));
                    requestTypes.AddRange(resource.RequestVariables.Select(v => v.exType));

                    resultVars.AddRange(okResponse.HeadersVariables.Select(h => h.exName));
                    resultTypes.AddRange(okResponse.HeadersVariables.Select(h => h.exType));
                    if (okResponse.Content != null)
                    {
                        resultVars.Add(okResponse.Content.exVarName("response_content"));
                        resultTypes.Add(okResponse.Content.exType);
                    }

                    if (resource.exHttpSessionKey != null)
                    {
                        requestArgs.Add($"get_session(conn, {Helper.AtomName(resource.exHttpSessionKey)})");
                        requestTypes.Add("session :: any()");
                    }
                    else if (resource.exHttpSession)
                    {
                        requestArgs.Add("get_session(conn)");
                        requestTypes.Add("session :: %{optional(String.t()) => any()}");
                    }

                    if (resource.exConn)
                    {
                        requestArgs.Add("conn");
                        requestTypes.Add("Plug.Conn.t()");
                        resultVars.Add("conn");
                        resultTypes.Add("Plug.Conn.t()");
                    }

                    string result = "";
                    if (resultVars.Count == 1)
                        result = $"{resultVars[0]} = ";
                    else if (resultVars.Count > 1)
                        result = $"{{{resultVars.JoinStrings(", ")}}} = ";

                    r += $@"Logger.debug(""rpc_req"", data: %{{method: ""{resource.exHttpServerCallback}.{resource.exName}"", args: [{requestArgs.Where(arg => arg != "conn").JoinStrings(", ")}]}}, domain: [:rpc])";

                    r += $"{result}{resource.exHttpServerCallback}.{resource.exName}({requestArgs.JoinStrings(", ")})";

                    var logResultVars = resultVars.Where(arg => arg != "conn").ToList();
                    var logResult = "nil";
                    if (logResultVars.Count == 1)
                        logResult = logResultVars[0];
                    else if (logResultVars.Count > 1)
                        logResult = $"{{{logResultVars.JoinStrings(", ")}}}";
                    r += $@"Logger.debug(""rpc_res"", data: %{{method: ""{resource.exHttpServerCallback}.{resource.exName}"", result: {logResult}}}, domain: [:rpc])";

                    if (okResponse.Content != null)
                    {
                        switch (okResponse.Content.Format)
                        {
                            case DataFormat.Binary:
                                var binaryTag = BinarySerialization.BinaryTag(okResponse.Content.Type, resource);
                                if (binaryTag == SerializationTags.Binary)
                                    r.Block($@"body = {okResponse.Content.exVarName("response_content")}");
                                else
                                    r.Block($@"body = {okResponse.Content.exVarName("response_content")}
  |> Igor.Binary.pack_value({binaryTag})");
                                break;
                            default:
                                r.Block($@"body = {okResponse.Content.exVarName("response_content")}
  |> Igor.Json.pack_value({JsonSerialization.JsonTag(okResponse.Content.Type, resource)})
  |> Igor.Json.encode!");
                                break;
                        }
                    }

                    r += "conn";
                    r++;
                    foreach (var header in okResponse.Headers)
                    {
                        if (header == okResponse.ContentTypeHeader)
                            r += $@"|> put_resp_content_type({header.exValue})";
                        else
                            r += $@"|> put_resp_header(""{header.Name}"", {header.exValue})";
                    }

                    if (okResponse.Content != null && okResponse.ContentTypeHeader == null)
                    {
                        r += $@"|> put_resp_content_type(""{HttpUtils.GetDefaultContentType(okResponse.Content.Format, false)}"")";
                    }

                    if (okResponse.Content != null)
                        r += $@"|> send_resp({okResponse.StatusCode}, body)";
                    else
                        r += $@"|> send_resp({okResponse.StatusCode}, """")";
                    r--;
                    
                    r--;
                    r.Line("rescue");
                    r++;

                    foreach (var throws in resource.Responses.Where(resp => resp.StatusCode >= 300))
                    {
                        if (throws.Content == null)
                            continue;
                        RecordForm exception = null;
                        if (throws.Content.Type is RecordForm recForm)
                        {
                            exception = recForm;
                        }
                        else if (throws.Content.Type is GenericType genericType && genericType.Prototype is RecordForm recPrototype)
                        {
                            exception = recPrototype;
                        }

                        if (exception != null)
                        {
                            r += $"e in {exception.Module.exName}.{exception.exName} ->";
                            r++;
                            r += $@"Logger.notice(""rpc_exc"", data: %{{method: ""{resource.exHttpServerCallback}.{resource.exName}"", exception: e}}, domain: [:rpc])";

                            var postProcess = resource.exPostprocessException;
                            if (postProcess == null)
                                r += "body = e";    
                            else
                                r += $"body = {postProcess}(e)";
                            r++;
                            r += $@"|> Igor.Json.pack_value({JsonSerialization.JsonTag(throws.Content.Type, resource)})";
                            r += "|> Igor.Json.encode!";
                            r--;
                            r.Block($@"conn
  |> put_resp_content_type(""{HttpUtils.GetDefaultContentType(throws.Content.Format, false)}"")
  |> send_resp({throws.StatusCode}, body)");

                            r--;
                        }
                        /*
                        var vars = new Dictionary<string, string>();
                        vars.Add("status_code", throws.StatusCode.ToString());
                        foreach (var header in throws.HeadersVariables)
                        {
                            vars.Add(header.exAtomName, header.exName);
                        }
                        var responseHeadersVar = "ResponseHeaders" + throws.StatusCode;
                        var responseContent = throws.Content != null ? throws.Content.exVarName("ResponseContent" + throws.StatusCode) : "";
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

                        var failFun = $"{resource.exName}_{throws.StatusCode}";
                        var failVars = new List<string>();
                        failVars.AddRange(throws.HeadersVariables.Select(h => h.exName));
                        if (throws.Content != null)
                            failVars.Add(throws.Content.exVarName("ResponseContent" + throws.StatusCode));

                        mod.Export(failFun, failVars.Count);
                        mod.Function($@"
{failFun}({failVars.JoinStrings(", ")}) ->
    throw(#{{{vars.JoinStrings(", ", p => $"{p.Key} => {p.Value}")}}}).
");
                        */
                        
                    }

                    r += "e ->";
                    r++;
                    r += $@"Logger.error(""rpc_exc"", data: %{{method: ""{resource.exHttpServerCallback}.{resource.exName}"", exception: e, stacktrace: __STACKTRACE__}}, domain: [:rpc])";

                    var format500 = resource.exFormat500;
                    var result500 = resource.Responses.FirstOrDefault(t => t.StatusCode == 500);
                    if (format500 != null && result500 == null)
                        resource.Warning($"{ExAttributes.HttpFormat500.Name} is set for resource but response type for status 500 is not defined.");

                    if (format500 != null && result500 != null)
                    {
                        r += $"body = {format500}(e)";
                        r++;
                        r += $@"|> Igor.Json.pack_value({JsonSerialization.JsonTag(result500.Content.Type, resource)})";
                        r += "|> Igor.Json.encode!";
                        r--;
                        r.Block($@"conn
  |> put_resp_content_type(""{HttpUtils.GetDefaultContentType(result500.Content.Format, false)}"")
  |> send_resp(500, body)");
                    }
                    else
                    {
                        r += $@"body = Igor.Json.encode!(%{{""error"" => inspect(e)}})";
                        r += $@"conn
  |> put_resp_content_type(""application/json"")
  |> send_resp(500, body)";
                    }
                    r--;

                    r--;
                    r.Line("end");
                    mod.Function(r.Build());

                    string resultType = "";
                    if (resultTypes.Count == 1)
                        resultType = resultTypes[0];
                    else if (resultTypes.Count > 1)
                        resultType = $"{{{resultTypes.JoinStrings(", ")}}}";
                    else
                        resultType = "any";
                    exBehaviourMod.Callback($@"@callback {resource.exName}({requestTypes.JoinStrings(", ")}) :: {resultType}").Annotation = resource.Annotation;
                }
            }
        }
    }
}
