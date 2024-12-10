using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Igor.CSharp
{
    internal class CsServiceMessagesGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.csEnabled && serviceForm.csMessages)
                {
                    var file = model.FileOf(serviceForm);
                    GenerateMessages(file, serviceForm, Direction.ClientToServer);
                    GenerateMessages(file, serviceForm, Direction.ServerToClient);
                    if (serviceForm.binaryEnabled)
                    {
                        GenerateBinarySerializer(file, serviceForm);
                    }
                    if (serviceForm.jsonEnabled)
                    {
                        GenerateJsonSerializer(file, serviceForm);
                    }
                }
            }
        }

        private void GenerateMessages(CsFile file, ServiceForm serviceForm, Direction direction)
        {
            var messageClassName = direction == Direction.ClientToServer ? "ClientMessage" : "ServerMessage";
            var ns = file.Namespace(serviceForm.csNamespace);
            var serviceClass = ns.Class(serviceForm.csName);
            serviceClass.Static = true;
            var root = serviceClass.InnerClass(messageClassName);
            root.Abstract = true;
            foreach (var func in serviceForm.Functions)
            {
                if (func.Direction == direction)
                {
                    var msg = serviceClass.InnerClass(func.csName);
                    msg.BaseClass = messageClassName;
                    ArgumentProperties(msg, func.Arguments);
                    if (func.IsRpc)
                        msg.Property("RpcId", "public int RpcId { get; set; }");
                }
                else if (func.IsRpc)
                {
                    var replyMsg = serviceClass.InnerClass(func.csName + "Response");
                    replyMsg.BaseClass = messageClassName;
                    replyMsg.Property("RpcId", "public int RpcId { get; set; }");
                    if (func.ReturnArguments.Any())
                        replyMsg.Property("Result", $"public {func.csRpcResultClassName} Result {{ get; set; }}");
                    var nullable = CsVersion.NullableReferenceTypes ? "?" : "";
                    replyMsg.Property("Exception", $"public System.Exception{nullable} Exception {{ get; set; }}");
                    replyMsg.ReadOnlyProperty("IsSuccess", "public bool IsSuccess", "Exception == null");
                }
            }
        }

        private void GenerateBinarySerializer(CsFile file, ServiceForm serviceForm)
        {
            var ns = file.Namespace(serviceForm.csBinaryNamespace);
            var serviceClass = ns.Class(serviceForm.csName + "BinarySerializer");
            serviceClass.Static = true;

            foreach (var func in serviceForm.Functions.Where(f => f.IsRpc && f.ReturnArguments.Any()))
            {
                var resultMsg = serviceClass.InnerClass(func.csRpcResultClassName);
                var resultTarget = func.csRpcQualifiedResultClassName;
                var resultArgs = new List<(string, CsType)>(func.ReturnArguments.Select(arg => (arg.csName, arg.csType)));
                GenerateBinarySerializer(resultMsg, resultTarget, resultArgs, serviceForm.csBinaryNamespace);
            }

            foreach (var direction in Directions.Values)
                GenerateBinarySerialization(serviceClass, serviceForm, direction);
        }

        private void GenerateBinarySerialization(CsClass serviceClass, ServiceForm serviceForm, Direction direction)
        {
            var messageClassName = direction == Direction.ClientToServer ? "ClientMessage" : "ServerMessage";
            var rootTarget = $"{serviceForm.csName}.{messageClassName}";
            var root = serviceClass.InnerClass(messageClassName);
            root.Sealed = true;
            root.Interface($"Igor.Serialization.IBinarySerializer<{rootTarget}>");

            foreach (var func in serviceForm.Functions)
            {
                if (func.Direction == direction)
                {
                    var msg = serviceClass.InnerClass(func.csName);
                    var target = CsName.Combine(serviceForm.csName, func.csName);
                    var args = new List<(string, CsType)>();
                    if (func.IsRpc)
                        args.Add(("RpcId", new CsIntegerType(IntegerType.Int)));
                    args.AddRange(func.Arguments.Select(arg => (arg.csName, arg.csType)));
                    GenerateBinarySerializer(msg, target, args, serviceForm.csBinaryNamespace);
                }
                else if (func.IsRpc)
                {
                    var replyName = func.csName + "Response";
                    var msg = serviceClass.InnerClass(replyName);
                    var target = CsName.Combine(serviceForm.csName, replyName);
                    GenerateRpcReplyBinarySerializer(msg, func, target, serviceForm.csBinaryNamespace);
                }
            }

            string RootSendCase(ServiceFunction function)
            {
                var innerSer = function.Direction == direction ? function.csName : function.csName + "Response";
                return $@"    if (value is {function.Service.csName}.{innerSer})
    {{
        writer.Write((byte){function.Index});
        {innerSer}.Instance.Serialize(writer, ({function.Service.csName}.{innerSer})value);
        return;
    }}";
            }


            root.Method($@"public void Serialize(BinaryWriter writer, {rootTarget} value)
{{
{serviceForm.Functions.Where(f => f.Direction == direction || f.IsRpc).JoinLines(RootSendCase)}
    throw new System.ArgumentException($""Unknown message type: {{value.GetType()}}"");
}}");

            string RootRecvCase(ServiceFunction function)
            {
                var innerSer = function.Direction == direction ? function.csName : function.csName + "Response";
                return $@"        case {function.Index}: return {innerSer}.Instance.Deserialize(reader);";
            }

            root.Method($@"public {rootTarget} Deserialize(BinaryReader reader)
{{
    byte msgId = reader.ReadByte();
    switch (msgId)
    {{
{serviceForm.Functions.Where(f => f.Direction == direction || f.IsRpc).JoinLines(RootRecvCase)}
        default: throw new Igor.Services.Binary.UnknownServiceFunctionException(msgId);
    }}
}}");

            root.Property("Instance", $"public static readonly {root.Name} Instance = new {root.Name}();");
        }

        private void ArgumentProperty(CsClass c, FunctionArgument arg)
        {
            c.Property(arg.csName, $"public {arg.csTypeName} {arg.csName} {{ get; set; }}");
        }

        private void ArgumentProperties(CsClass c, IEnumerable<FunctionArgument> args)
        {
            foreach (var arg in args)
                ArgumentProperty(c, arg);
        }

        private void GenerateBinarySerializer(CsClass c, string target, IEnumerable<(string, CsType)> args, string ns)
        {
            string WriteArg(string name, CsType csType)
            {
                var igorSerializer = csType.binarySerializer(ns);
                if (csType.csNotNullRequired)
                    return
                        $@"    if (value.{name} != null) {igorSerializer}.Serialize(writer, value.{name});
    else throw new System.ArgumentException(""Required property {name} is null"");";
                else
                    return
                        $@"    {igorSerializer}.Serialize(writer, value.{name});";
            }


            c.Sealed = true;
            c.Interface($"Igor.Serialization.IBinarySerializer<{target}>");
            c.Method($@"public void Serialize(BinaryWriter writer, {target} value)
{{
{args.JoinLines(nt => WriteArg(nt.Item1, nt.Item2))}
}}");

            c.Method($@"public {target} Deserialize(BinaryReader reader)
{{
    var result = new {target}();
    Deserialize(reader, result);
    return result;
}}");

            string ReadArg(string name, CsType csType)
            {
                return $"    value.{name} = {csType.binarySerializer(ns)}.Deserialize(reader);";
            }

            c.Method($@"public void Deserialize(BinaryReader reader, {target} value)
{{
{args.JoinLines(nt => ReadArg(nt.Item1, nt.Item2))}
}}");

            c.Property("Instance", $"public static readonly {c.Name} Instance = new {c.Name}();");
        }

        private void GenerateRpcReplyBinarySerializer(CsClass c, ServiceFunction rpc, string target, string ns)
        {
            c.Sealed = true;
            c.Interface($"Igor.Serialization.IBinarySerializer<{target}>");

            var r = new Renderer();
            r += $"public void Serialize(BinaryWriter writer, {target} value)";
            r += "{";
            r++;
            r += "writer.Write(value.RpcId);";
            r += "if (value.IsSuccess)";
            r += "{";
            r++;
            r += "writer.Write((byte)0);";
            if (rpc.ReturnArguments.Any())
                r += $"{rpc.csRpcResultClassName}.Instance.Serialize(writer, value.Result);";
            r--;
            r += "}";
            r += @"else
{
    writer.Write(byte.MaxValue);
    writer.Write(value.Exception.Message);
}";
            r--;
            r += "}";
            c.Method(r.Build());

            c.Method($@"public {target} Deserialize(BinaryReader reader)
{{
    var result = new {target}();
    Deserialize(reader, result);
    return result;
}}");
            r.Reset();
            r += $@"public void Deserialize(BinaryReader reader, {target} value)";
            r += "{";
            r++;
            r += @"value.RpcId = reader.ReadUInt16();";
            r += "switch (reader.ReadByte())";
            r += "{";
            r++;
            r += "case 0:";
            r++;
            if (rpc.ReturnArguments.Any())
                r += $"value.Result = {rpc.csRpcResultClassName}.Instance.Deserialize(reader);";
            r += "break;";
            r--;
            r += @"case byte.MaxValue:
    value.Exception = new Igor.Services.UnknownRemoteException(reader.ReadString());
    break;";
            r--;
            r += "}";
            r--;
            r += "}";
            c.Method(r.Build());

            c.Property("Instance", $"public static readonly {c.Name} Instance = new {c.Name}();");
        }

        private void GenerateJsonSerializer(CsFile file, ServiceForm serviceForm)
        {
            var ns = file.Namespace(serviceForm.csJsonNamespace);
            var serviceClass = ns.Class(serviceForm.csName + "JsonSerializer");
            serviceClass.Static = true;

            foreach (var direction in Directions.Values)
                GenerateJsonSerialization(serviceClass, serviceForm, direction);
        }

        private void GenerateJsonSerialization(CsClass serviceClass, ServiceForm serviceForm, Direction direction)
        {
            var messageClassName = direction == Direction.ClientToServer ? "ClientMessage" : "ServerMessage";
            var rootTarget = $"{serviceForm.csName}.{messageClassName}";
            var root = serviceClass.InnerClass(messageClassName);
            root.Sealed = true;
            root.Interface($"Json.Serialization.IJsonSerializer<{rootTarget}>");

            foreach (var func in serviceForm.Functions)
            {
                if (func.Direction == direction)
                {
                    var msg = serviceClass.InnerClass(func.csName);
                    var target = CsName.Combine(serviceForm.csName, func.csName);
                    GenerateJsonFuncSerializer(msg, target, func, serviceForm.csJsonNamespace, false);
                }
                else if (func.IsRpc)
                {
                    var replyName = func.csName + "Response";
                    var msg = serviceClass.InnerClass(replyName);
                    var target = CsName.Combine(serviceForm.csName, replyName);
                    GenerateJsonFuncSerializer(msg, target, func,serviceForm.csJsonNamespace, true);
                }
            }

            string SendCase(ServiceFunction func)
            {
                var target = CsName.Combine(serviceForm.csName, func.csName);
                var varName = func.csName.Format(Notation.LowerCamel);

                switch (func.Type(direction))
                {
                    case FunctionType.SendCast:
                    case FunctionType.SendRpc:
                        return $@"if (value is {target} {varName})
    return {func.csName}.Instance.Serialize({varName});";
                    case FunctionType.RecvRpc:
                        return $@"if (value is {target}Response {varName}Response)
    return {func.csName}Response.Instance.Serialize({varName}Response);";
                    default:
                        return "";
                }
            }

            root.Method($@"public Json.ImmutableJson Serialize({rootTarget} value)
{{
{serviceForm.Functions.Where(f => f.Direction == direction || f.IsRpc).JoinLines(SendCase).Indent(4)}
    throw new System.ArgumentException($""Unknown message type: {{value.GetType()}}"");
}}");

            string RecvCase(ServiceFunction func)
            {
                var name = func.csName;
                if (func.Type(direction) == FunctionType.RecvRpc)
                    name += "Response";
                return
                    $@"        case ""{func.jsonKey}"":
            return {name}.Instance.Deserialize(jsonObject);";
            }

            root.Method($@"public {rootTarget} Deserialize(Json.ImmutableJson json)
{{
    var jsonObject = json.AsObject;
    string method = jsonObject[""method""].AsString;
    switch (method)
    {{
{serviceForm.Functions.Where(f => f.Direction != direction || f.IsRpc).JoinLines(RecvCase)}
        default:
            throw new System.MissingMethodException(""{serviceForm.Name}"", method);
    }}
}}");

            root.Property("Instance", $"public static readonly {root.Name} Instance = new {root.Name}();");
        }

        private void GenerateJsonFuncSerializer(CsClass c, string target, ServiceFunction func, string ns, bool isResponse)
        {
            c.Sealed = true;
            c.Interface($"Json.Serialization.IJsonSerializer<{target}>");

            var message = new Dictionary<string, string>
            {
                ["method"] = func.jsonKey.Quoted(),
            };

            if (func.IsRpc)
                message.Add("id", "value.RpcId");

            string WriteArg(FunctionArgument arg, string source) =>
                $@"{arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Serialize({source})";

            if (!isResponse && func.Arguments.Any())
                message["params"] = $"new Json.JsonArray{CsSyntaxUtils.CollectionInitializer(func.Arguments.Select(argument => WriteArg(argument, "value." + argument.csName)), "()", 4)}";

            if (isResponse && func.ReturnArguments.Any())
                message["result"] = $"new Json.JsonArray{CsSyntaxUtils.CollectionInitializer(func.ReturnArguments.Select(argument => WriteArg(argument, "value.Result." + argument.csName)), "()", 4)}";

            c.Method($@"public Json.ImmutableJson Serialize({target} value)
{{
    return new Json.JsonObject{CsSyntaxUtils.IndexInitializer(message, "()", 4, true)};
}}");

            var r = new Renderer();
            r += $@"public {target} Deserialize(Json.ImmutableJson json)";
            r += "{";
            r++;
            r += $"{target} value = new {target}();";
            if (func.IsRpc)
            {
                r += @"value.RpcId = json[""id""].AsInt;";
            }
            if (!isResponse && func.Arguments.Any())
            {
                r += @"var paramsArray = json[""params""].AsArray;";
                foreach (var arg in func.Arguments)
                {
                    r += $"value.{arg.csName} = {arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Deserialize(paramsArray[{arg.Index}]);";
                }
            }

            if (isResponse)
            {
                r += @"if (json.AsObject.ContainsKey(""error""))";
                r += "{";
                r++;
                r += @"var errorJson = json[""error""].AsObject;";
                if (func.Throws.Any())
                {
                    r += @"var code = errorJson[""code""].AsInt;";
                    r += "switch (code)";
                    r += "{";
                    r++;
                    foreach (var ex in func.Throws)
                    {
                        r += $"case {ex.Id}:";
                        r++;
                        r += $@"value.Exception = {ex.Exception.csJsonSerializerInstance(ex.Function.Service.csNamespace)}.Deserialize(errorJson[""data""]);";
                        r += "break;";
                        r--;
                    }

                    r += "default:";
                    r++;
                    r += @"var errorMessage = errorJson[""message""].AsString;";
                    r += "value.Exception = new Igor.Services.UnknownRemoteException(errorMessage);";
                    r += "break;";
                    r--;
                    r--;
                    r += "}";
                }
                else
                {
                    r += @"var errorMessage = errorJson[""message""].AsString;";
                    r += "value.Exception = new Igor.Services.UnknownRemoteException(errorMessage);";
                }

                r--;
                r += "}";
                r += "else";
                r += "{";
                r++;
                if (func.ReturnArguments.Any())
                {
                    var resultTarget = func.csRpcQualifiedResultClassName;
                    r += @"var resultArray = json[""result""].AsArray;";
                    r += $@"value.Result = new {resultTarget}();";
                    foreach (var arg in func.ReturnArguments)
                    {
                        r += $"value.Result.{arg.csName} = {arg.csType.jsonSerializer(arg.Function.Service.csNamespace)}.Deserialize(resultArray[{arg.Index}]);";
                    }
                }

                r--;
                r += "}";
            }

            r += "return value;";
            r--;
            r += "}";
            c.Method(r.Build());

            c.Property("Instance", $"public static readonly {c.Name} Instance = new {c.Name}();");
        }
    }
}
