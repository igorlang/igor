using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using System.Linq;

namespace Igor.TypeScript
{
    internal class TsServiceGenerator : ITsGenerator
    {
        public void Generate(TsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.tsEnabled)
                {
                    GenerateService(serviceForm, model);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, TsModel model)
        {
            var file = model.File(serviceForm.Module.tsName, serviceForm.Module.tsFileName);

            foreach (var function in serviceForm.Functions)
            {
                foreach (var arg in function.Arguments)
                {
                    file.ImportType(arg.tsType);
                }
                foreach (var arg in function.ReturnArguments)
                {
                    file.ImportType(arg.tsType);
                }
                foreach (var @throw in function.Throws)
                {
                    file.ImportType(@throw.tsType);
                }
            }

            string InterfaceFunction(ServiceFunction func)
            {
                return $"{func.tsName}({func.tsTypedArgs}): {func.tsResultTypeName};";
            }

            foreach (var direction in Directions.Values)
            {
                var intf = file.Interface(serviceForm.tsInterfaceName(direction));
                foreach (var cast in serviceForm.Functions.Where(f => f.Direction == direction))
                    intf.Function(InterfaceFunction(cast));
            }

            var rpcTypes = serviceForm.Functions.Where(f => f.IsRpc && f.ReturnArguments.Any()).ToList();
            if (rpcTypes.Any())
            {
                var rpcNs = file.Namespace(serviceForm.tsName);
                foreach (var rpc in rpcTypes)
                {
                    var rpcClass = rpcNs.Interface(rpc.tsRpcResultTypeName);
                    foreach (var arg in rpc.ReturnArguments)
                    {
                        var maybeOpt = arg.tsType.isOptional ? "?" : "";
                        rpcClass.Property($"{arg.tsName}{maybeOpt}: {arg.tsTypeName};");
                    }
                }
            }
        }
    }
}
