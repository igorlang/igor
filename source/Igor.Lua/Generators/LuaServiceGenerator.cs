using Igor.Lua.AST;
using Igor.Lua.Json;
using Igor.Lua.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Lua
{
    internal class LuaServiceGenerator : ILuaGenerator
    {
        public void Generate(LuaModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.luaEnabled && serviceForm.jsonEnabled)
                {
                    if (serviceForm.luaClient)
                        GenerateService(serviceForm, model, Direction.ClientToServer);
                    if (serviceForm.luaServer)
                        GenerateService(serviceForm, model, Direction.ServerToClient);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, LuaModel model, Direction direction)
        {
            var file = model.File(serviceForm.luaFileName);

            foreach (var script in serviceForm.Module.luaRequires)
                file.Require(script);

            file.Namespace(serviceForm.luaNamespace);
            var serviceName = serviceForm.luaServiceName(direction);
            var c = file.Class(serviceName);
            c.Namespace = serviceForm.luaNamespace;
            c.Super = "igor.Service";

            foreach (var fun in serviceForm.Functions.Where(f => f.Direction == direction))
            {
                if (fun.IsRpc)
                {
                    c.Function($@"
function {serviceName}:{fun.luaName}({fun.luaArgs})
    local rpc = self:push_rpc({{{fun.Arguments.JoinStrings(", ", arg => $"{arg.luaName} = {arg.luaName}")}}})
    local params = {{{fun.Arguments.JoinStrings(", ", arg => JsonSerialization.Pack(arg.luaJsonTag, arg.luaName))}}}
    local request = {{method = '{fun.jsonKey}', params = params, id = rpc:get_id()}}
    self:send(request)
    return rpc
end
");
                }
                else
                {
                    c.Function($@"
function {serviceName}:{fun.luaName}({fun.luaArgs})
    local params = {{{fun.Arguments.JoinStrings(", ", arg => JsonSerialization.Pack(arg.luaJsonTag, arg.luaName))}}}
    local request = {{method = '{fun.jsonKey}', params = params}}
    self:send(request)
end
");
                }
            }

            var recvs = serviceForm.Functions.Where(f => f.IsRpc || f.Direction != direction);
            if (recvs.Any())
            {
                var r = new Renderer();
                r += $@"function {serviceName}:recv(message)";
                r++;
                r += "local method = message.method";
                bool isFirst = true;
                foreach (var recv in recvs)
                {
                    var m = $@"if method == ""{recv.jsonKey}"" then";
                    if (isFirst)
                        isFirst = false;
                    else
                        m = "else" + m;
                    r += m;
                    r++;
                    if (recv.Direction == direction)
                        r += $"self:_recv_reply_{recv.luaName}(message)";
                    else
                        r += $"self:_recv_{recv.luaName}(message)";
                    r--;
                }
                r +=
$@"else
    ferror(""Unknown backend method {serviceForm.luaName}:%s"", method)
end";
                r--;
                r += "end";
                c.Function(r.Build());

                foreach (var recv in recvs)
                {
                    if (recv.Direction == direction)
                    {
                        string succeed;
                        if (recv.ReturnArguments.Count == 0)
                        {
                            succeed = "rpc:succeed()";
                        }
                        else if (recv.ReturnArguments.Count == 1)
                        {
                            var arg = recv.ReturnArguments[0];
                            succeed =
$@"local {arg.luaName} = {JsonSerialization.Parse(arg.luaJsonTag, "reply.result[1]")}
rpc:succeed({arg.luaName})";
                        }
                        else
                        {
                            succeed =
$@"{recv.ReturnArguments.SelectWithIndex((arg, index) => $@"local {arg.luaName} = {JsonSerialization.Parse(arg.luaJsonTag, $"reply.result[{index + 1}]")}").JoinLines()}
local rpc_result = {{{recv.ReturnArguments.JoinStrings(", ", arg => $"{arg.luaName} = {arg.luaName}")}}}
rpc:succeed(rpc_result)";
                        }

                        c.Function($@"
function {serviceName}:_recv_reply_{recv.luaName}(reply)
    local rpc = self:pop_rpc(reply.id)
    if reply.error ~= nil then
        rpc:fail(reply.error)
    else
{succeed.Indent(8)}
    end
end
");
                    }
                    else if (recv.IsRpc)
                    {
                        c.Function($@"
function {serviceName}:_recv_{recv.luaName}(request)
    local rpc = igor.Rpc:new(request.id)
end
");
                    }
                    else if (recv.Arguments.Any())
                    {
                        c.Function($@"
function {serviceName}:_recv_{recv.luaName}(request)
    local params = request.params
{recv.Arguments.SelectWithIndex((arg, index) => $"    local {arg.luaName} = {JsonSerialization.Parse(arg.luaJsonTag, $"params[{index + 1}]")}").JoinLines()}
    self._callback:{recv.luaName}({recv.Arguments.JoinStrings(", ", arg => arg.luaName)})
end
");
                    }
                    else
                        c.Function($@"
function {serviceName}:_recv_{recv.luaName}(request)
    self._callback:{recv.luaName}()
end
");
                }
            }
        }
    }
}
