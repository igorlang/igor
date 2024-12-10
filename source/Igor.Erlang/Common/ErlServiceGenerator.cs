using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang
{
    internal class ErlServiceGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.erlEnabled)
                {
                    GenTypeSpecs(model, service);
                    if (service.erlClient)
                        GenService(model, service, Direction.ClientToServer);
                    if (service.erlServer)
                        GenService(model, service, Direction.ServerToClient);
                }
            }
        }

        private void GenTypeSpecs(ErlModel model, ServiceForm service)
        {
            var erl = model.Module(service.erlFileName);
            var c2s = service.Messages(Direction.ClientToServer);
            if (c2s.Any())
            {
                erl.ExportType("c2s_message", 0);
                erl.Type("c2s_message", c2s.JoinStrings(" | ", fun => fun.Direction == Direction.ClientToServer ? fun.erlRequestType : fun.erlReplyType));
            }
            var s2c = service.Messages(Direction.ServerToClient);
            if (service.Messages(Direction.ServerToClient).Any())
            {
                erl.ExportType("s2c_message", 0);
                erl.Type("s2c_message", s2c.JoinStrings(" | ", fun => fun.Direction == Direction.ServerToClient ? fun.erlRequestType : fun.erlReplyType));
            }
        }

        private void GenService(ErlModel model, ServiceForm service, Direction direction)
        {
            var dispatcher = service.erlDispatcher(direction);

            string RecvCast(ServiceFunction fun) =>
$@"recv({fun.erlRequestTermArgs}, State) ->
    Handler = {dispatcher}:handler(State, {fun.erlServiceName}),
    Handler:on_{fun.erlName}(State{fun.erlArgComma}{fun.erlArgs})";

            string RecvRpcFailure(FunctionThrow thr) =>
$@"{thr.Exception.erlVarName} when element(1, {thr.Exception.erlVarName}) =:= {thr.Exception.erlRecordName} ->
    {thr.Function.erlFailFunName}(State, RpcId, {thr.Exception.erlVarName});";

            string RecvRpcRequest(ServiceFunction fun) =>
$@"recv({fun.erlRequestTermArgs}, State) ->
    try
        Handler = {dispatcher}:handler(State, {fun.erlServiceName}),
        Handler:on_{fun.erlName}(State, RpcId{fun.erlArgComma}{fun.erlArgs})
    catch
{fun.Throws.JoinLines(RecvRpcFailure)}
        Reason ->
            ExceptionString = {dispatcher}:format_unknown_error({{throw, Reason}}),
            {fun.erlFailFunName}(State, RpcId, {{unknown_error, ExceptionString}});
        error:Reason:Stacktrace ->
            ExceptionString = {dispatcher}:format_unknown_error({{error, Reason, Stacktrace}}),
            {fun.erlFailFunName}(State, RpcId, {{unknown_error, ExceptionString}})
    end";

            string RecvRpcReply(ServiceFunction fun) =>
$@"recv({fun.erlReplyTermArgs}, State) ->
    {{{{OnSuccess, _OnFail}}, NewState}} = {dispatcher}:pop_context(State, {fun.erlServiceName}, RpcId),
    OnSuccess(NewState{fun.erlRetComma}{fun.erlRets});
recv({fun.erlFailTermArgs}, State) ->
    {{{{_OnSuccess, OnFail}}, NewState}} = {dispatcher}:pop_context(State, {fun.erlServiceName}, RpcId),
    OnFail(NewState, Exception)";

            string Recv(ServiceFunction function)
            {
                switch (function.Type(direction))
                {
                    case FunctionType.RecvCast: return RecvCast(function);
                    case FunctionType.RecvRpc: return RecvRpcRequest(function);
                    case FunctionType.SendRpc: return RecvRpcReply(function);
                    default: throw new EInternal($"No recv clause is possible for type {function.Type(direction)}");
                }
            }

            var erl = model.Module(service.erlApiFileName(direction));
            erl.IncludeLib("stdlib/include/assert.hrl");
            foreach (var function in service.Functions)
            {
                GenFunction(erl, function, direction);
            }
            if (dispatcher != null)
            {
                var recvMsgs = service.Messages(direction.Opposite());
                if (recvMsgs.Any())
                {
                    erl.Export("recv", 2);
                    var msgType = direction == Direction.ClientToServer ? "s2c_message()" : "c2s_message()";
                    erl.Function(
        $@"-spec recv(Message, State) -> State when
      Message :: {msgType}.

{recvMsgs.JoinStrings(";\n", Recv)}.
");
                }
            }
        }

        public void GenFunction(ErlModule erl, ServiceFunction function, Direction direction)
        {
            var dispatcher = function.Service.erlDispatcher(direction);

            string Cast()
            {
                var specWhen = function.Arguments.Count == 0 ? "." : $@" when
{function.Arguments.JoinStrings(",\n", arg => arg.erlSpec)}.";
                return
$@"-spec {function.erlName}(State{function.erlArgComma}{function.erlArgs}) -> State{specWhen}

{function.erlName}(State{function.erlArgComma}{function.erlArgs}) ->
    Message = {function.erlName}_message({function.erlArgs}),
    {dispatcher}:send(State, {function.erlServiceName}, Message).
";
            }

            string CastMessage()
            {
                var specWhen = function.Arguments.Count == 0 ? "." : $@" when
{function.Arguments.JoinStrings(",\n", arg => arg.erlSpec)}.";
                return
$@"-spec {function.erlName}_message({function.erlArgs}) -> {function.Service.erlMessageType(direction)}{specWhen}

{function.erlName}_message({function.erlArgs}) ->
{function.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
    {function.erlRequestTermArgs}.
";
            }

            string Rpc() =>
    $@"-spec {function.erlName}(State{function.erlArgComma}{function.erlArgs}, OnSuccess, OnFail) -> State when
{function.Arguments.JoinStrings(",\n", arg => arg.erlSpec)}{function.erlArgComma}
      OnSuccess :: {function.erlRpcSuccessFunType}(),
      OnFail :: {function.erlRpcFailFunType}().

{function.erlName}(State{function.erlArgComma}{function.erlArgs}, OnSuccess, OnFail) ->
    {{RpcId, NewState}} = {dispatcher}:push_context(State, {function.erlServiceName}, {{OnSuccess, OnFail}}),
    Message = {function.erlName}_message(RpcId{function.erlArgComma}{function.erlArgs}),
    {dispatcher}:send(NewState, {function.erlServiceName}, Message).
";

            string RpcMessage() =>
    $@"-spec {function.erlName}_message(RpcId{function.erlArgComma}{function.erlArgs}) -> {function.Service.erlMessageType(direction)} when
{function.Arguments.JoinStrings(",\n", arg => arg.erlSpec)}{function.erlArgComma}
      RpcId :: igor_types:rpc_id().

{function.erlName}_message(RpcId{function.erlArgComma}{function.erlArgs}) ->
{function.Arguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
    {function.erlRequestTermArgs}.
";

            string RpcReply() =>
    $@"-spec {function.erlReplyFunName}(State, RpcId{function.erlRetComma}{function.erlRets}) -> State when
{function.ReturnArguments.JoinStrings(",\n", arg => arg.erlSpec)}{function.erlRetComma}
      RpcId :: igor_types:rpc_id().

{function.erlReplyFunName}(State, RpcId{function.erlRetComma}{function.erlRets}) ->
    Message = {function.erlReplyFunName}_message(RpcId{function.erlRetComma}{function.erlRets}),
    {dispatcher}:send(State, {function.erlServiceName}, Message).

-spec {function.erlFailFunName}(State, RpcId, Exception) -> State when
      RpcId :: igor_types:rpc_id(),
      Exception :: term().

{function.erlFailFunName}(State, RpcId, Exception) ->
    Message = {function.erlFailFunName}_message(RpcId, Exception),
    {dispatcher}:send(State, {function.erlServiceName}, Message).
";

            string RpcReplyMessage() =>
    $@"-spec {function.erlReplyFunName}_message(RpcId{function.erlRetComma}{function.erlRets}) -> {function.Service.erlMessageType(direction)} when
{function.ReturnArguments.JoinStrings(",\n", arg => arg.erlSpec)}{function.erlRetComma}
      RpcId :: igor_types:rpc_id().

{function.erlReplyFunName}_message(RpcId{function.erlRetComma}{function.erlRets}) ->
{function.ReturnArguments.Where(arg => !(arg.Type is BuiltInType.Optional)).JoinLines(arg => $"    ?assert({arg.erlVarName} =/= undefined),")}
    {function.erlReplyTermArgs}.

-spec {function.erlFailFunName}_message(RpcId, Exception) -> {function.Service.erlMessageType(direction)} when
      RpcId :: igor_types:rpc_id(),
      Exception :: term().

{function.erlFailFunName}_message(RpcId, Exception) ->
    {function.erlFailTermArgs}.
";

            if (function.Type(direction) == FunctionType.SendCast)
            {
                erl.Export(function.erlName + "_message", function.Arguments.Count);
                erl.Function(CastMessage());

                if (dispatcher != null)
                {
                    erl.Export(function.erlName, function.Arguments.Count + 1);
                    erl.Function(Cast());
                }
            }

            if (function.Type(direction) == FunctionType.SendRpc)
            {
                erl.Export(function.erlName + "_message", function.Arguments.Count + 1);
                erl.Function(RpcMessage());

                if (dispatcher != null)
                {
                    erl.Export(function.erlName, function.Arguments.Count + 3);

                    erl.Type(function.erlRpcSuccessFunType, $"fun((State{function.erlRetComma}{function.erlRetTypes}) -> State)");
                    erl.Type(function.erlRpcFailFunType, "fun((State, tuple()) -> State)");

                    erl.Function(Rpc());
                }
            }

            if (function.Type(direction) == FunctionType.RecvCast)
            {
                if (dispatcher != null)
                {
                    erl.Callback(function.erlCallbackSpec);
                }
            }

            if (function.Type(direction) == FunctionType.RecvRpc)
            {
                erl.Export(function.erlReplyFunName + "_message", function.ReturnArguments.Count + 1);
                erl.Export(function.erlFailFunName + "_message", 2);
                erl.Function(RpcReplyMessage());

                if (dispatcher != null)
                {
                    erl.Callback(function.erlCallbackSpec);
                    erl.Export(function.erlReplyFunName, function.ReturnArguments.Count + 2);
                    erl.Export(function.erlFailFunName, 3);
                    erl.Function(RpcReply());
                }
            }
        }
    }
}
