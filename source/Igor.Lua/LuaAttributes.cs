using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Lua
{
    public enum EnumStyle
    {
        Table,
        Enum,
    }

    public enum RecordStyle
    {
        Data,
        Class,
    }

    public static class LuaAttributes
    {
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor File = new StringAttributeDescriptor("file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Require = new StringAttributeDescriptor("require", IgorAttributeTargets.Module | IgorAttributeTargets.Service);
        public static readonly StringAttributeDescriptor JsonSerializer = new StringAttributeDescriptor("json.serializer", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor VariantSerializerLookup = new BoolAttributeDescriptor("variant_serializer_lookup", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Namespace = new StringAttributeDescriptor("namespace", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> FieldNotation = new EnumAttributeDescriptor<Notation>("field_notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly IntAttributeDescriptor Index = new IntAttributeDescriptor("index", IgorAttributeTargets.RecordField);
        public static readonly EnumAttributeDescriptor<EnumStyle> EnumStyle = new EnumAttributeDescriptor<EnumStyle>("enum_style", IgorAttributeTargets.Enum, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<RecordStyle> RecordStyle = new EnumAttributeDescriptor<RecordStyle>("record_style", IgorAttributeTargets.Record, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor EnumAlias = new StringAttributeDescriptor("enum_alias", IgorAttributeTargets.Type);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static LuaAttributes()
        {
            var props = typeof(LuaAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
