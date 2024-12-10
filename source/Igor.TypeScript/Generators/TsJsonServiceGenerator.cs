using Igor.Text;
using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using System.Linq;

namespace Igor.TypeScript.Json
{
    internal class TsJsonServiceGenerator : ITsGenerator
    {
        public void Generate(TsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.tsEnabled && serviceForm.tsJsonClient)
                {
                    GenerateService(serviceForm, model, Direction.ClientToServer);
                }
                if (serviceForm.tsEnabled && serviceForm.tsJsonServer)
                {
                    GenerateService(serviceForm, model, Direction.ServerToClient);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, TsModel model, Direction direction)
        {
            var file = model.File(serviceForm.Module.tsName, serviceForm.Module.tsFileName);
            var service = file.Class(serviceForm.tsClassName(direction));
            service.BaseClass = "Igor.Json.Service";
            service.Interface(serviceForm.tsInterfaceName(direction));
            service.Constructor(
@"
constructor(sender: Igor.Json.ISender) {
    super(sender);
}
");

            foreach (var func in serviceForm.Functions)
            {
                switch (func.Type(direction))
                {
                    case FunctionType.SendCast:
                        service.Function(SendCast(func));
                        break;
                    case FunctionType.SendRpc:
                        service.Function(SendRpc(func));
                        service.Function(RecvRpcResult(func));
                        break;
                    case FunctionType.RecvCast:
                        service.Function(RecvCast(func));
                        break;
                    case FunctionType.RecvRpc:
                        service.Function(RecvRpc(func));
                        break;
                }
            }

            if (serviceForm.Functions.Any(f => f.Direction != direction || f.IsRpc))
            {
                string RecvCase(ServiceFunction func)
                {
                    return
    $@"        case '{func.jsonKey}':
            this.{func.tsRecvFun}(json);
            break;";
                }

                service.Interface("Igor.Json.IReceiver");
                service.Property($"callback?: {serviceForm.tsInterfaceName(direction.Opposite())};");
                service.Function($@"
recv(json: any): void {{
    switch (json.method) {{
{serviceForm.Functions.Where(f => f.Direction != direction || f.IsRpc).JoinLines(RecvCase)}
    }}
}}
");
            }
        }

        private string SendCast(ServiceFunction func)
        {
            if (func.Arguments.Count > 0)
            {
                return $@"
{func.tsName}({func.tsTypedArgs}): void {{
    const params: Igor.Json.JsonArray = {TsCodeUtils.MakeArray(func.Arguments.Select(WriteArg))};
    this.sender.send({{method: '{func.jsonKey}', params: params}});
}}
";
            }
            else
            {
                return $@"
{func.tsName}(): void {{
    this.sender.send({{method: '{func.jsonKey}'}});
}}
";
            }
        }

        private string SendRpc(ServiceFunction func)
        {
            return $@"
{func.tsName}({func.tsTypedArgs}): {func.tsResultTypeName} {{
    const params: Igor.Json.JsonArray = {TsCodeUtils.MakeArray(func.Arguments.Select(WriteArg))};
    const rpc = this._createRpc('{func.jsonKey}');
    this.sender.send({{method: '{func.jsonKey}', params: params, id: rpc.id}});
    return rpc.promise();
}}";
        }

        private string RecvRpcResult(ServiceFunction func)
        {
            var r = new Renderer();
            r += $"private {func.tsRecvFun}(json: any): void {{";
            r++;
            r += $"const rpc = this._findRpc(json.id, '{func.jsonKey}');";
            r += "if (json.error) {";
            r++;
            if (func.Throws.Any())
            {
                r += "switch (json.error.code) {";
                r++;
                foreach (var thr in func.Throws)
                {
                    r += $"case {thr.Id}: {{";
                    r++;
                    r += $"const error = {thr.tsType.fromJson("json.error.data", func.Service.tsNamespace)};";
                    r += "rpc.fail(error);";
                    r += "break;";
                    r--;
                    r += "}";
                }
                r += @"default: {
    rpc.fail(new Error(json.error.message));
    break;
}";
                r--;
                r += "}";
            }
            else
            {
                r += "rpc.fail(new Error(json.error.message));";
            }
            r--;
            r += "} else {";
            r++;
            r.ForEach(func.ReturnArguments, retArg =>
            {
                r += $"const {retArg.tsName} = {retArg.tsType.fromJson($"json.result[{retArg.Index}]", retArg.Function.Service.tsNamespace)};";
            });
            r += $@"const rpcResult = {{{func.ReturnArguments.JoinStrings(", ", ret => $"{ret.tsName}: {ret.tsName}")}}};";
            r += "rpc.complete(rpcResult);";
            r--;
            r += "}";
            r--;
            r += "}";
            return r.Build();
        }

        private string RecvCast(ServiceFunction func)
        {
            return $@"
private {func.tsRecvFun}(json: any): void {{
{func.Arguments.Select(ReadArg).JoinLines().Indent(4)}
    if (this.callback)
        this.callback.{func.tsName}({func.tsArgs});
}}
";
        }

        private string RecvRpc(ServiceFunction func)
        {
            var r = new Renderer();
            r += $@"private {func.tsRecvFun}(json: any): void {{";
            r++;
            r.Blocks(func.Arguments.Select(ReadArg));
            r += "if (this.callback) {";
            r++;
            r += "const rpcId = json.id;";
            r += $"const promise = this.callback.{func.tsName}({func.tsArgs});";
            r += "promise.then((result) => {";
            r++;
            string jsonResult = "[]";
            if (func.ReturnArguments.Any())
            {
                r.Block($"const resultJson = {TsCodeUtils.MakeArray(func.Arguments.Select(WriteArg))};");
                jsonResult = "resultJson";
            }
            r += $"this.sender.send({{method: '{func.jsonKey}', result: {jsonResult}, id: rpcId}});";
            r--;
            r += "});";
            r += "promise.catch((err) => {";
            r++;
            if (func.Throws.Any())
            {
                r += "let errorJson: any;";
                bool bIsFirst = true;
                foreach (var thr in func.Throws)
                {
                    if (bIsFirst)
                    {
                        bIsFirst = false;
                        r += $"if (err instanceof {thr.tsTypeName}) {{";
                    }
                    else
                    {
                        r += $"}} else if (err instanceof {thr.tsTypeName}) {{";
                    }
                    r++;
                    r += $"const errorData = {thr.tsType.toJson("err", func.Service.tsNamespace)};";
                    r += $"errorJson = {{code: {thr.Id}, data: errorData, message: err.message}};";
                    r--;
                }
                r += @"} else {
    errorJson = {code: 255, message: err.message};
};";
            }
            else
            {
                r += "const errorJson = {code: 255, message: err.message};";
            }
            r += $"this.sender.send({{method: '{func.jsonKey}', id: rpcId, error: errorJson}});";
            r--;
            r += "});";
            r--;
            r += "}";
            r--;
            r += "}";
            return r.Build();
        }

        private string WriteArg(FunctionArgument arg)
        {
            return arg.tsType.toJson(arg.tsName, arg.Function.Service.tsNamespace);
        }

        private string ReadArg(FunctionArgument arg)
        {
            return $"const {arg.tsName} = {arg.tsType.fromJson($"json.params[{arg.Index}]", arg.Function.Service.tsNamespace)};";
        }
    }
}
