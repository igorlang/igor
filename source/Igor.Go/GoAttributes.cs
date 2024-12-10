using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Go
{
    public static class GoAttributes
    {
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Package = new StringAttributeDescriptor("package", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Import = new StringAttributeDescriptor("import", IgorAttributeTargets.Module | IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor OptionalAlias = new StringAttributeDescriptor("opt_alias", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Ptr = new BoolAttributeDescriptor("ptr", IgorAttributeTargets.RecordField);

        // public static readonly StringAttributeDescriptor JsonSerializer = new StringAttributeDescriptor("json.serializer", IgorAttributeTargets.Type);
        public static readonly EnumAttributeDescriptor<Notation> FieldNotation = new EnumAttributeDescriptor<Notation>("field_notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        public static readonly BoolAttributeDescriptor EnumTypePrefix = new BoolAttributeDescriptor("enum_type_prefix", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Prefix = new StringAttributeDescriptor("prefix", IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor StringEnum = new BoolAttributeDescriptor("string_enum", IgorAttributeTargets.Enum, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Interface = new StringAttributeDescriptor("interface", IgorAttributeTargets.Variant);
        public static readonly BoolAttributeDescriptor JsonOmitEmpty = new BoolAttributeDescriptor("json.omitempty", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static GoAttributes()
        {
            var props = typeof(GoAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
