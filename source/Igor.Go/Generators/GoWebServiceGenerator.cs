using Igor.Go.AST;
using Igor.Go.Model;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Go
{
    internal class GoWebServiceGenerator : IGoGenerator
    {
        public void Generate(GoModel model, Module mod)
        {
            var file = model.FileOf(mod);

            foreach (var webService in mod.WebServices)
            {
                if (webService.goEnabled && webService.webClientEnabled)
                {
                    file.Import("net/http");

                    var goStruct = file.Struct(webService.goName);
                    goStruct.Property("Client").Type = "*http.Client";
                    goStruct.Property("Scheme").Type = "string";
                    goStruct.Property("Host").Type = "string";
                    goStruct.Property("Headers").Type = "map[string][]string";
                    
                    foreach (var resource in webService.Resources)
                        goStruct.Method(GenerateResourceMethod(resource, file, webService));
                }
            }
        }

        private string GenerateResourceMethod(WebResource resource, GoFile file, WebServiceForm webService)
        {
            var r = new Renderer { Tab = "\t", RemoveDoubleSpaces = false };

            file.Import("fmt");
            file.Import("path");
            file.Import("net/url");

            // build list of all arguments that will the accepted by the method
            var args = new List<string>();
            if (resource.RequestContent != null)
                args.Add($"{resource.RequestContent.goName} {resource.RequestContent.goType.Name}");
            args.AddRange(resource.RequestVariables.Select(v => $"{v.goName} {Helper.TargetType(v.Type).Name}"));

            // build list of return data
            // always adds error
            var res = new List<string>();
            if (resource.ResponseBodyType != null)
                res.Add($"*{Helper.TargetType(resource.ResponseBodyType).Name}");
            res.Add("error");

            // add start of method
            r.Line($@"func ({webService.goShortVarName} *{webService.goName}) {resource.goName}({args.JoinStrings(", ")}) ({res.JoinStrings(", ")}) {{");
            r.Indent();

            // only add logic for url queries when needed
            if (resource.Query.Count > 0)
            {
                r.Line("query := url.Values{}");
                foreach(var webQueryParameter in resource.Query)
                {
                    if (webQueryParameter.Var.IsOptional && !webQueryParameter.IsStatic)
                    {
                        r.Line($"if {webQueryParameter.Var.goName} != nil {{");
                        r.Indent();
                        r.Line($@"query.Set(""{webQueryParameter.Parameter}"", {(Helper.GetStrconvFormat(webQueryParameter.Var.Type, webQueryParameter.Var.goName))})");
                        r.Outdent();
                        r.Line("}");
                    } else {
                        r.Line($@"query.Set(""{webQueryParameter.Parameter}"", {(webQueryParameter.IsStatic ? webQueryParameter.StaticValue : Helper.GetStrconvFormat(webQueryParameter.Var.Type, webQueryParameter.Var.goName))})");
                    }
                }
                r.EmptyLine();
            }

            // create url object
            var path = resource.Path.Select(p => p.IsStatic ? $@"""{p.StaticValue}""" : $@"{(Helper.GetStrconvFormat(p.Var.Type, p.Var.goName))}");
            r.Line("reqURL := url.URL{");
            r.Indent();
            r.Line($"Scheme: {webService.goShortVarName}.Scheme,");
            r.Line($"Host:   {webService.goShortVarName}.Host,");
            r.Line($"Path:   path.Join({path.JoinStrings(", ")}),");
            if (resource.Query.Count > 0)
                r.Line("RawQuery: query.Encode(),");
            r.Outdent();
            r.Line("}");

            // only deal with a request body when needed
            if (resource.RequestContent != null)
            {
                file.Import("bytes");
                file.Import("encoding/json");

                r.Line($@"b, err := json.Marshal({resource.RequestContent.goName})");
                r.Line("if err != nil {");
                r.Indent();
                r.Line("return nil, err");
                r.Outdent();
                r.Line("}");
                r.Line("body := bytes.NewBuffer(b)");
                r.EmptyLine();
            } 

            // prepare the http request
            var httpMethod = Helper.GetHttpMethod(resource.Method);
            r.Line($@"req, err := http.NewRequest({httpMethod}, reqURL.String(), {(resource.RequestContent != null ? "body" : "nil")})");
            r.Line("if err != nil {");
            r.Indent();
            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
            r.Outdent();
            r.Line("}");
            r.EmptyLine();

            // add any base headers
            r.Line($"for header, values := range {webService.goShortVarName}.Headers {{");
            r.Indent();
            r.Line("for _, value := range values {");
            r.Indent();
            r.Line("req.Header.Add(header, value)");
            r.Outdent();
            r.Line("}");
            r.Outdent();
            r.Line("}");

            // add any resource specific headers
            foreach(var header in resource.RequestHeaders)
            {
                file.Import("strconv");

                if (header.Var.IsOptional)
                {
                    r.Line($@"if {header.Var.goName} != nil {{");
                    r.Indent();
                    r.Line($@"req.Header.Add(""{header.Name}"", {(header.IsStatic ? header.StaticValue : Helper.GetStrconvFormat(header.Var.Type, header.Var.goName))})");
                    r.Outdent();
                    r.Line("}");
                } else {
                    r.Line($@"req.Header.Add(""{header.Name}"", {(header.IsStatic ? header.StaticValue : Helper.GetStrconvFormat(header.Var.Type, header.Var.goName))})");
                }
            }
            r.EmptyLine();

            // add the actual http request
            r.Line($@"resp, err := {webService.goShortVarName}.Client.Do(req)");
            r.Line("if err != nil {");
            r.Indent();
            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
            r.Outdent();
            r.Line("}");
            r.EmptyLine();

            // add handling of status codes
            r.Line("switch resp.StatusCode {");
            foreach(var response in resource.Responses)
            {
                r.Line($@"case {Helper.GetHttpStatusName(response.StatusCode)}:");
                r.Indent();
                // treat all status codes in the 200 range as success
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    // handle successful response body
                    // assumes that return data is json
                    if (response.Content != null)
                    { 
                        file.Import("io");
                        file.Import("encoding/json");

                        r.Line("defer resp.Body.Close()");
                        r.Line("body, err := io.ReadAll(resp.Body)");
                        r.Line("if err != nil {");
                        r.Indent();
                        r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
                        r.Outdent();
                        r.Line("}");
                        r.EmptyLine();

                        r.Line($@"var ret {response.Content.goType.Name}");
                        r.Line("err = json.Unmarshal(body, &ret)");
                        r.Line("if err != nil {");
                        r.Indent();
                        r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
                        r.Outdent();
                        r.Line("}");
                        r.Line("return &ret, nil");
                    } else {
                        // deal with responses without a body, nothing went wrong
                        r.Line("return nil");
                    }
                // everything else is considered an error
                } else {
                    if (response.Content != null) {
                        var targetType = Helper.TargetType(response.Content.Type);
                        GoStruct errorStruct;

                        switch (targetType)
                        {
                        case GoGenericType t:
                            errorStruct = file.Struct($@"{t.ValueType.Name}");

                            if (!errorStruct.Methods.Exists(m => m.Contains("Error")))
                            {
                                var props = errorStruct.Properties.Select(p => $"e.{p.Name}");
                                var f = Enumerable.Repeat("%v", props.Count());
                                var er = new Renderer { Tab = "\t", RemoveDoubleSpaces = false };
                                er.Line($@"func (e {t.ValueType.Name}[T]) Error() string {{");
                                er.Indent();
                                er.Line($@"return fmt.Sprintf(""create error response {f.JoinStrings(", ")}"", {props.JoinStrings(", ")})");
                                er.Outdent();
                                er.Line("}");
                                errorStruct.Method(er.Build());
                            }

                            // assume generic response is json encoded
                            file.Import("io");
                            file.Import("encoding/json");

                            r.Line("defer resp.Body.Close()");
                            r.Line("body, err := io.ReadAll(resp.Body)");
                            r.Line("if err != nil {");
                            r.Indent();
                            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
                            r.Outdent();
                            r.Line("}");
                            r.EmptyLine();

                            r.Line($@"var ret {t.Name}");
                            r.Line("err = json.Unmarshal(body, &ret)");
                            r.Line("if err != nil {");
                            r.Indent();
                            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}err");
                            r.Outdent();
                            r.Line("}");
                            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}ret");

                            break;
                        default:
                            // add a error method to the record struct to make it implement the
                            // golang error interface.
                            errorStruct = file.Struct($@"{targetType.Name}");

                            // ensure that an error method is not already added
                            if (!errorStruct.Methods.Exists(m => m.Contains("Error")))
                            {
                                var er = new Renderer { Tab = "\t", RemoveDoubleSpaces = false };
                                er.Line($@"func (e {response.Content.Type}) Error() string {{");
                                er.Indent();
                                er.Line($@"return ""error status code {response.StatusCode}""");
                                er.Outdent();
                                er.Line("}");
                                errorStruct.Method(er.Build());
                            }
                            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}{response.Content.Type}{{}}");
                            break;
                        }
                    } else {
                        r.Line($@"return fmt.Errorf(""received error status code {response.StatusCode}"")");
                    }
                }
                r.Outdent();
            }

            // add a default case to make sure that any unexpected error codes are treated as errors
            r.Line("default:");
            r.Indent();
            r.Line($@"return {(resource.ResponseBodyType == null ? "" : "nil, ")}fmt.Errorf(""unexpected status code: %d"", resp.StatusCode)");
            r.Outdent();
            r.Line("}");

            // finally done
            r.Outdent();
            r.Line("}");          

            return r.Build();
        }
    }
}
