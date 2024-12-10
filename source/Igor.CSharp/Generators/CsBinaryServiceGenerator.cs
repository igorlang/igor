using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsBinaryServiceGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.csEnabled && serviceForm.csBinaryClient)
                {
                    GenerateService(serviceForm, model, Direction.ClientToServer);
                }
                if (serviceForm.csEnabled && serviceForm.csBinaryServer)
                {
                    GenerateService(serviceForm, model, Direction.ServerToClient);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, CsModel model, Direction direction)
        {
            var file = model.File(serviceForm.Module.csFileName);
            var ns = file.Namespace(serviceForm.csNamespace);
            file.Use("System.IO");
            CsVersion.UsingTasks(file);
            file.UseAlias("IgorSerializer", "Igor.Serialization.IgorSerializer");
            var service = ns.Class(serviceForm.csClassName(direction));
            service.AddAttributes(serviceForm.ListAttribute(CsAttributes.Attribute));
            service.BaseClass = "Igor.Services.Binary.BinaryService";
            service.Interface(serviceForm.csInterfaceName(direction));
            service.Property("Listener", $"public {serviceForm.csInterfaceName(direction.Opposite())} Listener {{ get; set; }}");
            service.Constructor(
$@"
public {serviceForm.csClassName(direction)}(Igor.Services.Binary.ISender sender) :
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
                        break;
                    case FunctionType.RecvCast:
                        service.Method(RecvCast(func));
                        break;
                    case FunctionType.RecvRpc:
                        service.Method(RecvRpc(func));
                        break;
                }
            }

            string RecvCase(ServiceFunction func)
            {
                return
$@"        case {func.Index}:
            Recv_{func.csName}(reader);
            break;";
            }

            service.Method($@"
public override void Recv(BinaryReader reader)
{{
    byte msgId = reader.ReadByte();
    switch (msgId)
    {{
{serviceForm.Functions.Where(f => f.Direction != direction || f.IsRpc).JoinLines(RecvCase)}
        default:
            throw new Igor.Services.Binary.UnknownServiceFunctionException(msgId);
    }}
}}
");
        }

        private string RequireArgument(FunctionArgument arg) =>
$@"if ({arg.csArgName} == null)
    throw new System.ArgumentNullException({CsVersion.NameOf(arg.csArgName)});";

        public string SendCast(ServiceFunction func)
        {
            return $@"
public void {func.csName}({func.csTypedArgs})
{{
{func.Arguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var packet = Sender.GetSendPacket();
    var writer = packet.Writer;
    writer.Write((byte){func.Index});
{func.Arguments.JoinLines(WriteArg)}
    packet.Send();
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
            var setResult = func.ReturnArguments.Any() ?
                $"SucceedRpc(rpcId, new {func.csRpcQualifiedResultClassName}({func.csReturns}));"
                : "SucceedRpc(rpcId);";
            return $@"
public {func.csRpcTaskClass} {func.csName}({func.csTypedArgs})
{{
{func.Arguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var id = GenerateRpcId();
    var rpcTask = CreateRpc{RpcGenericArg(func)}(id);
    var packet = Sender.GetSendPacket();
    var writer = packet.Writer;
    writer.Write((byte){func.Index});
    writer.Write(id);
{func.Arguments.JoinLines(WriteArg)}
    packet.Send();
    return rpcTask;
}}

void Recv_{func.csName}(BinaryReader reader)
{{
    int rpcId = reader.ReadInt32();
    byte igorResult = reader.ReadByte();
    switch (igorResult)
    {{
        case 0:
            {{
{func.ReturnArguments.JoinLines(ReadArg).Indent(12)}
                {setResult}
            }}
            break;
{func.Throws.JoinStrings(RecvThrow)}

        case byte.MaxValue:
            {{
                var igorMessage = reader.ReadString();
                FailRpc{RpcGenericArg(func)}(rpcId, new Igor.Services.UnknownRemoteException(igorMessage));
            }}
            break;
    }}
}}
";
        }

        public string RecvCast(ServiceFunction func)
        {
            return $@"
void Recv_{func.csName}(BinaryReader reader)
{{
{func.Arguments.JoinLines(ReadArg)}
    if (Listener != null)
        Listener.{func.Name}({func.csArgs});
    else
        UnhandledRecv(""{func.csName}"");
}}
";
        }

        public string RecvRpc(ServiceFunction func)
        {
            var typedReturns = string.IsNullOrEmpty(func.csTypedReturns) ? "" : ", " + func.csTypedReturns;
            var exceptions = func.Throws.Any() ?
$@"{func.Throws.JoinStrings(WriteThrow)}
{{
    writer.Write(byte.MaxValue);
    writer.Write(exception.Message);
}}" :
@"writer.Write(byte.MaxValue);
writer.Write(exception.Message);";

            return
$@"
void SendReply_{func.csName}(int rpcId{typedReturns})
{{
{func.ReturnArguments.Where(arg => arg.csType.csNotNullRequired).JoinLines(RequireArgument).Indent(4)}
    var packet = Sender.GetSendPacket();
    var writer = packet.Writer;
    writer.Write((byte){func.Index});
    writer.Write(rpcId);
    writer.Write((byte)0);
{func.ReturnArguments.JoinLines(WriteArg)}
    packet.Send();
}}

void SendFail_{func.csName}(int rpcId, System.Exception exception)
{{
    var packet = Sender.GetSendPacket();
    var writer = packet.Writer;
    writer.Write((byte){func.Index});
    writer.Write(rpcId);
{exceptions.Indent(4)}
    packet.Send();
}}

void Recv_{func.csName}(BinaryReader reader)
{{
    int rpcId = reader.ReadInt32();
{func.Arguments.JoinLines(ReadArg)}
    if (Listener != null)
        Listener.{func.csName}({func.csArgs}).ContinueWith(t => SendRpcResult_{func.csName}(t, rpcId));
    else
        UnhandledRecv(""{func.csName}"");
}}

void SendRpcResult_{func.csName}({func.csRpcTaskClass} task, int rpcId)
{{
    if (task.IsFaulted)
        SendFail_{func.csName}(rpcId, task.{CsVersion.TaskException});
    else
        SendReply_{func.csName}(rpcId{func.ReturnArguments.JoinStrings(arg => $", task.Result.{arg.csName}")});
}}
";
        }

        private string WriteArg(FunctionArgument arg)
        {
            var igorSerializer = arg.csType.binarySerializer(arg.Function.Service.csNamespace);
            return $@"    {igorSerializer}.Serialize(writer, {arg.csArgName});";
        }

        private string ReadArg(FunctionArgument arg)
        {
            return $"    {arg.csTypeName} {arg.csArgName} = {arg.csType.binarySerializer(arg.Function.Service.csNamespace)}.Deserialize(reader);";
        }

        public string WriteThrow(FunctionThrow @throw) =>
            $@"if (exception is {@throw.csTypeName})
{{
    writer.Write((byte){@throw.Id});
    {@throw.Exception.csBinarySerializerInstance(@throw.Function.Service.csNamespace)}.Serialize(writer, exception as {@throw.csTypeName});
}}
else";

        public string RecvThrow(FunctionThrow @throw) =>
$@"
        case {@throw.Id}:
            {{
                var igorException = {@throw.Exception.csBinarySerializerInstance(@throw.Function.Service.csNamespace)}.Deserialize(reader);
                FailRpc{RpcGenericArg(@throw.Function)}(rpcId, igorException);
            }}
            break;
";
    }
}
