using Igor.CSharp.AST;
using Igor.CSharp.Model;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsServiceGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var serviceForm in mod.Services)
            {
                if (serviceForm.csEnabled)
                {
                    GenerateService(serviceForm, model);
                }
            }
        }

        private void GenerateService(ServiceForm serviceForm, CsModel model)
        {
            var file = model.File(serviceForm.Module.csFileName);
            var ns = file.Namespace(serviceForm.csNamespace);
            file.Use("System.IO");
            CsVersion.UsingTasks(file);

            string csInterface(ServiceFunction func)
            {
                if (func.IsRpc)
                    return $"{func.csRpcTaskClass} {func.csName}({func.csTypedArgs});";
                else
                    return $"void {func.csName}({func.csTypedArgs});";
            }

            foreach (var direction in Directions.Values)
            {
                var intf = ns.Interface(serviceForm.csInterfaceName(direction));
                intf.Kind = ClassKind.Interface;
                foreach (var cast in serviceForm.Functions.Where(f => f.Direction == direction))
                    intf.Method(csInterface(cast));
            }

            var rpcs = serviceForm.Functions.Where(f => f.IsRpc && f.ReturnArguments.Any());
            if (rpcs.Any())
            {
                var c = ns.Class(serviceForm.csName);
                c.Static = true;
                foreach (var rpc in rpcs)
                {
                    var rpcClass = c.InnerClass(rpc.csRpcResultClassName);
                    var features = RecordFeatures.Reference;
                    if (serviceForm.csGenerateEquals)
                        features |= RecordFeatures.EqualsAndGetHashCode;
                    features |= RecordFeatures.DefaultCtor;
                    features |= RecordFeatures.SetupCtor;
                    var props = rpc.ReturnArguments.Select(arg => arg.csProperty).ToList();
                    rpcClass.Record(props, serviceForm.csNamespace, features);
                }
            }
        }
    }
}
