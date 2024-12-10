using System.Collections.Generic;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// TypeScript interface model
    /// </summary>
    public class TsInterface : TsDeclaration
    {
        /// <summary>
        /// List of generic arguments (type variables)
        /// </summary>
        public IReadOnlyList<string> GenericArgs { get; set; }

        internal List<string> Interfaces { get; } = new List<string>();
        internal List<string> Properties { get; } = new List<string>();
        internal List<string> Functions { get; } = new List<string>();

        internal TsInterface(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Add implemented interface. Calling this function twice with the same interface name only adds interface once.
        /// </summary>
        /// <param name="intf">TypeScript interface name</param>
        public void Interface(string intf)
        {
            if (!Interfaces.Contains(intf))
                Interfaces.Add(intf);
        }

        /// <summary>
        /// Declare property in this interface
        /// </summary>
        /// <param name="prop">Property declaration text</param>
        public void Property(string prop)
        {
            Properties.Add(prop);
        }

        /// <summary>
        /// Declare function in this interface
        /// </summary>
        /// <param name="fun">Function declaration text</param>
        public void Function(string fun)
        {
            Functions.Add(fun);
        }
    }
}
