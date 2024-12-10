using System.Collections.Generic;

namespace Igor.TypeScript.Model
{
    public abstract class TsDeclarationScope
    {
    }

    /// <summary>
    /// TypeScript file model
    /// </summary>
    public class TsFile : TsDeclarationScope
    {
        /// <summary>
        /// Module name, used for import
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// File name with extension, relative to the output folder
        /// </summary>
        public string FileName { get; private set; }

        internal List<string> Imports { get; } = new List<string>();

        internal bool IsEmpty => RootNamespace.Declarations.Count == 0;

        internal TsFile(string name, string path)
        {
            this.Name = name;
            this.FileName = path;
        }

        public TsNamespace RootNamespace { get; } = new TsNamespace(null);

        /// <summary>
        /// Create or find existing namespace declaration by name
        /// </summary>
        /// <param name="name">TypeScript namespace name</param>
        /// <returns>TypeScript namespace model</returns>
        public TsNamespace Namespace(string name) => name == null ? RootNamespace : RootNamespace.Namespace(name);

        /// <summary>
        /// Define function in this namespace
        /// </summary>
        /// <param name="fun">Function text</param>
        public void Function(string fun) => RootNamespace.Function(fun);

        /// <summary>
        /// Create or find existing enum declaration by name
        /// </summary>
        /// <param name="name">TypeScript enum type name</param>
        /// <returns>TypeScript enum model</returns>
        public TsEnum Enum(string name) => RootNamespace.Enum(name);

        /// <summary>
        /// Create or find existing class declaration by name
        /// </summary>
        /// <param name="name">TypeScript class type name</param>
        /// <returns>TypeScript class model</returns>
        public TsClass Class(string name) => RootNamespace.Class(name);

        /// <summary>
        /// Create or find existing interface declaration by name
        /// </summary>
        /// <param name="name">TypeScript interface type name</param>
        /// <returns>TypeScript interface model</returns>
        public TsInterface Interface(string name) => RootNamespace.Interface(name);


        /// <summary>
        /// Add import statement.
        /// Calling it twice with the same argument will only produce one import statement.
        /// </summary>
        /// <param name="import">Full import statement string ending with simicolon</param>
        public void Import(string import)
        {
            if (!Imports.Contains(import))
                Imports.Add(import);
        }
    }
}
