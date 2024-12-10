using System.Collections.Generic;
using System.Linq;

namespace Igor.Schema
{
    public enum ConnectorType1
    {
        None = 0,
        @in = 1,
        @out = 2,
        property = 3,
        asset = 4,
    }

    public class ConnectorAttribute
    {
        public string name { get; set; }
        public ConnectorType1 type { get; set; }
        public string position { get; set; }
        public string color { get; set; }
        public string caption { get; set; }
        public string category { get; set; }
    }

    public static class DiagramSchemaAttributes
    {
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Caption = new StringAttributeDescriptor("caption", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor SpecialField = new StringAttributeDescriptor("special_field", IgorAttributeTargets.RecordField, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor Archetype = new StringAttributeDescriptor("archetype", IgorAttributeTargets.Record, AttributeInheritance.Inherited);
        public static readonly StringAttributeDescriptor Icon = new StringAttributeDescriptor("icon", IgorAttributeTargets.Record, AttributeInheritance.Inherited);
        public static readonly StringAttributeDescriptor Color = new StringAttributeDescriptor("color", IgorAttributeTargets.Record, AttributeInheritance.Inherited);
        public static readonly BoolAttributeDescriptor ShowIcon = new BoolAttributeDescriptor("show_icon", IgorAttributeTargets.Type, AttributeInheritance.Inherited);
        public static readonly ObjectAttributeDescriptor<ConnectorAttribute> Connector = new ObjectAttributeDescriptor<ConnectorAttribute>("connector", IgorAttributeTargets.Any, AttributeInheritance.Inherited);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static DiagramSchemaAttributes()
        {
            var props = typeof(DiagramSchemaAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
