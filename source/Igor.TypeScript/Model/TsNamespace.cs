using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// TypeScript namespace model
    /// </summary>
    public class TsNamespace : TsDeclaration
    {
        /// <summary>
        /// Is namespace exported?
        /// </summary>
        public bool Export { get; set; } = true;

        internal TsNamespace(string name)
        {
            this.Name = name;
        }

        internal List<TsDeclaration> Declarations { get; } = new List<TsDeclaration>();
        internal List<string> Functions { get; } = new List<string>();

        /// <summary>
        /// Define function in this namespace
        /// </summary>
        /// <param name="fun">Function text</param>
        public void Function(string fun)
        {
            Functions.Add(fun);
        }

        /// <summary>
        /// Create or find existing enum declaration by name
        /// </summary>
        /// <param name="name">TypeScript enum type name</param>
        /// <returns>TypeScript enum model</returns>
        public TsEnum Enum(string name)
        {
            return AddDeclaration(name, () => new TsEnum(name));
        }

        /// <summary>
        /// Create or find existing class declaration by name
        /// </summary>
        /// <param name="name">TypeScript class type name</param>
        /// <returns>TypeScript class model</returns>
        public TsClass Class(string name)
        {
            return AddDeclaration(name, () => new TsClass(name));
        }

        /// <summary>
        /// Create or find existing interface declaration by name
        /// </summary>
        /// <param name="name">TypeScript interface type name</param>
        /// <returns>TypeScript interface model</returns>
        public TsInterface Interface(string name)
        {
            return AddDeclaration(name, () => new TsInterface(name));
        }

        /// <summary>
        /// Create or find existing namespace declaration by name
        /// </summary>
        /// <param name="name">TypeScript namespace name</param>
        /// <returns>TypeScript namespace model</returns>
        public TsNamespace Namespace(string name)
        {
            return AddDeclaration(name, () => new TsNamespace(name));
        }

        protected T AddDeclaration<T>(string name, Func<T> newItem) where T : TsDeclaration
        {
            var item = Declarations.OfType<T>().FirstOrDefault(d => d.Name == name);
            if (item == null)
            {
                item = newItem();
                var groupped = Declarations.FindLastIndex(d => d.Name == name);
                if (groupped >= 0 && groupped < Declarations.Count - 1)
                    Declarations.Insert(groupped + 1, item);
                else
                    Declarations.Add(item);
            }
            return item;
        }
    }
}
