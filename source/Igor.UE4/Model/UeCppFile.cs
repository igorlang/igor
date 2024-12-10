using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4.Model
{
    /// <summary>
    /// UE4 cpp file model
    /// </summary>
    public class UeCppFile : UeFile
    {
        internal readonly List<UeNamespace> Namespaces = new List<UeNamespace>();

        internal override bool IsEmpty => Namespaces.All(n => n.IsEmpty) && DefaultNamespace.IsEmpty;

        internal UeCppFile(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Represents default (empty) namespace model of the cpp file, containing all root declarations
        /// </summary>
        public UeNamespace DefaultNamespace { get; } = new UeNamespace(null);

        /// <summary>
        /// Get or create a namespace with a given name.
        /// </summary>
        /// <param name="name">A name of a namespace to be found or created. Can be nested (contain ::)</param>
        /// <returns>Namespace model</returns>
        public UeNamespace Namespace(string name)
        {
            if (name == null)
                return DefaultNamespace;
            var parts = name.Split(UeName.NamespaceSeparators, 2, System.StringSplitOptions.None);
            UeNamespace result = Namespaces.GetOrAdd(parts[0], ns => ns.Name, () => new UeNamespace(parts[0]));
            if (parts.Length > 1)
                result = result.Namespace(parts[1]);
            return result;
        }
    }
}
