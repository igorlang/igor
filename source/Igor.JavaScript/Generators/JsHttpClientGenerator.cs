using Igor.Text;
using Igor.JavaScript.AST;
using Igor.JavaScript.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Igor.JavaScript.Render;

namespace Igor.JavaScript
{
    internal class JsHttpClientGenerator : IJsGenerator
    {
        public void Generate(JsModel model, Module mod)
        {
            foreach (var webService in mod.WebServices)
            {
                if (webService.jsEnabled && webService.webClientEnabled)
                    GenerateWebServiceClient(model, webService);
            }
        }

        private void GenerateWebServiceClient(JsModel model, WebServiceForm webService)
        {
            var file = model.File(webService.jsFileName);

            file.Import("./request", defaultExport: "Req");

            foreach (var resource in webService.Resources)
            {
                file.Declaration(ResourceMethod(file, resource));
            }
        }

        public string ResourceMethod(JsFile file, WebResource resource)
        {
            var method = resource.Method.ToString().ToLower();
            var args = new List<string>();
            /*
            foreach (var resp in resource.Responses)
            {
                if (resp.Content != null)
                    file.ImportType(resp.Content.tsType);
            }
            */
            args.AddRange(resource.PathVariables.Select(v => v.jsName));
            if (resource.RequestContent != null)
            {
                args.Add(resource.RequestContent.jsName);
            }
            args.AddRange(resource.QueryVariables.Select(v => v.jsName));
            args.AddRange(resource.RequestHeadersVariables.Select(v => v.jsName));

            bool hasOptionalQueryArgs = resource.QueryVariables.Any(q => q.IsOptional);

            string PathInterpolation()
            {
                var sb = new StringBuilder();
                var interpolation = resource.QueryVariables.Any() || resource.PathVariables.Any();
                sb.Append(interpolation ? "`/" : "'/");
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
                        sb.AppendFormat(@"${{{0}}}", segment.Var.jsName);
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
                            sb.Append(q.IsStatic ? q.StaticValue : $@"${{{q.Var.jsName}}}");
                        }
                    }
                }
                sb.Append(interpolation ? "`" : "'");
                return sb.ToString();
            }

            var r = JsRenderer.Create();

            r += $"export const {resource.jsName} = ({args.JoinStrings(", ")}) => {{";
            r++;
            r += $"return Req.{resource.Method}({{";
            r++;
            r += $"url: {PathInterpolation()},";
            if (resource.Attribute(JsAttributes.WithToken, false))
                r += "withToken: true,";
            if (resource.RequestContent != null)
                r += $"data: {resource.RequestContent.jsName},";
            r--;
            r += "});";
            r--;
            r += "};";

            return r.Build();
        }
    }
}
