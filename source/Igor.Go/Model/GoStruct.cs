using System.Collections.Generic;

namespace Igor.Go.Model
{
    public class GoProperty
    {
        public string Name { get; }
        public string Type { get; set; }
        public string Comment { get; set; }
        public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>();

        public GoProperty(string name)
        {
            Name = name;
        }

        public void Tag(string tag, string value)
        {
            Tags[tag] = value;
        }
    }

    public class GoStruct : GoTypeDeclaration
    {
        internal List<string> Embeds { get; } = new List<string>();

        internal GoStruct(string name) : base(name)
        {
        }

        internal List<GoProperty> Properties { get; } = new List<GoProperty>();

        public GoProperty Property(string prop)
        {
            return Properties.GetOrAdd(prop, p => p.Name, () => new GoProperty(prop));
        }

        public void Embed(string embeddedStruct)
        {
            if (!Embeds.Contains(embeddedStruct))
                Embeds.Add(embeddedStruct);
        }

        public IReadOnlyList<string> GenericArgs { get; set; }

        internal List<string> Methods { get; } = new List<string>();

        public void Method(string method)
        {
            Methods.Add(method);
        }
    }
}
