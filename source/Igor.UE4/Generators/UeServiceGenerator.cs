using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4
{
    internal class UeServiceGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            foreach (var service in mod.Services)
            {
                if (service.ueEnabled)
                {
                    GenerateService(model, service, Direction.ClientToServer);
                    GenerateService(model, service, Direction.ServerToClient);
                }
            }
        }

        public void GenerateService(UeModel model, ServiceForm s, Direction direction)
        {
            var h = model.HFile(s.Module.ueHFile);
            h.Include($"{s.ueIgorPath}IgorService.h");

            var name = s.ueInterfaceName(direction);
            var intf = h.Namespace(s.ueNamespace).Class(name);

            foreach (var func in s.Casts(direction, FunctionType.SendRpc))
            {
                var rpcType = intf.Struct(func.ueRpcResultTypeName);

                foreach (var ret in func.ReturnArguments)
                {
                    rpcType.Field(ret.ueName, ret.ueType.RelativeName(s.ueNamespace), AccessModifier.Public);
                }

                intf.Typedef(func.ueRpcResponseTypeName, $"TIgorRpcResponse<{func.ueRpcResultTypeName}, {func.ueErrorType}>", AccessModifier.Public);
            }

            foreach (var func in s.Functions.Where(f => f.Direction == direction))
            {
                intf.Function($"virtual {func.ueResponseType} {func.ueName}({func.Arguments.JoinStrings(", ", arg => $"const {arg.ueType.RelativeName(s.ueNamespace)}& {arg.ueName}")}) = 0;", AccessModifier.Public);
                foreach (var arg in func.Arguments)
                {
                    foreach (var incl in arg.ueType.HIncludes)
                        if (incl != h.FileName)
                            h.Include(incl);
                }
            }
            intf.Function($"virtual ~{name}() {{ }}", AccessModifier.Public);
        }
    }
}
