using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp
{
    public enum ListTypeImplementation
    {
        List,
        ReadOnly,
    }

    public enum DictTypeImplementation
    {
        Dictionary,
        ReadOnly,
    }

    public static class CsAttributes
    {
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly EnumAttributeDescriptor<Notation> FieldNotation = new EnumAttributeDescriptor<Notation>("field_notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Sealed = new BoolAttributeDescriptor("sealed", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Partial = new BoolAttributeDescriptor("partial", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Class = new BoolAttributeDescriptor("class", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor Struct = new BoolAttributeDescriptor("struct", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor SetupCtor = new BoolAttributeDescriptor("setup_ctor", IgorAttributeTargets.Type, AttributeInheritance.Inherited);
        public static readonly BoolAttributeDescriptor SetupCtorIgnore = new BoolAttributeDescriptor("setup_ctor.ignore", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor DefaultCtor = new BoolAttributeDescriptor("default_ctor", IgorAttributeTargets.Type, AttributeInheritance.Inherited);
        public new static readonly BoolAttributeDescriptor Equals = new BoolAttributeDescriptor("equals", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor EqualsIgnore = new BoolAttributeDescriptor("equals.ignore", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor Equality = new BoolAttributeDescriptor("equality", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor EqualityComparer = new BoolAttributeDescriptor("equality_comparer", IgorAttributeTargets.Enum);
        public static readonly EnumAttributeDescriptor<PrimitiveType> Type = new EnumAttributeDescriptor<PrimitiveType>("type", IgorAttributeTargets.Type) { DeprecationMessage = "Use alias instead." };
        public static readonly StringAttributeDescriptor Attribute = new StringAttributeDescriptor("attribute", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Namespace = new StringAttributeDescriptor("namespace", IgorAttributeTargets.Type | IgorAttributeTargets.Service | IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor BinaryNamespace = new StringAttributeDescriptor("binary.namespace", IgorAttributeTargets.Type | IgorAttributeTargets.Service | IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor BinarySerializer = new StringAttributeDescriptor("binary.serializer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor JsonNamespace = new StringAttributeDescriptor("json.namespace", IgorAttributeTargets.Type | IgorAttributeTargets.Service | IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor JsonSerializer = new StringAttributeDescriptor("json.serializer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor StringNamespace = new StringAttributeDescriptor("string.namespace", IgorAttributeTargets.Type | IgorAttributeTargets.Service | IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor StringSerializer = new StringAttributeDescriptor("string.serializer", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor JsonSerializable = new BoolAttributeDescriptor("json.serializable", IgorAttributeTargets.Type, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor EnumBaseTypes = new BoolAttributeDescriptor("enum_base_types", IgorAttributeTargets.Enum, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Messages = new BoolAttributeDescriptor("messages", IgorAttributeTargets.Service);
        public static readonly BoolAttributeDescriptor XmlAttributes = new BoolAttributeDescriptor("xml.attributes", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Immutable = new BoolAttributeDescriptor("immutable", IgorAttributeTargets.Any, AttributeInheritance.Inherited);
        public static readonly BoolAttributeDescriptor Nullable = new BoolAttributeDescriptor("nullable", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor ReadOnly = new BoolAttributeDescriptor("readonly", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<ListTypeImplementation> ListImplementation = new EnumAttributeDescriptor<ListTypeImplementation>("list_implementation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<DictTypeImplementation> DictImplementation = new EnumAttributeDescriptor<DictTypeImplementation>("dict_implementation", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor TargetFramework = new StringAttributeDescriptor("target_framework", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Tpl = new BoolAttributeDescriptor("tpl", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor JsonTest = new BoolAttributeDescriptor("json.test", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static CsAttributes()
        {
            var props = typeof(CsAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
