using System.Collections.Generic;

namespace Igor.Go.Model
{
    public class GoInterface : GoTypeDeclaration
    {
        internal GoInterface(string name) : base(name)
        {
        }

        internal List<string> Functions { get; } = new List<string>();

        public void Function(string fun)
        {
            Functions.Add(fun);
        }
    }
}
