using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4.Model
{
    /// <summary>
    /// UE4 C++ namespace model
    /// </summary>
    public class UeNamespace
    {
        private class TextGroup
        {
            public object Group { get; set; }
            public string Text { get; }

            public TextGroup(string text, object group)
            {
                this.Text = text;
                this.Group = group;
            }
        }

        internal bool IsEmpty => !Enums.Any() && !Structs.Any() && !Typedefs.Any() && Namespaces.All(n => n.IsEmpty) && !Definitions.Any();

        /// <summary>
        /// Namespace name
        /// </summary>
        public string Name { get; }

        private readonly List<TextGroup> definitions = new List<TextGroup>();

        internal readonly List<UeEnum> Enums = new List<UeEnum>();
        internal readonly List<UeStruct> Structs = new List<UeStruct>();
        internal readonly List<UeTypedef> Typedefs = new List<UeTypedef>();
        internal readonly List<UeNamespace> Namespaces = new List<UeNamespace>();

        public IEnumerable<string> Definitions => definitions.GroupBy(tg => tg.Group, tg => tg.Text).SelectMany(g => g);


        /// <summary>
        /// Find or create a nested namespace with a given name.
        /// </summary>
        /// <param name="name">A name of a namespace to be found or created. Can be nested (contain ::)</param>
        /// <returns>Namespace model</returns>
        public UeNamespace Namespace(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            var parts = name.Split(UeName.NamespaceSeparators, 2, StringSplitOptions.None);
            UeNamespace result = Namespaces.GetOrAdd(parts[0], ns => ns.Name, () => new UeNamespace(parts[0]));
            if (parts.Length > 1)
                result = result.Namespace(parts[1]);
            return result;
        }

        /// <summary>
        /// Find or create an enumeration type with a given name
        /// </summary>
        /// <param name="name">A name of enum to be found or created</param>
        /// <returns>Enum model</returns>
        public UeEnum Enum(string name)
        {
            return Enums.GetOrAdd(name, e => e.Name, () => new UeEnum(name));
        }

        /// <summary>
        /// Find or create a struct or class declaration with a given name
        /// </summary>
        /// <param name="name">A name of a struct or class to be found or created</param>
        /// <returns>Struct or class model</returns>
        public UeStruct StructOrClass(string name)
        {
            return Structs.GetOrAdd(name, s => s.Name, () => new UeStruct(name));
        }

        /// <summary>
        /// Find or create a struct declaration with a given name
        /// </summary>
        /// <param name="name">A name of a struct to be found or created</param>
        /// <returns>Struct model</returns>
        public UeStruct Struct(string name)
        {
            var result = StructOrClass(name);
            result.Type = StructType.Struct;
            return result;
        }

        /// <summary>
        /// Find or create a class declaration with a given name
        /// </summary>
        /// <param name="name">A name of a class to be found or created</param>
        /// <returns>Class model</returns>
        public UeStruct Class(string name)
        {
            var result = StructOrClass(name);
            result.Type = StructType.Class;
            return result;
        }

        public UeTypedef Typedef(string name, string declaration)
        {
            var typedef = new UeTypedef(name, declaration);
            Typedefs.Add(typedef);
            return typedef;
        }

        internal UeNamespace(string name)
        {
            this.Name = name;
        }

        public void Function(string text, object group)
        {
            definitions.Add(new TextGroup(text, group));
        }

        public void Definition(string text, object group)
        {
            definitions.Add(new TextGroup(text, group));
        }

        public bool IsDefined(string name)
        {
            return Enums.Any(e => e.Name == name) || Structs.Any(s => s.Name == name) || Typedefs.Any(t => t.Name == name);
        }
    }
}
