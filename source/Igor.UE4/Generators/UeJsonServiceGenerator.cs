using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4
{
    internal class UeJsonServiceGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.ueEnabled && service.ueClient && service.jsonEnabled)
                {
                    GenerateService(model, service, Direction.ClientToServer);
                }
                if (service.ueEnabled && service.ueServer && service.jsonEnabled)
                {
                    GenerateService(model, service, Direction.ServerToClient);
                }
            }
        }

        public void GenerateService(UeModel model, ServiceForm s, Direction direction)
        {
            var h = model.HFile(s.Module.ueHFile);
            h.Include("Containers/Union.h");
            h.Include($"{s.ueIgorPath}IgorJsonService.h");

            var intf = s.ueInterfaceName(direction);
            var listenerIntf = s.ueInterfaceName(direction.Opposite());

            var ns = s.ueNamespace;
            var service = h.Namespace(ns).Class(s.ueName);
            service.BaseType("FIgorJsonService");
            service.BaseType(intf);
            service.Function($"{s.ueName}(const TSharedRef<IIgorJsonSender>& InSender, TSharedPtr<{listenerIntf}> InCallback) : FIgorJsonService(InSender), Callback(InCallback) {{ }}", AccessModifier.Public);
            service.Field("Callback", $"TSharedPtr<{listenerIntf}>", AccessModifier.Public);
            service.Function("virtual void Recv(const TSharedRef<FJsonObject>& Json) override;", AccessModifier.Public);
            foreach (var func in s.Casts(direction, FunctionType.Send))
            {
                service.Function($"virtual {func.ueResponseType} {func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.RelativeName(ns)}& {arg.ueName}")}) override;", AccessModifier.Public);
            }
            foreach (var func in s.Casts(direction, FunctionType.Recv))
            {
                service.Function($"void {func.ueRecvName}(const TSharedRef<FJsonObject>& Json);", AccessModifier.Private);
            }
            foreach (var func in s.Casts(direction, FunctionType.SendRpc))
            {
                var rpcType = service.Struct(func.ueRpcTypeName, AccessModifier.Protected);
                rpcType.BaseType($"FRpc<{func.ueRpcResultTypeName}, {func.ueErrorType}>");
                rpcType.Function($@"{func.ueRpcTypeName}(const TSharedRef<TIgorAsyncResult<{func.ueRpcResponseTypeName}>>& InAsyncResult) : FRpc(InAsyncResult) {{ }}", AccessModifier.Public);

                rpcType.Function($@"virtual void ReadResult(const TArray<TSharedPtr<FJsonValue>>& Json, TSharedPtr<{func.ueRpcResultTypeName}>& OutResult) override;", AccessModifier.Public);
                rpcType.Function($@"virtual void ReadError(const TSharedPtr<FJsonValue>& Json, const int32 ErrorCode, TSharedPtr<{func.ueErrorType}>& OutError) override;", AccessModifier.Public);
            }

            var cpp = model.CppFile(s.Module.ueCppFile);
            cpp.Include($"{s.ueIgorPath}IgorJson.h");

            string recvMsg(ServiceFunction func)
            {
                if (func.Direction == direction)
                    return $@"if (Method == TEXT(""{func.jsonKey}""))
{{
    RecvRpc(Json);
}}";
                else
                    return $@"if (Method == TEXT(""{func.jsonKey}""))
{{
    {func.ueRecvName}(Json);
}}";
            }

            var cppRecv =
                    s.Casts(direction, FunctionType.Recv | FunctionType.Rpc).Any() ?
                $@"void {s.ueQualifiedName}::Recv(const TSharedRef<FJsonObject>& Json)
{{
    FString Method = Json->GetStringField(TEXT(""method""));
{s.Casts(direction, FunctionType.Recv | FunctionType.Rpc).JoinStrings("\nelse ", recvMsg)}
    else
    {{
        verifyf(false, TEXT(""Unknown service function %s""), *Method);
    }}
}}"
            :
$@"void {s.ueQualifiedName}::Recv(const TSharedRef<FJsonObject>& Json)
{{
    FString Method = Json->GetStringField(TEXT(""method""));
    verifyf(false, TEXT(""Unknown service function %s""), *Method);
}}";

            cpp.DefaultNamespace.Function(cppRecv, s);

            foreach (var func in s.Casts(direction, FunctionType.SendRpc))
            {
                var readResult = $@"void {s.ueQualifiedName}::{func.ueRpcTypeName}::ReadResult(const TArray<TSharedPtr<FJsonValue>>& Json, TSharedPtr<{func.ueRpcResultTypeName}>& OutResult)
{{
    OutResult = MakeShareable(new {func.ueRpcResultTypeName}());
{func.ReturnArguments.SelectWithIndex((arg, i) => $"    Igor::IgorReadJson(Json[{i}], OutResult->{arg.ueName});").JoinLines()}
}}";

                var readError = new Renderer();
                readError += $@"void {s.ueQualifiedName}::{func.ueRpcTypeName}::ReadError(const TSharedPtr<FJsonValue>& Json, const int32 ErrorCode, TSharedPtr<{func.ueErrorType}>& OutError)";
                readError += "{";
                readError++;
                if (func.Throws.Any())
                {
                    readError += "switch (ErrorCode)";
                    readError += "{";
                    foreach (var error in func.Throws)
                    {
                        readError += $@"case {error.Id}:
{{
    {error.Exception.ueType.RelativeName(ns)} {error.Exception.ueVarName};
    if (Igor::IgorReadJson(Json, {error.Exception.ueVarName}))
    {{
        OutError = MakeShareable(new {func.ueErrorType}({error.Exception.ueVarName}));
    }}
}}
break;";
                    }
                    readError += "}";
                }

                readError--;
                readError += "}";

                cpp.DefaultNamespace.Function(readResult, s);
                cpp.DefaultNamespace.Function(readError.Build(), s);
            }

            foreach (var func in s.Casts(direction, FunctionType.Send))
            {
                var sendImpl =
                    func.IsRpc ?
$@"TSharedRef<TIgorAsyncResult<{UeName.RelativeName(ns, intf, null)}::{func.ueRpcResponseTypeName}>> {s.ueQualifiedName}::{func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.QualifiedName}& {arg.ueName}")})
{{
    TSharedRef<FJsonObject> Json = MakeShareable(new FJsonObject());
    int RpcId = GenerateRpcId();
    Json->SetStringField(TEXT(""method""), TEXT(""{func.jsonKey}""));
    Json->SetNumberField(TEXT(""id""), RpcId);
    TArray<TSharedPtr<FJsonValue>> Params;
{func.Arguments.JoinLines(arg => $"Params.Add(Igor::IgorWriteJson({arg.ueName}));")}
    Json->SetArrayField(TEXT(""params""), Params);
    Sender->Send(Json);
    TSharedRef<TIgorAsyncResult<{func.ueRpcResponseTypeName}>> AsyncResult = MakeShareable(new TIgorAsyncResult<{func.ueRpcResponseTypeName}>());
    AddRpc(RpcId, MakeShareable(new {func.ueRpcTypeName}(AsyncResult)));
    return AsyncResult;
}}"
                    :
$@"void {s.ueQualifiedName}::{func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.QualifiedName}& {arg.ueName}")})
{{
    TSharedRef<FJsonObject> Json = MakeShareable(new FJsonObject());
    Json->SetStringField(TEXT(""method""), TEXT(""{func.jsonKey}""));
    TArray<TSharedPtr<FJsonValue>> Params;
{func.Arguments.JoinLines(arg => $"Params.Add(Igor::IgorWriteJson({arg.ueName}));")}
    Json->SetArrayField(TEXT(""params""), Params);
    Sender->Send(Json);
}}";
                cpp.DefaultNamespace.Function(sendImpl, s);
            }

            foreach (var func in s.Casts(direction, FunctionType.Recv))
            {
                string recvImpl;
                if (!func.IsRpc)
                {
                    var parseParams = func.Arguments.Any() ? @"    TArray<TSharedPtr<FJsonValue>> Params = Json->GetArrayField(TEXT(""params""));" : "";
                    recvImpl =
$@"void {s.ueQualifiedName}::{func.ueRecvName}(const TSharedRef<FJsonObject>& Json)
{{
{func.Arguments.JoinLines(arg => $"{arg.ueType.QualifiedName} {arg.ueName};")}
{parseParams}
{ func.Arguments.SelectWithIndex((arg, i) => $"Igor::IgorReadJson(Params[{i}], {arg.ueName});").JoinLines()}
    if (Callback.IsValid())
    {{
        Callback->{func.ueName}({func.Arguments.JoinStrings(", ", arg => arg.ueName)});
    }}
}}";
                }
                else
                {
                    recvImpl =
$@"void {s.ueQualifiedName}::{func.ueRecvName}(const TSharedRef<FJsonObject>& Json)
{{
{func.Arguments.JoinLines(arg => $"{arg.ueType.QualifiedName} {arg.ueName};")}
{func.Arguments.JoinLines(arg => $"Igor::IgorReadJson(Json, {arg.ueName});")}
    if (Callback.IsValid())
    {{
        Callback->{func.ueName} ({func.Arguments.JoinStrings(", ", arg => arg.ueName)});
    }}
}}";
                }

                cpp.DefaultNamespace.Function(recvImpl, s);
            }
        }
    }
}
