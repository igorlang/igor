using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Xsd
{
    public static class XsdAttributes
    {
        public static readonly StringAttributeDescriptor XsdName = new StringAttributeDescriptor("xsd.name", IgorAttributeTargets.Type);
        public static readonly EnumAttributeDescriptor<Notation> XsdNotation = new EnumAttributeDescriptor<Notation>("xsd.notation", IgorAttributeTargets.Type, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor XsdRef = new StringAttributeDescriptor("xsd.ref", IgorAttributeTargets.Type);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static XsdAttributes()
        {
            var props = typeof(XsdAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
