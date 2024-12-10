using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.JavaScript
{
    /// <summary>
    /// Built-in attributes available for JavaScript target ("js")
    /// </summary>
    public static class JsAttributes
    {
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly BoolAttributeDescriptor WithToken = new BoolAttributeDescriptor("with_token", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        /// <summary>
        /// Returns the list of all built-in JavaScript attributes
        /// </summary>
        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static JsAttributes()
        {
            var props = typeof(JsAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
