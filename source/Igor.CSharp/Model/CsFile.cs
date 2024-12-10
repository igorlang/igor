using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp.Model
{
    public class CsFile
    {
        public string Name { get; }
        public string FileName { get; private set; }

        internal List<string> Usings { get; } = new List<string>();
        internal Dictionary<string, string> UsingAliases { get; } = new Dictionary<string, string>();
        internal List<CsNamespace> Namespaces { get; } = new List<CsNamespace>();
        
        internal bool IsEmpty => Namespaces.Count == 0 || Namespaces.All(n => n.IsEmpty);

        internal CsFile(string name)
        {
            this.Name = name;
            this.FileName = name;
        }

        public void Use(string ns)
        {
            if (!Usings.Contains(ns))
                Usings.Add(ns);
        }

        public void UseAlias(string ident, string alias)
        {
            UsingAliases[ident] = alias;
        }

        public CsNamespace Namespace(string @namespace)
        {
            if (string.IsNullOrEmpty(@namespace))
                @namespace = null;
            return Namespaces.GetOrAdd(@namespace, n => n.Namespace, () => new CsNamespace(@namespace));
        }
    }
}
