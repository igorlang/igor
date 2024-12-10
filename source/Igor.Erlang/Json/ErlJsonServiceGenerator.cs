using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Json
{
    internal class ErlJsonServiceGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.erlEnabled && service.jsonEnabled)
                {
                    GenService(model, service);
                }
            }
        }

        private void GenService(ErlModel model, ServiceForm service)
        {
            var erl = model.Module(service.erlFileName);
            erl.IncludeLib("stdlib/include/assert.hrl");

            GenParse(erl, service, Direction.ClientToServer);
            GenPack(erl, service, Direction.ClientToServer);
            GenParse(erl, service, Direction.ServerToClient);
            GenPack(erl, service, Direction.ServerToClient);
        }

        private void GenParse(ErlModule erl, ServiceForm service, Direction direction)
        {
            string parseFunName = direction == Direction.ServerToClient ? "s2c_message_from_json" : "c2s_message_from_json";

            string ParseCast(ServiceFunction fun) =>
$@"{parseFunName}(#{{<<""method"">> := <<""{fun.jsonKey}"">>{fun.erlJsonMaybeParams}}}) ->
{fun.erlJsonParseParams}
    {fun.erlRequestTermArgs}";

            string ParseRpcRequest(ServiceFunction fun) =>
$@"{parseFunName}(#{{<<""method"">> := <<""{fun.jsonKey}"">>, <<""id"">> := RpcId{fun.erlJsonMaybeParams}}}) ->
{fun.erlJsonParseParams}
    {fun.erlRequestTermArgs}";

            string ParseRpcFailure(FunctionThrow thr) =>
$@"{parseFunName}(#{{<<""method"">> := <<""{thr.Function.jsonKey}"">>, <<""id"">> := RpcId, <<""error"">> := #{{<<""code"">> := {thr.Id}, <<""data"">> := ExceptionJson}}}}) ->
    Exception = {JsonSerialization.ParseJson(thr.Exception.erlJsonTag(thr.Function), "ExceptionJson", erl.Name)},
    {thr.Function.erlFailTermArgs};";

            string ParseRpcReply(ServiceFunction fun) =>
$@"{parseFunName}(#{{<<""method"">> := <<""{fun.jsonKey}"">>, <<""id"">> := RpcId{fun.erlJsonMaybeResult}}}) ->
{fun.erlJsonParseResult}
    {fun.erlReplyTermArgs};
{fun.Throws.JoinLines(ParseRpcFailure)}
{parseFunName}(#{{<<""method"">> := <<""{fun.jsonKey}"">>, <<""id"">> := RpcId, <<""error"">> := #{{<<""code"">> := 255, <<""message"">> := ExceptionString}}}}) ->
    Exception = {{unknown_error, ExceptionString}},
    {fun.erlFailTermArgs}";

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
    $@"-spec {parseFunName}(igor_json:json()) -> {service.erlMessageType(direction)}.

{messages.JoinStrings(";\n", Parse)}.
");
            }
        }

        private void GenPack(ErlModule erl, ServiceForm service, Direction direction)
        {
            string packFunName = direction == Direction.ServerToClient ? "s2c_message_to_json" : "c2s_message_to_json";

            string PackCast(ServiceFunction fun) =>
$@"{packFunName}({fun.erlRequestTermArgs}) ->
{fun.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlJsonRpc.FormatRequest(fun.jsonKey, fun.Arguments).Indent(4)}";

            string PackRpcRequest(ServiceFunction fun) =>
$@"{packFunName}({fun.erlRequestTermArgs}) ->
{fun.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlJsonRpc.FormatRequest(fun.jsonKey, fun.Arguments, "RpcId").Indent(4)}";

            string PackRpcFailure(FunctionThrow thr) =>
$@"{packFunName}({{{thr.Function.erlFailFunName}, RpcId, Exception}}) when element(1, Exception) =:= {thr.Exception.erlRecordName} ->
{ErlJsonRpc.FormatFail(thr.Function.jsonKey, thr.Id, $@"<<""{thr.Exception.jsonKey}"">>", thr.Exception.erlJsonTag(thr.Function).PackJson("Exception"), "RpcId").Indent(4)};";

            string PackRpcReply(ServiceFunction fun) =>
    $@"{packFunName}({fun.erlReplyTermArgs}) ->
{fun.ReturnArguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
{ErlJsonRpc.FormatResult(fun.jsonKey, fun.ReturnArguments, "RpcId").Indent(4)};
{fun.Throws.JoinLines(PackRpcFailure)}
{packFunName}({{{fun.erlFailFunName}, RpcId, {{unknown_error, ExceptionString}}}}) ->
{ErlJsonRpc.FormatFail(fun.jsonKey, 255, "ExceptionString", null, "RpcId").Indent(4)}";

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
    $@"-spec {packFunName}({service.erlMessageType(direction)}) -> igor_json:json().

{messages.JoinStrings(";\n", Pack)}.
");
            }
        }
    }
}
