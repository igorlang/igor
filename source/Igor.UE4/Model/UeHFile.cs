using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4.Model
{
    public class UeForwardDeclaration
    {
        public string Name { get; }
        public string Namespace { get; }
        public StructType Type { get; }

        internal UeForwardDeclaration(string name, StructType structType, string @namespace = null)
        {
            Name = name;
            Namespace = @namespace;
            Type = structType;
        }
    }

    public class UeHFile : UeFile
    {
        internal readonly List<UeForwardDeclaration> ForwardDeclarations = new List<UeForwardDeclaration>();
        internal readonly List<UeNamespace> Namespaces = new List<UeNamespace>();

        internal override bool IsEmpty => Namespaces.All(n => n.IsEmpty) && DefaultNamespace.IsEmpty;

        [Obsolete("This property is not needed anymore.")]
        public bool GeneratedInclude { get; set; }
        
        public string GeneratedIncludeName => System.IO.Path.GetFileNameWithoutExtension(FileName) + ".generated.h";

        /// <summary>
        /// Represents default (empty) namespace model of the header file, containing all root declarations
        /// </summary>
        public UeNamespace DefaultNamespace { get; } = new UeNamespace(null);

        internal UeHFile(string name)
            : base(name)
        {
        }

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

        public UeForwardDeclaration ForwardDeclaration(string name, StructType structType, string @namespace = null)
        {
            return ForwardDeclarations.GetOrAdd(name, d => d.Name, () => new UeForwardDeclaration(name, structType, @namespace));
        }
    }
}
