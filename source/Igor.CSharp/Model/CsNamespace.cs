using System.Collections.Generic;

namespace Igor.CSharp.Model
{
    public class CsNamespace
    {
        public string Namespace { get; }
        internal List<CsEnum> Enums { get; } = new List<CsEnum>();
        internal List<CsClass> Classes { get; } = new List<CsClass>();
        internal List<CsNamespace> Namespaces { get; } = new List<CsNamespace>();

        internal CsNamespace(string @namespace)
        {
            this.Namespace = @namespace;
        }

        internal bool IsEmpty => Enums.Count == 0 && Classes.Count == 0;

        public CsEnum Enum(string name)
        {
            return Enums.GetOrAdd(name, e => e.Name, () => new CsEnum(name));
        }

        public CsClass Class(string name)
        {
            return Classes.GetOrAdd(name, c => c.Name, () => new CsClass(name));
        }

        public CsClass Interface(string name)
        {
            var @class = Class(name);
            @class.Kind = ClassKind.Interface;
            return @class;
        }

        public CsClass Struct(string name)
        {
            var @class = Class(name);
            @class.Kind = ClassKind.Struct;
            return @class;
        }

        public CsNamespace InnerNamespace(string @namespace)
        {
            return Namespaces.GetOrAdd(@namespace, n => n.Namespace, () => new CsNamespace(@namespace));
        }

        public CsClass DefineClass(string name, AccessModifier accessModifier = AccessModifier.Public, string baseClass = null, IEnumerable<string> interfaces = null)
        {
            var cl = Class(name);
            cl.AccessModifier = accessModifier;
            if (baseClass != null)
                cl.BaseClass = baseClass;
            if (interfaces != null)
                foreach (var intf in interfaces)
                    cl.Interface(intf);
            return cl;
        }
    }
}
