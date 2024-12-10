using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsJsonServiceGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.csEnabled && serviceForm.csJsonClient)
                {
                    GenerateService(serviceForm, model, Direction.ClientToServer);
                }
                if (serviceForm.csEnabled && serviceForm.csJsonServer)
                {
                    GenerateService(serviceForm, model, Direction.ServerToClient);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, CsModel model, Direction direction)
        {
            var file = model.File(serviceForm.Module.csFileName);
            var ns = file.Namespace(serviceForm.csNamespace);
            CsVersion.UsingTasks(file);
            file.UseAlias("JsonSerializer", "Json.Serialization.JsonSerializer");
            var service = ns.Class(serviceForm.csClassName(direction));
            service.AddAttributes(serviceForm.ListAttribute(CsAttributes.Attribute));
            service.BaseClass = "Igor.Services.Json.JsonService";
            service.Interface(serviceForm.csInterfaceName(direction));
            service.Property("Listener", $"public {serviceForm.csInterfaceName(direction.Opposite())} Listener {{ get; set; }}");
            service.Constructor(
$@"
public {serviceForm.csClassName(direction)}(Igor.Services.Json.IJsonSender sender) :
    base(sender)
{{
}}
");

            foreach (var func in serviceForm.Functions)
            {
                switch (func.Type(direction))
                {
                    case FunctionType.SendCast:
                        service.Method(SendCast(func));
                        break;
                    case FunctionType.SendRpc:
                        service.Method(SendRpc(func));
                        service.Method(RecvRpcResult(func));
                        break;
                    case FunctionType.RecvCast:
                        service.Method(RecvCast(func));
                        break;
                    case FunctionType.RecvRpc:
                        service.Method(RecvRpc(func));
                        service.Method(SendRpcResponse(func));
                        service.Method(SendRpcResult(func));
                        service.Method(SendRpcError(func));
                        break;
                }
            }

            string RecvCase(ServiceFunction func)
            {
                return
$@"        case ""{func.jsonKey}"":
            Recv_{func.jsonKey}(jsonObject);
            break;";
            }

            service.Method($@"
public override void Recv(Json.ImmutableJson json)
{{
    var jsonObject = json.AsObject;
    string method = jsonObject[""method""].AsString;
    switch (method)
    {{
{serviceForm.Functions.Where(f => f.Direction != direction || f.IsRpc).JoinLines(RecvCase)}
        default:
            throw new System.MissingMethodException(""{serviceForm.Name}"", method);
    }}
}}
");
        }

        private string RequireArgument(FunctionArgument arg) =>
$@"if ({arg.csArgName} == null)
    throw new System.ArgumentNullException({CsVersion.NameOf(arg.csArgName)});";

        public string SendCast(ServiceFunction func)
        {
            var message = new Dictionary<string, string>
            {
                ["method"] = func.jsonKey.Quoted(),
            };
            if (func.Arguments.Any())
                message["params"] = $"new Json.JsonArray{CsSyntaxUtils.CollectionInitializer(func.Arguments.Select(WriteArg), "()", 4)}";
            return $@"
public void {func.csName}({func.csTypedArgs})
{{
{func.Arguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var messageJson = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(message, "()", 4, true)};
    Sender.Send(messageJson);
}}
";
        }

        private string RpcGenericArg(ServiceFunction func)
        {
            if (func.ReturnArguments.Any())
                return $"<{func.csRpcQualifiedResultClassName}>";
            else
                return "";
        }

        public string SendRpc(ServiceFunction func)
        {
            var message = new Dictionary<string, string>
            {
                ["method"] = func.jsonKey.Quoted(),
                ["id"] = "rpcId"
            };
            if (func.Arguments.Any())
                message["params"] = $"new Json.JsonArray{CsSyntaxUtils.CollectionInitializer(func.Arguments.Select(WriteArg), "()", 4)}";
            return $@"
public {func.csRpcTaskClass} {func.csName}({func.csTypedArgs})
{{
{func.Arguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var rpcId = GenerateRpcId();
    var rpcTask = CreateRpc{RpcGenericArg(func)}(rpcId);
    var messageJson = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(message, "()", 4, true)};
    Sender.Send(messageJson);
    return rpcTask;
}}";
        }

        public string RecvRpcResult(ServiceFunction func)
        {
            var setResult = func.ReturnArguments.Any() ?
                $"SucceedRpc(rpcId, new {func.csRpcQualifiedResultClassName}({func.csReturns}));"
                : "SucceedRpc(rpcId);";
            return $@"void Recv_{func.csName}(Json.ImmutableJsonObject json)
{{
    int rpcId = json[""id""].AsInt;
    if (json.TryGetValue(""error"", out var error))
    {{
        var code = error[""code""].AsInt;
        switch (code)
        {{
{func.Throws.JoinStrings(RecvThrow)}
        case byte.MaxValue:
        default:
            {{
                var errorMessage = error[""message""].AsString;
                FailRpc{RpcGenericArg(func)}(rpcId, new Igor.Services.UnknownRemoteException(errorMessage));
            }}
            break;
        }}
    }}
    else
    {{
        var result = json[""result""].AsArray;
{func.ReturnArguments.JoinLines(ReadResultArg).Indent(8)}
        {setResult}
    }}
}}
";
        }

        public string RecvCast(ServiceFunction func)
        {
            string readParams = null;
            if (func.Arguments.Any())
            {
                readParams = $@"    var paramsArray = json[""params""].AsArray;
{func.Arguments.JoinLines(ReadArg).Indent(4)}";
            }
            return $@"
void Recv_{func.csName}(Json.ImmutableJsonObject json)
{{
{readParams}
    if (Listener != null)
        Listener.{func.Name}({func.csArgs});
    else
        UnhandledRecv(""{func.csName}"");
}}
";
        }

        public string RecvRpc(ServiceFunction func)
        {
            string readParams = null;
            if (func.Arguments.Any())
            {
                readParams = $@"    var paramsArray = json[""params""].AsArray;
{func.Arguments.JoinLines(ReadArg).Indent(4)}";
            }
            return
$@"void Recv_{func.csName}(Json.ImmutableJsonObject json)
{{
{readParams}
    int rpcId = json[""id""].AsInt;
    if (Listener != null)
        Listener.{func.csName}({func.csArgs}).ContinueWith(t => SendRpcResult_{func.csName}(t, rpcId));
    else
        UnhandledRecv(""{func.csName}"");
}}";
        }

        public string SendRpcResponse(ServiceFunction func)
        {
            return
$@"void SendRpcResult_{func.csName}({func.csRpcTaskClass} task, int rpcId)
{{
    if (task.IsFaulted)
        SendFail_{func.csName}(rpcId, task.{CsVersion.TaskException});
    else
        SendReply_{func.csName}(rpcId{func.ReturnArguments.JoinStrings(arg => $", task.Result.{arg.csName}")});
}}
";
        }

        public string SendRpcResult(ServiceFunction func)
        {
            var typedReturns = string.IsNullOrEmpty(func.csTypedReturns) ? "" : ", " + func.csTypedReturns;
            var resultMessage = new Dictionary<string, string>
            {
                ["id"] = "rpcId",
                ["method"] = func.jsonKey.Quoted(),
                ["result"] = func.ReturnArguments.Any() ? $"new Json.JsonArray{CsSyntaxUtils.CollectionInitializer(func.ReturnArguments.Select(WriteArg), "()", 4)}" : "Json.ImmutableJson.EmptyArray"
            };
            return
$@"
void SendReply_{func.csName}(int rpcId{typedReturns})
{{
{func.ReturnArguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var messageJson = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(resultMessage, "()", 4, true)};
    Sender.Send(messageJson);
}}";
        }

        public string SendRpcError(ServiceFunction func)
        {
            var typedReturns = string.IsNullOrEmpty(func.csTypedReturns) ? "" : ", " + func.csTypedReturns;
            var errorMessage = new Dictionary<string, string>
            {
                ["id"] = "rpcId",
                ["method"] = func.jsonKey.Quoted()
            };
            var defaultError = new Dictionary<string, string> { ["code"] = "255", ["message"] = "exception.Message" };
            if (!func.Throws.Any())
            {
                errorMessage["error"] = $"new Json.JsonObject{CsSyntaxUtils.IndexInitializer(defaultError, "()", 4, true)}";
            }
            var exceptions = func.Throws.Any() ?
$@"{func.Throws.JoinStrings(WriteThrow)}
{{
    messageJson[""error""] = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(defaultError, "()", 4, true)};
}}" : "";
            return
$@"void SendFail_{func.csName}(int rpcId, System.Exception exception)
{{
    var messageJson = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(errorMessage, "()", 4, true)};
{exceptions.Indent(4)}
    Sender.Send(messageJson);
}}";
        }

        private string WriteArg(FunctionArgument arg) =>
            $@"{arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Serialize({arg.csArgName})";

        private string ReadArg(FunctionArgument arg) =>
            $"{arg.csTypeName} {arg.csArgName} = {arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Deserialize(paramsArray[{arg.Index}]);";

        private string ReadResultArg(FunctionArgument arg) =>
            $"{arg.csTypeName} {arg.csArgName} = {arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Deserialize(result[{arg.Index}]);";

        public string WriteThrow(FunctionThrow @throw)
        {
            var data = $"{@throw.Exception.csJsonSerializerInstance(@throw.Function.Service.csNamespace)}.Serialize(exception as {@throw.csTypeName})";
            var error = new Dictionary<string, string> { ["code"] = @throw.Id.ToString(), ["message"] = "exception.Message", ["data"] = data };
            return $@"if (exception is {@throw.csTypeName})
{{
    messageJson[""error""] = new Json.JsonObject{CsSyntaxUtils.IndexInitializer(error, "()", 4, true)};
}}
else";
        }

        public string RecvThrow(FunctionThrow @throw)
        {
            return $@"
        case {@throw.Id}:
            {{
                var igorException = {@throw.Exception.csJsonSerializerInstance(@throw.Function.Service.csNamespace)}.Deserialize(error[""data""]);
                FailRpc{RpcGenericArg(@throw.Function)}(rpcId, igorException);
            }}
            break;
";
        }
    }
}
