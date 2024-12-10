using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.AST
{
    public partial class FunctionArgument
    {
        public string erlName => Name.Format(Notation.LowerUnderscore);
        public string erlArgName => Name.Format(Notation.UpperCamel);
        public string erlVarName => Helper.ShadowName(Name.Format(Notation.UpperCamel));
        public string erlType => Helper.ErlType(Type, false);
        public string erlSpec => $"      {erlVarName} :: {erlType}";
    }

    public partial class ServiceFunction
    {
        public string erlName => Name.Format(Notation.LowerUnderscore);
        public string erlArgs => Arguments.JoinStrings(", ", arg => arg.erlVarName);
        public string erlRets => ReturnArguments.JoinStrings(", ", arg => arg.erlVarName);
        public string erlRetTypes => ReturnArguments.JoinStrings(", ", arg => arg.erlType);
        public string erlArgComma => Arguments.Count != 0 ? ", " : "";
        public string erlRetComma => ReturnArguments.Count != 0 ? ", " : "";

        public string erlFailFunName => $"fail_{erlName}";
        public string erlReplyFunName => $"reply_{erlName}";
        public string erlRpcSuccessFunType => $"on_{erlName}_success";
        public string erlRpcFailFunType => $"on_{erlName}_fail";
        public string erlServiceName => Service.erlName;

        public string erlRequestTermArgs => IsRpc ? $"{{{erlName}, RpcId{erlArgComma}{erlArgs}}}" : $"{{{erlName}{erlArgComma}{erlArgs}}}";
        public string erlReplyTermArgs => $"{{reply_{erlName}, RpcId{erlRetComma}{erlRets}}}";
        public string erlFailTermArgs => $"{{fail_{erlName}, RpcId, Exception}}";

        public string erlRequestType => IsRpc ? $"{{'{erlName}', igor_types:rpc_id(){erlArgComma}{Arguments.JoinStrings(", ", arg => arg.erlType)}}}" : $"{{'{erlName}'{erlArgComma}{Arguments.JoinStrings(", ", arg => arg.erlType)}}}";
        public string erlReplyType => $"{{'reply_{erlName}', igor_types:rpc_id(){erlRetComma}{erlRetTypes}}} | {{'fail_{erlName}', igor_types:rpc_id(), term()}}";

        public string erlCallbackSpec
        {
            get
            {
                var varSpecs = Arguments.Select(arg => arg.erlSpec).ToList();
                var args = Arguments.Select(arg => arg.erlVarName).ToList();
                if (IsRpc)
                {
                    varSpecs.Insert(0, "      RpcId :: igor_types:rpc_id()");
                    args.Insert(0, "RpcId");
                }
                args.Insert(0, "State");
                if (varSpecs.Count > 0)
                    return $@"-callback on_{erlName}({args.JoinStrings(", ")}) -> State when
{varSpecs.JoinStrings(",\n")}.
";
                else
                    return
    $@"-callback on_{erlName}({args.JoinStrings(", ")}) -> State.
";
            }
        }
    }

    public partial class ServiceForm
    {
        public string erlFileName => Attribute(ErlAttributes.File, erlName);
        public string erlModName => System.IO.Path.GetFileNameWithoutExtension(erlFileName);

        public string erlApiFileName(Direction direction) => direction == Direction.ServerToClient ? Attribute(ErlAttributes.ServerFile, erlFileName) : Attribute(ErlAttributes.ClientFile, erlFileName);

        public string erlApiModName(Direction direction) => System.IO.Path.GetFileNameWithoutExtension(erlApiFileName(direction));

        public bool erlClient => Attribute(CoreAttributes.Client, false);
        public bool erlServer => Attribute(CoreAttributes.Server, false);

        public string erlDispatcher(Direction direction) => direction == Direction.ServerToClient ? Attribute(ErlAttributes.ServerDispatcher, erlDefaultDispatcher) : Attribute(ErlAttributes.ClientDispatcher, erlDefaultDispatcher);

        private string erlDefaultDispatcher => Attribute(ErlAttributes.Dispatcher, null);

        public string erlMessageType(Direction direction) => direction == Direction.ServerToClient ? "s2c_message()" : "c2s_message()";

        public IEnumerable<ServiceFunction> Messages(Direction direction)
        {
            return Functions.Where(f => (f.IsRpc || f.Direction == direction));
        }
    }
}
