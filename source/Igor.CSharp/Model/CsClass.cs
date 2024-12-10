using System.Collections.Generic;

namespace Igor.CSharp.Model
{
    public enum ClassKind
    {
        Class,
        Struct,
        Interface,
    }

    public enum AccessModifier
    {
        Public,
        Protected,
        Private,
        Internal,
    }

    public class CsProperty : CsAttributeHost
    {
        public string Name { get; }
        public string Declaration { get; set; }

        public CsProperty(string name, string declaration = null)
        {
            Name = name;
            Declaration = declaration;
        }
    }

    public class CsClass : CsAttributeHost
    {
        public string Name { get; }
        public ClassKind Kind { get; set; }
        public string BaseClass { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public bool Partial { get; set; }
        public bool Sealed { get; set; }
        public bool Abstract { get; set; }
        public bool Static { get; set; }
        public IReadOnlyList<string> GenericArgs { get; set; }

        internal List<CsClass> InnerClasses { get; } = new List<CsClass>();
        internal List<string> Interfaces { get; } = new List<string>();
        internal List<CsProperty> Properties { get; } = new List<CsProperty>();
        internal List<string> Constructors { get; } = new List<string>();
        internal List<string> Methods { get; } = new List<string>();

        internal CsClass(string name)
        {
            this.Name = name;
            this.Kind = ClassKind.Class;
            this.AccessModifier = AccessModifier.Public;
        }

        public void Interface(string intf)
        {
            if (!Interfaces.Contains(intf))
                Interfaces.Add(intf);
        }

        public CsProperty Property(string name, string declaration = null)
        {
            return Properties.GetOrAdd(name, p => p.Name, () => new CsProperty(name, declaration));
        }

        public void Constructor(string ctor)
        {
            Constructors.Add(ctor);
        }

        public void Method(string method)
        {
            Methods.Add(method);
        }

        public CsClass InnerClass(string name)
        {
            return InnerClasses.GetOrAdd(name, c => c.Name, () => new CsClass(name));
        }
    }
}
