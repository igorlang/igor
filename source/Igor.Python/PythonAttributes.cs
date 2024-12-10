using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Python
{
    public static class PythonAttributes
    {
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Import = new StringAttributeDescriptor("import", IgorAttributeTargets.Module | IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor JsonSerializer = new StringAttributeDescriptor("json.serializer", IgorAttributeTargets.Type);
        public static readonly EnumAttributeDescriptor<Notation> FieldNotation = new EnumAttributeDescriptor<Notation>("field_notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly IntAttributeDescriptor Index = new IntAttributeDescriptor("index", IgorAttributeTargets.RecordField);
        public static readonly StringAttributeDescriptor EnumAlias = new StringAttributeDescriptor("enum_alias", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Classes = new BoolAttributeDescriptor("classes", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static PythonAttributes()
        {
            var props = typeof(PythonAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
