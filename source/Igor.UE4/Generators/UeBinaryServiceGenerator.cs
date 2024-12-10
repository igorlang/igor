using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4
{
    internal class UeBinaryServiceGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.ueEnabled && service.ueClient && service.binaryEnabled)
                {
                    GenerateService(model, service, Direction.ClientToServer);
                }
                if (service.ueEnabled && service.ueServer && service.binaryEnabled)
                {
                    GenerateService(model, service, Direction.ServerToClient);
                }
            }
        }

        public void GenerateService(UeModel model, ServiceForm s, Direction direction)
        {
            var h = model.HFile(s.Module.ueHFile);
            h.Include($"{s.ueIgorPath}IgorBinaryService.h");

            var service = h.Namespace(s.ueNamespace).Class(s.ueName);
            service.BaseType("FIgorBinaryService");
            service.Function($"{s.ueName}(const TSharedRef<IIgorBinarySender>& InSender) : FIgorService(InSender) {{ }}", AccessModifier.Public);
            service.Field("Listener", $"TSharedPtr<{s.ueInterfaceName(direction.Opposite())}>", AccessModifier.Public);
            service.Function("virtual void Recv(FIgorBinaryReader& Reader) override;", AccessModifier.Public);
            foreach (var func in s.Casts(direction, FunctionType.Send))
            {
                var resultType = func.IsRpc ? $"TSharedPtr<const {func.ueRpcTypeName}>" : "void";
                service.Function($"{resultType} {func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.QualifiedName}& {arg.ueName}")});", AccessModifier.Public);
            }
            foreach (var func in s.Casts(direction, FunctionType.Recv | FunctionType.Rpc))
            {
                service.Function($"void {func.ueRecvName}(FIgorBinaryReader& Reader);", AccessModifier.Private);
            }
            foreach (var func in s.Casts(direction, FunctionType.SendRpc))
            {
                var rpcType = service.Class(func.ueRpcTypeName);
                rpcType.BaseType("FIgorRpc");
                rpcType.Friend(s.ueName);
                rpcType.Function($"{func.ueRpcTypeName}(uint16 InRpcId) : FIgorRpc(InRpcId) {{ }}", AccessModifier.Public);
                foreach (var ret in func.ReturnArguments)
                {
                    rpcType.Field(ret.ueName, ret.ueType.QualifiedName, AccessModifier.Public);
                }
            }

            var cpp = model.CppFile(s.Module.ueCppFile);
            var igorPath = s.ueIgorPath;
            cpp.Include($"{igorPath}IgorBinary.h");
            cpp.Include($"{igorPath}IgorReader.h");
            cpp.Include($"{igorPath}IgorWriter.h");

            string recvMsg(ServiceFunction func) =>
$@"case {func.Index}:
    {func.ueRecvName}(Reader);
    break;";

            var cppRecv =
                    s.Casts(direction, FunctionType.Recv | FunctionType.Rpc).Any() ?
                $@"void {s.ueName}::Recv(FIgorBinaryReader& Reader)
{{
    uint8 MsgId = Reader.ReadByte();
    switch (MsgId)
    {{
{s.Casts(direction, FunctionType.Recv | FunctionType.Rpc).JoinLines(recvMsg)}
    default:
        verifyf(false, TEXT(""Unknown service function %d""), MsgId);
        break;
    }}
}}"
            :
$@"void {s.ueName}::Recv(FIgorBinaryReader& Reader)
{{
    uint8 MsgId = Reader.ReadByte();
    verifyf(false, TEXT(""Unknown service function %d""), MsgId);
}}";

            cpp.DefaultNamespace.Function(cppRecv, s);

            foreach (var func in s.Casts(direction, FunctionType.Send))
            {
                var sendImpl =
                    func.IsRpc ?
$@"TSharedPtr<const {s.ueName}::{func.ueRpcTypeName}> {s.ueName}::{func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.QualifiedName}& {arg.ueName}")})
{{
    uint16 RpcId = GenerateRpcId();
    TSharedPtr<{func.ueRpcTypeName}> Rpc = MakeShareable(new {func.ueRpcTypeName}(RpcId));
    FIgorBinaryWriter& Writer = Sender->BeginSend();
    Writer.WriteByte({func.Index});
    Igor::IgorWriteBinary(Writer, RpcId);
{func.Arguments.JoinLines(arg => $"Igor::IgorWriteBinary(Writer, {arg.ueName});")}
    Sender->EndSend();
    AddRpc(Rpc);
    return Rpc;
}}"
                    :
$@"void {s.ueName}::{func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.QualifiedName}& {arg.ueName}")})
{{
    FIgorBinaryWriter& Writer = Sender->BeginSend();
    Writer.WriteByte({func.Index});
{func.Arguments.JoinLines(arg => $"Igor::IgorWriteBinary(Writer, {arg.ueName});")}
    Sender->EndSend();
}}";
                cpp.DefaultNamespace.Function(sendImpl, s);
            }

            foreach (var func in s.Casts(direction, FunctionType.Recv | FunctionType.Rpc))
            {
                string recvImpl;
                if (func.Type(direction) == FunctionType.RecvCast)
                    recvImpl =
$@"void {s.ueName}::{func.ueRecvName}(FIgorBinaryReader& Reader)
{{
{func.Arguments.JoinLines(arg => $"{arg.ueType.QualifiedName} {arg.ueName};")}
{func.Arguments.JoinLines(arg => $"Igor::IgorReadBinary(Reader, {arg.ueName});")}
    if (Listener.IsValid())
    {{
        Listener->{func.ueName}({func.Arguments.JoinStrings(", ", arg => arg.ueName)});
    }}
}}";
                else if (func.Type(direction) == FunctionType.RecvRpc)
                    recvImpl =
$@"void {s.ueName}::{func.ueRecvName}(FIgorBinaryReader& Reader)
{{
{func.Arguments.JoinLines(arg => $"{arg.ueType.QualifiedName} {arg.ueName};")}
{func.Arguments.JoinLines(arg => $"Igor::IgorReadBinary(Reader, {arg.ueName});")}
    if (Listener.IsValid())
    {{
        Listener->{func.ueName} ({func.Arguments.JoinStrings(", ", arg => arg.ueName)});
    }}
}}";
                else // if (func.type == FunctionType.SendRpc)
                    recvImpl =
$@"void {s.ueName}::{func.ueRecvName}(FIgorBinaryReader& Reader)
{{
    uint16 RpcId;
    Igor::IgorReadBinary(Reader, RpcId);
    TSharedPtr<{func.ueRpcTypeName}> Rpc = FindRpc<{func.ueRpcTypeName}>(RpcId);
    uint8 IgorResult = Reader.ReadByte();
    switch (IgorResult)
    {{
    case 0:
        {{
{func.ReturnArguments.JoinLines(arg => $"Igor::IgorReadBinary(Reader, Rpc->{arg.ueName});")}
            Rpc->Succeed();
        }}
        break;

    case 255:
        {{
            FString ErrorString;
            Igor::IgorReadBinary(Reader, ErrorString);
            Rpc->Fail(ErrorString);
        }}
        break;

    default:
        verifyf(false, TEXT(""Unknown exception id %d""), IgorResult);
        break;
    }}
}}";

                cpp.DefaultNamespace.Function(recvImpl, s);
            }
        }
    }
}
