using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.JsonSchema
{
    /// <summary>
    /// Built-in attributes available for JsonSchema target ("json_schema")
    /// </summary>
    public static class JsonSchemaAttributes
    {
        public static readonly StringAttributeDescriptor RootType = new StringAttributeDescriptor("root_type", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        /// <summary>
        /// Returns the list of all built-in JavaScript attributes
        /// </summary>
        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static JsonSchemaAttributes()
        {
            var props = typeof(JsonSchemaAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
