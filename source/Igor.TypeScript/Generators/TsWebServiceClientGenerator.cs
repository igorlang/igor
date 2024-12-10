using Igor.Text;
using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igor.TypeScript
{
    internal class TsWebServiceClientGenerator : ITsGenerator
    {
        public void Generate(TsModel model, Module mod)
        {
            foreach (var webService in mod.WebServices)
            {
                if (webService.tsEnabled && webService.webClientEnabled)
                    GenerateWebServiceClient(model, webService);
            }
        }

        private void GenerateWebServiceClient(TsModel model, WebServiceForm webService)
        {
            var file = model.File(webService.tsName, webService.tsFileName);
            file.Import("import { Injectable } from '@angular/core';");
            file.Import("import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';");
            file.Import("import { Observable, throwError } from 'rxjs';");
            file.Import("import { map, catchError } from 'rxjs/operators';");
            file.Import("import * as Igor from './igor';");

            var c = file.Class(webService.tsName);
            c.Decorator(
@"@Injectable({
    providedIn: 'root',
})");
            c.Property($"public baseUrl = '{webService.tsBaseUrl}';");

            c.Constructor("constructor(private http: HttpClient) { }");

            foreach (var resource in webService.Resources)
            {
                c.Function(ResourceMethod(file, resource));
            }
        }

        public string ResourceMethod(TsFile file, WebResource resource)
        {
            var method = resource.Method.ToString().ToLower();
            string content = null;
            string ns = null;
            var args = new List<string>();
            if (resource.RequestContent != null)
            {
                content = resource.tsRequestBodyType.toJson("request", ns);
                args.Add($"request: {resource.tsRequestBodyType.relativeName(ns)}");
                file.ImportType(resource.tsRequestBodyType);
            }
            foreach (var resp in resource.Responses)
            {
                if (resp.Content != null)
                    file.ImportType(resp.Content.tsType);
            }
            foreach (var arg in resource.RequestVariables)
            {
                file.ImportType(arg.tsType);
            }
            args.AddRange(resource.RequestVariables.Select(v => v.tsArg(ns)));

            bool hasOptionalQueryArgs = resource.QueryVariables.Any(q => q.IsOptional);

            string PathInterpolation()
            {
                var sb = new StringBuilder();
                sb.Append("`${this.baseUrl}/");
                bool isFirst = true;
                foreach (var segment in resource.Path)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append("/");
                    if (segment.IsStatic)
                        sb.Append(segment.StaticValue);
                    else
                        sb.AppendFormat(@"${{{0}}}", segment.Var.tsType.toUri(segment.Var.tsName, ns));
                }
                if (resource.Query.Any())
                {
                    if (hasOptionalQueryArgs)
                    {
                        sb.Append("${queryString}");
                    }
                    else
                    {
                        sb.Append("?");
                        bool isFirstQ = true;
                        foreach (var q in resource.Query)
                        {
                            if (isFirstQ)
                                isFirstQ = false;
                            else
                                sb.Append("&");

                            sb.Append(q.Parameter);
                            sb.Append("=");
                            sb.Append(q.IsStatic ? q.StaticValue : $@"${{{q.Var.tsType.toUri(q.Var.tsName, ns)}}}");
                        }
                    }
                }
                sb.Append("`");
                return sb.ToString();
            }

            var r = new Renderer();

            var complexResponse = resource.Responses.Exists(resp => resp.IsSuccess && resp.Status != null);
            var responseTypes = resource.Responses.Where(resp => resp.IsSuccess && resp.Content?.Type != null).Select(resp => Helper.TargetType(resp.Content.Type).relativeName(ns)).Distinct().ToList();
            var responseBodyType = responseTypes.Any() ? responseTypes.JoinStrings(" | ") : "void";

            r += $@"public {resource.tsName}({args.JoinStrings(", ")}): Observable<{responseBodyType}> {{";
            r++;
            var headers = new Dictionary<string, string>();
            if (resource.RequestContent != null)
            {
                headers.Add("Content-Type", "'application/json'");
            }
            foreach (var header in resource.RequestHeaders)
            {
                headers.Add(header.Name, header.tsHeaderValue(ns));
            }

            var options = new Dictionary<string, string>();

            if (headers.Any())
            {
                r += $"const headers = new HttpHeaders({{{headers.JoinStrings(", ", h => $"'{h.Key}': {h.Value}")}}});";
                options.Add("headers", "headers");
            }

            if (complexResponse)
            {
                options.Add("observe", "'response'");
            }

            if (hasOptionalQueryArgs)
            {
                r += "const queryParams: Array<string> = [];";
                foreach (var q in resource.Query)
                {
                    if (q.IsStatic)
                    {
                        r += $@"queryParams.push(""{q.Parameter}={q.StaticValue}"");";
                    }
                    else
                    {
                        var toUri = q.Var.tsType.toUri(q.Var.tsName, ns);
                        if (q.Format == DataFormat.Json)
                            toUri = $"encodeURIComponent(JSON.stringify({toUri}))";
                        if (q.Var.IsOptional)
                        {
                            r += $@"if ({q.Var.tsName} != null)
    queryParams.push(`{q.Parameter}=${{{toUri}}}`);";
                        }
                        else
                        {
                            r += $@"queryParams.push(`{q.Parameter}=${{{toUri}}}`);";
                        }
                    }
                }
                r += "const queryString = queryParams.length > 0 ? `?${queryParams.join('&')}` : '';";
            }

            r.EmptyLine();
            var methodArgs = new List<string> { PathInterpolation() };
            if (content != null)
                methodArgs.Add(content);
            else if (resource.Method == HttpMethod.POST || resource.Method == HttpMethod.PUT)
                methodArgs.Add("null");
            if (options.Count > 0)
                methodArgs.Add(options.JoinStrings(", ", p => $"{p.Key}: {p.Value}").Quoted("{", "}"));
            r += "return this.http";
            r++;
            r += $".{method}({methodArgs.JoinStrings(", ")})";
            r += ".pipe(";
            r++;
            var errors = resource.Responses.Where(resp => resp.StatusCode > 300 && resp.Content != null);
            if (errors.Any())
            {
                r += "catchError(response => {";
                r++;
                r += "if (response instanceof HttpErrorResponse) {";
                r++;
                r += "switch (response.status) {";
                r++;
                foreach (var error in errors)
                {
                    r += $"case {error.StatusCode}: return throwError({error.Content.tsType.fromJson("response.error", ns)});";
                }
                r--;
                r += "}";
                r--;
                r += "}";
                r += "return throwError(response);";
                r--;
                r += "}),";
            }

            if (complexResponse)
            {
                r += "map((response) => {";
                r++;
                r += "switch (response.status) {";
                r++;
                bool isDefault = true;
                foreach (var resp in resource.Responses)
                {
                    if (resp.IsSuccess)
                    {
                        r += $"case {resp.StatusCode}:";
                        if (isDefault)
                            r += "default:";
                        isDefault = false;
                        r += $"    return {Helper.TargetType(resp.Content.Type).fromJson(TsCodeUtils.As("response.body", "Igor.Json.JsonValue"), ns)};";
                    }
                }
                r--;
                r += "}";
                r--;
                r += "})";
            }
            else if (resource.ResponseBodyType != null)
                r += $"map(response => {resource.tsResponseBodyType.fromJson(TsCodeUtils.As("response", "Igor.Json.JsonValue"), ns)})";
            else
                r += "map(response => undefined)";
            r--;
            r += ");";
            r--;
            r--;
            r += "}";

            return r.Build();
        }
    }
}
