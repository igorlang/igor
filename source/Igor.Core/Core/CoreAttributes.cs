using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor
{
    public enum XmlPairItemType
    {
        Element,
        Attribute,
        Content,
    }

    public class XmlNamespaceAttribute
    {
        public string prefix { get; set; }
        public string @namespace { get; set; }
        public string location { get; set; }
    }

    /// <summary>
    /// Built-in target-independent attributes
    /// </summary>
    public static class CoreAttributes
    {
        public static readonly BoolAttributeDescriptor Enabled = new BoolAttributeDescriptor("enabled", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Annotation = new StringAttributeDescriptor("annotation", IgorAttributeTargets.Any);
        public static readonly BoolAttributeDescriptor Flags = new BoolAttributeDescriptor("flags", IgorAttributeTargets.Enum);
        public static readonly BoolAttributeDescriptor NotEmpty = new BoolAttributeDescriptor("not_empty", IgorAttributeTargets.Type | IgorAttributeTargets.RecordField, AttributeInheritance.Type);
        public static readonly EnumAttributeDescriptor<IntegerType> IntType = new EnumAttributeDescriptor<IntegerType>("int_type", IgorAttributeTargets.Enum);
        public static readonly BoolAttributeDescriptor PatchRecord = new BoolAttributeDescriptor("patch_record", IgorAttributeTargets.Record);

        public static readonly BoolAttributeDescriptor JsonEnabled = new BoolAttributeDescriptor("json.enabled", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor JsonIgnore = new BoolAttributeDescriptor("json.ignore", IgorAttributeTargets.RecordField);
        public static readonly EnumAttributeDescriptor<Notation> JsonNotation = new EnumAttributeDescriptor<Notation>("json.notation", IgorAttributeTargets.Type, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> JsonFieldNotation = new EnumAttributeDescriptor<Notation>("json.field_notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor JsonKey = new StringAttributeDescriptor("json.key", IgorAttributeTargets.Any);
        public static readonly BoolAttributeDescriptor JsonNulls = new BoolAttributeDescriptor("json.nulls", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor JsonNumber = new BoolAttributeDescriptor("json.number", IgorAttributeTargets.Enum);

        public static readonly BoolAttributeDescriptor BinaryEnabled = new BoolAttributeDescriptor("binary.enabled", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor BinaryIgnore = new BoolAttributeDescriptor("binary.ignore", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor BinaryHeader = new BoolAttributeDescriptor("binary.header", IgorAttributeTargets.Record);

        public static readonly BoolAttributeDescriptor XmlEnabled = new BoolAttributeDescriptor("xml.enabled", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor XmlIgnore = new BoolAttributeDescriptor("xml.ignore", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor XmlElement = new BoolAttributeDescriptor("xml.element", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor XmlSimpleType = new BoolAttributeDescriptor("xml.simple_type", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor XmlComplexType = new BoolAttributeDescriptor("xml.complex_type", IgorAttributeTargets.Type);
        public static readonly BoolAttributeDescriptor XmlContent = new BoolAttributeDescriptor("xml.content", IgorAttributeTargets.Type | IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor XmlText = new BoolAttributeDescriptor("xml.text", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor XmlAttribute = new BoolAttributeDescriptor("xml.attribute", IgorAttributeTargets.RecordField);
        public static readonly BoolAttributeDescriptor XmlOrdered = new BoolAttributeDescriptor("xml.ordered", IgorAttributeTargets.Type, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> XmlNotation = new EnumAttributeDescriptor<Notation>("xml.notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> XmlEnumNotation = new EnumAttributeDescriptor<Notation>("xml.enum_notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> XmlElementNotation = new EnumAttributeDescriptor<Notation>("xml.element_notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> XmlAttributeNotation = new EnumAttributeDescriptor<Notation>("xml.attribute_notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor XmlName = new StringAttributeDescriptor("xml.name", IgorAttributeTargets.Any);
        public static readonly ObjectAttributeDescriptor<XmlNamespaceAttribute> XmlXmlns = new ObjectAttributeDescriptor<XmlNamespaceAttribute>("xml.xmlns", IgorAttributeTargets.Module);
        public static readonly BoolAttributeDescriptor XmlFlat = new BoolAttributeDescriptor("xml.flat", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor XmlKVList = new BoolAttributeDescriptor("xml.kvlist", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor XmlItemName = new StringAttributeDescriptor("xml.item_name", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor XmlKeyName = new StringAttributeDescriptor("xml.key_name", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor XmlValueName = new StringAttributeDescriptor("xml.value_name", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly EnumAttributeDescriptor<XmlPairItemType> XmlKey = new EnumAttributeDescriptor<XmlPairItemType>("xml.key", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly EnumAttributeDescriptor<XmlPairItemType> XmlValue = new EnumAttributeDescriptor<XmlPairItemType>("xml.value", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor XsdXsType = new StringAttributeDescriptor("xsd.xs_type", IgorAttributeTargets.Type, AttributeInheritance.Type);

        public static readonly BoolAttributeDescriptor Client = new BoolAttributeDescriptor("client", IgorAttributeTargets.Service);
        public static readonly BoolAttributeDescriptor Server = new BoolAttributeDescriptor("server", IgorAttributeTargets.Service);
        public static readonly BoolAttributeDescriptor HttpClient = new BoolAttributeDescriptor("http.client", IgorAttributeTargets.WebService, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor HttpServer = new BoolAttributeDescriptor("http.server", IgorAttributeTargets.WebService, AttributeInheritance.Scope);

        public static readonly BoolAttributeDescriptor HttpUnfold = new BoolAttributeDescriptor("http.unfold", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor HttpUnfoldIndex = new BoolAttributeDescriptor("http.unfold_index", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor HttpSeparator = new StringAttributeDescriptor("http.separator", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly BoolAttributeDescriptor HttpPathParts = new BoolAttributeDescriptor("http.path_parts", IgorAttributeTargets.Any, AttributeInheritance.Type);

        public static readonly BoolAttributeDescriptor HttpFormEnabled = new BoolAttributeDescriptor("http.form.enabled", IgorAttributeTargets.Type, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> HttpFormNotation = new EnumAttributeDescriptor<Notation>("http.form.notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HttpFormName = new StringAttributeDescriptor("http.form.name", IgorAttributeTargets.RecordField);

        public static readonly BoolAttributeDescriptor StringEnabled = new BoolAttributeDescriptor("string.enabled", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<Notation> StringFieldNotation = new EnumAttributeDescriptor<Notation>("string.field_notation", IgorAttributeTargets.RecordField | IgorAttributeTargets.EnumField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor StringValue = new StringAttributeDescriptor("string.value", IgorAttributeTargets.Any);

        public static readonly IntAttributeDescriptor IntMin = new IntAttributeDescriptor("min", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly IntAttributeDescriptor IntMax = new IntAttributeDescriptor("max", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly FloatAttributeDescriptor FloatMin = new FloatAttributeDescriptor("min", IgorAttributeTargets.Any, AttributeInheritance.Type);
        public static readonly FloatAttributeDescriptor FloatMax = new FloatAttributeDescriptor("max", IgorAttributeTargets.Any, AttributeInheritance.Type);

        /// <summary>
        /// Returns the list of all built-in target-independent attributes
        /// </summary>
        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static CoreAttributes()
        {
            var props = typeof(CoreAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
