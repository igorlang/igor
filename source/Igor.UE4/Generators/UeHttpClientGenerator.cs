using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4
{
    public class UeHttpClientGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            foreach (var webservice in mod.WebServices)
            {
                if (webservice.ueEnabled && webservice.webClientEnabled)
                    GenerateHttpClient(webservice, model);
            }
        }

        private void GenerateHttpClient(WebServiceForm webservice, UeModel model)
        {
            var hFile = model.HFile(webservice.Module.ueHFile);
            hFile.Include("IgorHttpClient.h");
            var cppFile = model.CppFile(webservice.Module.ueCppFile);
            var hns = hFile.Namespace(webservice.ueNamespace);
            var cppns = cppFile.Namespace(webservice.ueNamespace);
            var serviceClass = hns.Class(webservice.ueName);
            serviceClass.Comment = webservice.Annotation;
            serviceClass.ApiMacro = webservice.ueApiMacro;
            serviceClass.BaseType("FIgorHttpClient");
            var baseUrl = webservice.ueBaseUrl;
            var baseUrlString = baseUrl == null ? "" : $@"TEXT(""{baseUrl}"")";
            serviceClass.Function($"{webservice.ueName}() : FIgorHttpClient({baseUrlString}) {{ }}", AccessModifier.Public);
            serviceClass.Function($"{webservice.ueName}(const FString& InBaseUrl) : FIgorHttpClient(InBaseUrl) {{ }}", AccessModifier.Public);

            foreach (var resource in webservice.Resources)
            {
                GenerateResource(resource, serviceClass, cppns, cppFile);
            }
        }

        private void GenerateResource(WebResource resource, UeStruct hclass, UeNamespace cppns, UeCppFile cppFile)
        {
            var ns = resource.WebService.ueNamespace;
            var requestBodyType = resource.RequestBodyType == null ? "void" : resource.ueRequestBodyType.RelativeName(ns);
            var requestTypeName = "F" + resource.ueName + "Request";
            var resourceType = hclass.Class(requestTypeName, AccessModifier.Public);
            resourceType.BaseType($"TIgorHttpRequest<{requestTypeName}>");
            var requestParams = new List<(UeType ueType, string name)>();
            if (resource.RequestContent != null)
            {
                var contentSetup = resource.ueContentSetup;
                if (resource.RequestContent.Var != null)
                    contentSetup = resource.RequestContent.Var.ueSetup;
                var requestContentName = resource.RequestContent.ueName("RequestContent");
                resourceType.Field(requestContentName, resource.RequestContent.ueType.RelativeConstName(ns), AccessModifier.Public);
                switch (contentSetup)
                {
                    case UeHttpClientRequestSetup.Args:
                        requestParams.Add((resource.RequestContent.ueType, requestContentName));
                        break;
                }
            }
            foreach (var requestVar in resource.RequestVariables)
            {
                var varField = resourceType.Field(requestVar.ueName, requestVar.ueType.RelativeConstName(ns), AccessModifier.Public);
                varField.Comment = requestVar.Annotation;
                if (requestVar.DefaultValue != null)
                    varField.Value = requestVar.ueType.FormatValue(requestVar.DefaultValue, ns);
                switch (requestVar.ueSetup)
                {
                    case UeHttpClientRequestSetup.Args:
                        requestParams.Add((requestVar.ueType, requestVar.ueName));
                        break;
                }
            }

            var requestParamsString = requestParams.JoinStrings(", ", p => $"const {p.ueType.RelativeConstName(ns)}& In{p.name}");
            var commaRequestParamsString = requestParams.Any() ? ", " + requestParams.JoinStrings(", ", p => $"const {p.ueType.RelativeConstName(ns)}& In{p.name}") : "";

            var response = resource.Responses.FirstOrDefault(resp => resp.StatusCode < 300);
            if (response?.Content != null)
            {
                var varName = response.Content.ueName("ResponseContent");
                resourceType.Field(varName, response.Content.ueType.RelativeName(ns), AccessModifier.Private);
                resourceType.Function($@"const {response.Content.ueType.RelativeName(ns)}& Get{varName}() const {{ return {varName}; }};", AccessModifier.Public);
            }

            resourceType.Function($"{requestTypeName}(const TSharedRef<FIgorHttpClient>& InOwnerHttpClient, const FHttpRequestRef& InHttpRequest{commaRequestParamsString}) : TIgorHttpRequest(InOwnerHttpClient, InHttpRequest){requestParams.JoinStrings(p => $", {p.name}(In{p.name})")} {{ }}", AccessModifier.Public);
            resourceType.Function("virtual void SetupRequest() override;", AccessModifier.Protected);
            resourceType.Function("virtual bool ParseResponse(const FHttpResponsePtr& InResponse) override;", AccessModifier.Protected);

            var funText = $@"TSharedRef<{requestTypeName}> {resource.ueName}({requestParamsString});";
            var fun = hclass.Function(funText, AccessModifier.Public);
            fun.Comment = resource.Annotation;

            var r = new Renderer();
            r += $@"void {hclass.Name}::{requestTypeName}::SetupRequest()";
            r += "{";
            r++;
            r += $@"HttpRequest->SetVerb(""{resource.Method}"");";
            if (resource.RequestContent != null)
            {
                resourceType.Field("RequestContentAsString", "FString", AccessModifier.Private);
                resourceType.Function("const FString& GetRequestContentAsString() const { return RequestContentAsString; }", AccessModifier.Public);
                r.Block($@"const TSharedRef<FJsonValue> Json = Igor::IgorWriteJson({resource.RequestContent.ueName("RequestContent")});
const TSharedPtr<TJsonWriter<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>> JsonWriter = TJsonWriterFactory<TCHAR, TCondensedJsonPrintPolicy<TCHAR>>::Create(&RequestContentAsString);
if (!FJsonSerializer::Serialize(Json->AsObject().ToSharedRef(), *JsonWriter))
{{
    UE_LOG({resource.ueLogCategory}, Error, TEXT(""{resource.WebService.ueName}::{requestTypeName} Failed to serialize request content""));
    return;
}}
HttpRequest->SetContentAsString(RequestContentAsString);");
            }
            string pathExpr;
            if (resource.PathVariables.Any())
            {
                r += $@"const FString URLPath = FString::Format(TEXT(""{resource.uePath}""), {{ {resource.PathVariables.JoinStrings(", ", v => v.ueName)} }});";
                pathExpr = "URLPath";
            }
            else
            {
                pathExpr = $@"TEXT(""{resource.uePath}"")";
            }
            string queryParams = "";
            if (resource.Query.Any())
            {
                queryParams = ", QueryParams";
                r += "FIgorQueryParams QueryParams;";
                foreach (var queryParam in resource.Query)
                {
                    if (queryParam.IsStatic)
                    {
                        r += $@"QueryParams.Add(FIgorQueryParam(TEXT(""{queryParam.Parameter}""), TEXT(""{queryParam.StaticValue}""));";
                    }
                    else
                    {
                        var ueType = queryParam.Var.ueType;
                        if (ueType is UeOptionalType optType)
                        {
                            r += $@"if ({queryParam.Var.ueName}.IsSet())";
                            r += "{";
                            r++;
                            r += $@"QueryParams.Add(FIgorQueryParam(TEXT(""{queryParam.Parameter}""), {queryParam.Var.ueStringValue}));";
                            r += "";
                            r--;
                            r += "}";
                        }
                        else
                        {
                            r += $@"QueryParams.Add(FIgorQueryParam(TEXT(""{queryParam.Parameter}""), {queryParam.Var.ueStringValue}));";
                        }
                    }
                }
            }
            r += $"const FString URL = FormatURL(OwnerHttpClient->GetBaseURL(), {pathExpr}{queryParams});";
            r += @"HttpRequest->SetURL(URL);";

            foreach (var header in resource.RequestHeaders)
            {
                if (header.IsStatic)
                    r += $@"HttpRequest->SetHeader(TEXT(""{header.Name}""), TEXT(""{header.StaticValue}""));";
                else
                    r += $@"HttpRequest->SetHeader(TEXT(""{header.Name}""), {header.Var.ueName});";
            }

            r--;
            r += "}";
            cppns.Function(r.Build(), resource);

            r.Reset();
            r += $"bool {hclass.Name}::{requestTypeName}::ParseResponse(const FHttpResponsePtr& InResponse)";
            r += "{";
            r++;
            r += $@"if (InResponse->GetResponseCode() != {response.StatusCode})
{{
    return false;
}}";
            if (response.Content != null)
            {
                switch (response.Content.Format)
                {
                    case DataFormat.Json:
                    default:
                        cppFile.Include("Serialization/JsonReader.h");
                        r += "TSharedPtr<FJsonValue> Json = nullptr;";
                        r += "const TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<TCHAR>::Create(InResponse->GetContentAsString());";
                        r += @"if (!FJsonSerializer::Deserialize(Reader, Json))
{
    return false;
}";
                        r += $@"return Igor::IgorReadJson(Json, {response.Content.ueName("ResponseContent")});";
                        break;
                }
            }
            else
            {
                r += "return true;";
            }
            r--;
            r += "}";
            cppns.Function(r.Build(), resource);

            r.Reset();
            r += $@"TSharedRef<{hclass.Name}::{requestTypeName}> {hclass.Name}::{resource.ueName}({requestParamsString})";
            r += "{";
            r++;
            if (resource.ueLazy)
            {
                r += $"return MakeShareable(new {requestTypeName}(this->AsShared(), CreateHttpRequest(){requestParams.JoinStrings(p => $", In{p.name}")}));";
            }
            else
            {
                r += $"TSharedRef<{requestTypeName}> Request = MakeShareable(new {requestTypeName}(this->AsShared(), CreateHttpRequest(){requestParams.JoinStrings(p => $", In{p.name}")}));";
                r += "Request->ProcessRequest();";
                r += "return Request;";
            }
            r--;
            r += "}";
            cppns.Function(r.Build(), resource);
        }
    }
}
