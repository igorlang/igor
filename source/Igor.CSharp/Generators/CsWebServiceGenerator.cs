using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsWebServiceGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var webService in mod.WebServices)
            {
                if (webService.csEnabled && webService.webClientEnabled)
                    GenerateWebServiceClient(model, webService);
            }
        }

        private void GenerateWebServiceClient(CsModel model, WebServiceForm webService)
        {
            var file = model.File(webService.Module.csFileName);
            file.Use("System.IO");
            file.Use("System.Net.Http");
            file.Use("System.Threading");
            CsVersion.UsingTasks(file);
            // file.Use("Igor.Web");
            var c = file.Namespace(webService.csNamespace).DefineClass(webService.csName);
            c.Interface("System.IDisposable");
            c.Constructor($@"
public {webService.csName}(HttpClient httpClient)
{{
    HttpClient = httpClient;
}}
");
            c.Property("HttpClient", "protected HttpClient HttpClient { get; }");

            c.Method(@"
public void Dispose()
{
      Dispose(true);
      System.GC.SuppressFinalize(this);
}
");

            c.Method(@"
protected virtual void Dispose(bool disposing)
{
    HttpClient.Dispose();
}
");

            foreach (var resource in webService.Resources)
            {
                c.Method(ResourceMethod(resource, file));
            }
        }

        public string ResourceMethod(WebResource resource, CsFile file)
        {
            var method = Helper.GetHttpMethod(resource.Method);
            var ns = resource.WebService.csNamespace;
            var requestBodyType = resource.RequestBodyType == null ? "void" : resource.csRequestBodyType.relativeName(ns);
            var responseBodyType = resource.ResponseBodyType == null ? "void" : resource.csResponseBodyType.relativeName(ns);
            var args = new List<string>();
            if (resource.RequestContent != null)
                args.Add($"{resource.RequestContent.csType.relativeName(ns)} {resource.RequestContent.csName}");
            args.AddRange(resource.RequestVariables.Select(v => v.csArg(ns)));
            args.Add($"CancellationToken cancellationToken = {CsVersion.Default("CancellationToken")}");

            var mb = new Renderer();
            mb.Block(
$@"public async {CsVersion.TaskClass}<{responseBodyType}> {resource.csName}({args.JoinStrings(", ")})
{{");
            mb.Indent();
            if (resource.RequestContent != null)
            {
                var n = resource.RequestContent.csName;
                if (resource.RequestContent.csType.csNotNullRequired)
                {
                    mb.Block($@"if ({n} == null)
    throw new System.ArgumentNullException({CsVersion.NameOf(n)});");
                }
            }
            mb.Blocks(resource.QueryVariables.Concat(resource.RequestHeadersVariables).Where(v => v.csType.csNotNullRequired).Select(v => v.csRequireNotNull));
            if (resource.Query.Any())
            {
                mb.Line($"var queryBuilder = new WebQueryBuilder({resource.csPath});");
                mb.Blocks(resource.Query.Select(p => p.csAppendQuery(ns)));
                mb.Line($@"using (var httpRequest = new HttpRequestMessage({method}, queryBuilder.ToString()))");
            }
            else
            {
                mb.Line($@"using (var httpRequest = new HttpRequestMessage({method}, {resource.csPath}))");
            }
            if (resource.RequestHeaders.Any() || resource.RequestContent != null)
            {
                mb.Line("{");
                mb.Indent();
                var contentType = resource.RequestHeaders.LastOrDefault(h => h.Name == "Content-Type");
                if (resource.RequestContent != null)
                {
                    if (contentType == null)
                    {
                        string format = "application/json";
                        if (resource.RequestContent.Format == DataFormat.Xml)
                            format = "text/plain";
                        mb.Line($@"httpRequest.Content = new StringContent({resource.RequestContent.csRequestContentString(ns)}, System.Text.Encoding.UTF8, ""{format}"");");
                    }
                    else
                    {
                        mb.Line($"httpRequest.Content = new StringContent({resource.RequestContent.csRequestContentString(ns)});");
                        mb.Line($"httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue({contentType.csHeaderValue});");
                    }
                }
                mb.Blocks(resource.RequestHeaders.Where(h => h != contentType).Select(h => $@"httpRequest.Headers.Add(""{h.Name}"", {h.csHeaderValue});"));
            }

            mb.Block(
@"using (var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false))
{");
            void ParseResponse(WebResponse response)
            {
                var returnOrThrow = response.StatusCode < 300 ? "return" : "throw";
                switch (response.Content.Format)
                {
                    case DataFormat.Text:
                        mb.Line($"{returnOrThrow} await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);");
                        break;

                    case DataFormat.Xml:
                        file.Use("System.Xml.Serialization");
                        mb.Block(
$@"using (var responseStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
{{
    var xmlSerializer = new XmlSerializer(typeof({response.Content.csType.relativeName(ns)}));
    {returnOrThrow} ({response.Content.csType.relativeName(ns)})xmlSerializer.Deserialize(responseStream);
}}");
                        break;

                    case DataFormat.Json:
                    default:
                        mb.Block(
$@"using (var responseStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
using (var reader = new StreamReader(responseStream))
    {returnOrThrow} {response.Content.csType.jsonSerializer(ns)}.Deserialize(Json.JsonParser.Parse(reader));");
                        break;
                }
            }

            mb.Indent();
            if (resource.Responses.Count == 1 && resource.Responses.First().StatusCode == 200)
            {
                mb.Line("httpResponse.EnsureSuccessStatusCode();");
                ParseResponse(resource.Responses[0]);
            }
            else
            {
                file.UseAlias("HttpStatusCode", "System.Net.HttpStatusCode");

                mb.Block(@"switch (httpResponse.StatusCode)
{");
                mb.Indent();
                foreach (var response in resource.Responses)
                {
                    mb.Line($"case {Helper.GetHttpStatusCode(response.StatusCode)}:");
                    mb.Indent();
                    mb.Line("{");
                    mb.Indent();
                    ParseResponse(response);
                    mb.Outdent();
                    mb.Line("}");
                    mb.Outdent();
                }
                mb.Line("default:");
                mb.Indent();
                mb.Line(@"throw new HttpRequestException(string.Format(""Unexpected status code: {0} {1}"", httpResponse.StatusCode, httpResponse.ReasonPhrase));");
                mb.Outdent();
                mb.Outdent();
                mb.Line("}");
            }

            mb.Outdent();
            mb.Line("}");

            if (resource.RequestHeaders.Any() || resource.RequestContent != null)
            {
                mb.Outdent();
                mb.Line("}");
            }
            mb.Outdent();
            mb.Line("}");
            return mb.Build();
        }
    }
}
