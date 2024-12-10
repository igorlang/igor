using System.Collections.Generic;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// TypeScript class model
    /// </summary>
    public class TsClass : TsDeclaration
    {
        public string BaseClass { get; set; }

        /// <summary>
        /// Is class exported?
        /// </summary>
        public bool Export { get; set; } = true;

        /// <summary>
        /// Is class abstract?
        /// </summary>
        public bool Abstract { get; set; }

        /// <summary>
        /// List of generic arguments (type variables)
        /// </summary>
        public IReadOnlyList<string> GenericArgs { get; set; }

        internal List<string> Decorators { get; } = new List<string>();
        internal List<string> Interfaces { get; } = new List<string>();
        internal List<string> Properties { get; } = new List<string>();
        internal List<string> Constructors { get; } = new List<string>();
        internal List<string> Functions { get; } = new List<string>();

        internal TsClass(string name)
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
        /// Define property in this class
        /// </summary>
        /// <param name="prop">Property definition text</param>
        public void Property(string prop)
        {
            if (!Properties.Contains(prop))
                Properties.Add(prop);
        }

        /// <summary>
        /// Define constructor in this class
        /// </summary>
        /// <param name="ctor">Constructor definition text</param>
        public void Constructor(string ctor)
        {
            if (!Constructors.Contains(ctor))
                Constructors.Add(ctor);
        }

        /// <summary>
        /// Define function in this class
        /// </summary>
        /// <param name="fun">Function definition text</param>
        public void Function(string fun)
        {
            Functions.Add(fun);
        }

        /// <summary>
        /// Add decorator
        /// </summary>
        /// <param name="decorator">Decorator text</param>
        public void Decorator(string decorator)
        {
            if (!Decorators.Contains(decorator))
                Decorators.Add(decorator);
        }
    }
}
