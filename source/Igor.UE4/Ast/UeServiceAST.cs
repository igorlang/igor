using Igor.Text;
using Igor.UE4.Model;
using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4.AST
{
    public partial class FunctionArgument
    {
        public UeType ueType => Helper.TargetType(Type);
        public string ueName => Name.Format(Notation.UpperCamel);
    }

    public partial class ServiceFunction
    {
        public string ueName => Name.Format(Notation.UpperCamel);
        public string ueRecvName => $"Recv_{ueName}";
        public string ueRpcTypeName => $"F{ueName}Rpc";
        public string ueRpcResultTypeName => $"F{ueName}Result";
        public string ueRpcResponseTypeName => $"F{ueName}Response";
        public string ueResponseType => IsRpc ? $"TSharedRef<TIgorAsyncResult<{ueRpcResponseTypeName}>>" : "void";
        public string ueErrorType => Throws.Any() ? $"TUnion<{Throws.JoinStrings(", ", t => t.Exception.ueType.RelativeName(Service.ueNamespace))}>" : "FNull";
    }

    public partial class ServiceForm
    {
        internal override string ueDefaultName => "F" + uePrefix + base.ueDefaultName;
        public bool ueClient => Attribute(CoreAttributes.Client, false);
        public bool ueServer => Attribute(CoreAttributes.Server, false);
        public string ueRelativeName(string ns) => UeName.RelativeName(ueNamespace, ueName, ns);
        public string ueQualifiedName => ueRelativeName(null);

        public string ueInterfaceName(Direction direction) => $"I{uePrefix}{base.ueDefaultName}{direction}";

        internal IEnumerable<ServiceFunction> Casts(Direction direction, FunctionType type)
        {
            return Functions.Where(f => (f.Type(direction) & type) != 0);
        }
    }
}
