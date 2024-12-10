using Igor.Text;
using System.Collections.Generic;

namespace Igor.CSharp.Model
{
    public class CsAttributeHost
    {
        internal List<string> Attributes { get; } = new List<string>();

        public string Summary { get; set; }

        public void AddAttribute(string attr)
        {
            Attributes.Add(attr);
        }

        public void AddAttribute(string attr, IReadOnlyList<(string, string)> args)
        {
            if (args.Count > 0)
                AddAttribute($"{attr}({args.JoinStrings(", ", nv => $"{nv.Item1} = {nv.Item2}")})");
            else
                AddAttribute(attr);
        }

        public void AddAttributes(IEnumerable<string> attrs)
        {
            Attributes.AddRange(attrs);
        }
    }
}
