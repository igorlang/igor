using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Binary
{
    internal class ErlBinaryServiceGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.erlEnabled && service.binaryEnabled)
                {
                    GenService(model, service);
                }
            }
        }

        private void GenService(ErlModel model, ServiceForm service)
        {
            var erl = model.Module(service.erlFileName);
            erl.IncludeLib("stdlib/include/assert.hrl");
            erl.IncludeLib("igor/include/igor_binary.hrl");

            GenParse(erl, service, Direction.ClientToServer);
            GenPack(erl, service, Direction.ClientToServer);
            GenParse(erl, service, Direction.ServerToClient);
            GenPack(erl, service, Direction.ServerToClient);
        }

        private void GenParse(ErlModule erl, ServiceForm service, Direction direction)
        {
            string parseFunName = direction == Direction.ServerToClient ? "s2c_message_from_binary" : "c2s_message_from_binary";

            string ParseCast(ServiceFunction fun) =>
$@"{parseFunName}(<<?byte({fun.Index}),Binary/binary>>) ->
{ErlIgorReader.Render(fun.Arguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag))).Indent(4)}
    {{{fun.erlRequestTermArgs}, Tail}}";

            string ParseRpcRequest(ServiceFunction fun) =>
$@"{parseFunName}(<<?byte({fun.Index}),?int(RpcId),Binary/binary>>) ->
{ErlIgorReader.Render(fun.Arguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag))).Indent(4)}
    {{{fun.erlRequestTermArgs}, Tail}}";

            string ParseRpcFailure(FunctionThrow thr) =>
$@"{parseFunName}(<<?byte({thr.Function.Index}),?ushort(RpcId),?byte({thr.Id}),Binary/binary>>) ->
    {{Exception, Tail}} = {BinarySerialization.ParseBinary(thr.Exception.erlBinaryTag(thr.Function), "Binary", erl.Name)},
    {{{{fail_{thr.Function.erlName}, RpcId, Exception}}, Tail}};";

            string ParseRpcReply(ServiceFunction fun) =>
    $@"{parseFunName}(<<?byte({fun.Index}),?int(RpcId),?byte(0),Binary/binary>>) ->
{ErlIgorReader.Render(fun.ReturnArguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag))).Indent(4)}
    {{{fun.erlReplyTermArgs}, Tail}};
{fun.Throws.JoinLines(ParseRpcFailure)}
{parseFunName}(<<?byte({fun.Index}),?int(RpcId),?byte(255),?binary(_Size,ExceptionString),Tail/binary>>) ->
    Exception = {{unknown_error, ExceptionString}},
    {{{fun.erlFailTermArgs}, Tail}}";

            string Parse(ServiceFunction fun)
            {
                switch (fun.Type(direction))
                {
                    case FunctionType.SendCast: return ParseCast(fun);
                    case FunctionType.SendRpc: return ParseRpcRequest(fun);
                    case FunctionType.RecvRpc: return ParseRpcReply(fun);
                    default: throw new EInternal($"No recv clause is possible for type {fun.Type(direction)}");
                }
            }

            var messages = service.Messages(direction);

            if (messages.Any())
            {
                erl.Export(parseFunName, 1);
                erl.Function(
    $@"-spec {parseFunName}(binary()) -> {{{service.erlMessageType(direction)}, Tail :: binary()}}.

{messages.JoinStrings(";\n", Parse)}.
");
            }
        }

        private void GenPack(ErlModule erl, ServiceForm service, Direction direction)
        {
            string packFunName = direction == Direction.ServerToClient ? "s2c_message_to_iodata" : "c2s_message_to_iodata";

            string PackCast(ServiceFunction fun) =>
$@"{packFunName}({fun.erlRequestTermArgs}) ->
{fun.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlIgorWriter.Render(new[] { (fun.Index.ToString(), SerializationTags.Byte) }.Concat(fun.Arguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag)))).Indent(4)}";

            string PackRpcRequest(ServiceFunction fun) =>
$@"{packFunName}({fun.erlRequestTermArgs}) ->
{fun.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlIgorWriter.Render(new[] { (fun.Index.ToString(), SerializationTags.Byte), ("RpcId", SerializationTags.Int) }.Concat(fun.Arguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag)))).Indent(4)}";

            string PackRpcFailure(FunctionThrow thr) =>
$@"{packFunName}({{{thr.Function.erlFailFunName}, RpcId, Exception}}) when element(1, Exception) =:= {thr.Exception.erlRecordName} ->
{ErlIgorWriter.Render((thr.Function.Index.ToString(), SerializationTags.Byte), ("RpcId", SerializationTags.Int), (thr.Id.ToString(), SerializationTags.Byte), ("Exception", thr.Exception.erlBinaryTag(thr.Function))).Indent(4)};";

            string PackRpcReply(ServiceFunction fun) =>
    $@"{packFunName}({fun.erlReplyTermArgs}) ->
{fun.ReturnArguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlIgorWriter.Render(new[] { (fun.Index.ToString(), SerializationTags.Byte), ("RpcId", SerializationTags.Int), ("0", SerializationTags.Byte) }.Concat(fun.ReturnArguments.Select(arg => (arg.erlVarName, arg.erlBinaryTag)))).Indent(4)};
{fun.Throws.JoinLines(PackRpcFailure)}
{packFunName}({{{fun.erlFailFunName}, RpcId, {{unknown_error, ExceptionString}}}}) ->
{ErlIgorWriter.Render((fun.Index.ToString(), SerializationTags.Byte), ("RpcId", SerializationTags.Int), ("255", SerializationTags.Byte), ("ExceptionString", SerializationTags.String)).Indent(4)}";

            string Pack(ServiceFunction fun)
            {
                switch (fun.Type(direction))
                {
                    case FunctionType.SendCast: return PackCast(fun);
                    case FunctionType.SendRpc: return PackRpcRequest(fun);
                    case FunctionType.RecvRpc: return PackRpcReply(fun);
                    default: throw new EInternal($"No recv clause is possible for type {fun.Type(direction)}");
                }
            }

            var messages = service.Messages(direction);

            if (messages.Any())
            {
                erl.Export(packFunName, 1);
                erl.Function(
    $@"-spec {packFunName}({service.erlMessageType(direction)}) -> iodata().

{messages.JoinStrings(";\n", Pack)}.
");
            }
        }
    }
}
