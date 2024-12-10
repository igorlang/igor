using System.Collections.Generic;
using System.Linq;

namespace Igor.Schema
{
    public static class SchemaAttributes
    {
        public static readonly BoolAttributeDescriptor Ignore = new BoolAttributeDescriptor("ignore", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor Root = new BoolAttributeDescriptor("root", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor RootType = new StringAttributeDescriptor("root_type", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Multiline = new BoolAttributeDescriptor("multiline", IgorAttributeTargets.Type | IgorAttributeTargets.RecordField, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor Compact = new BoolAttributeDescriptor("compact", IgorAttributeTargets.Type, AttributeInheritance.Type) { DeprecationMessage = "Use meta=(compact) instead." };
        public static readonly StringAttributeDescriptor Category = new StringAttributeDescriptor("category", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor Group = new StringAttributeDescriptor("group", IgorAttributeTargets.Any, AttributeInheritance.Inherited);
        public static readonly StringAttributeDescriptor Interface = new StringAttributeDescriptor("interface", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor Source = new StringAttributeDescriptor("source", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor EditorKey = new StringAttributeDescriptor("editor_key", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor Help = new StringAttributeDescriptor("help", IgorAttributeTargets.Any) { DeprecationMessage = "Use annotations instead. Example: # single-line help string; <# multi-line help string #>" };
        public static readonly EnumAttributeDescriptor<DescriptorKind> Editor = new EnumAttributeDescriptor<DescriptorKind>("editor", IgorAttributeTargets.Any);
        public static readonly IntAttributeDescriptor IntMin = new IntAttributeDescriptor("min", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly IntAttributeDescriptor IntMax = new IntAttributeDescriptor("max", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly FloatAttributeDescriptor FloatMin = new FloatAttributeDescriptor("min", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly FloatAttributeDescriptor FloatMax = new FloatAttributeDescriptor("max", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor PathRoot = new StringAttributeDescriptor("path.root", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor PathDefault = new StringAttributeDescriptor("path.default", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor PathExtension = new StringAttributeDescriptor("path.extension", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor PathIncludeExtension = new BoolAttributeDescriptor("path.include_extension", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor Path = new BoolAttributeDescriptor("path", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor Syntax = new StringAttributeDescriptor("syntax", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly JsonAttributeDescriptor Meta = new JsonAttributeDescriptor("meta", IgorAttributeTargets.Any, AttributeInheritance.Type);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static SchemaAttributes()
        {
            var props = typeof(SchemaAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
