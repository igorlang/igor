using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.TypeScript
{
    /// <summary>
    /// Built-in attributes available for TypeScript target ("ts")
    /// </summary>
    public static class TsAttributes
    {
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Namespace = new StringAttributeDescriptor("namespace", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor JsonSerializer = new StringAttributeDescriptor("json.serializer", IgorAttributeTargets.Type);
        public static readonly EnumAttributeDescriptor<Notation> FieldNotation = new EnumAttributeDescriptor<Notation>("field_notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor SetupCtor = new BoolAttributeDescriptor("setup_ctor", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Parameter = new BoolAttributeDescriptor("parameter", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Readonly = new BoolAttributeDescriptor("readonly", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Public = new BoolAttributeDescriptor("public", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Private = new BoolAttributeDescriptor("private", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor BaseUrl = new StringAttributeDescriptor("http.base_url", IgorAttributeTargets.WebService);
        public static readonly BoolAttributeDescriptor ErrorMessage = new BoolAttributeDescriptor("error_message", IgorAttributeTargets.RecordField);

        /// <summary>
        /// Returns the list of all built-in TypeScript attributes
        /// </summary>
        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static TsAttributes()
        {
            var props = typeof(TsAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
