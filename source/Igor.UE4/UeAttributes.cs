using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4
{
    public enum UeHttpClientRequestSetup
    {
        [IgorEnumValue("args")]
        Args,
        [IgorEnumValue("request")]
        Request,
    }

    public static class UeAttributes
    {
        public static readonly BoolAttributeDescriptor Ignore = new BoolAttributeDescriptor("ignore", IgorAttributeTargets.RecordField);
        public static readonly StringAttributeDescriptor Name = new StringAttributeDescriptor("name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor BaseType = new StringAttributeDescriptor("base_type", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Alias = new StringAttributeDescriptor("alias", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor Namespace = new StringAttributeDescriptor("namespace", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor Prefix = new StringAttributeDescriptor("prefix", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor Ptr = new BoolAttributeDescriptor("ptr", IgorAttributeTargets.Any);
        public static readonly BoolAttributeDescriptor Typedef = new BoolAttributeDescriptor("typedef", IgorAttributeTargets.Type);
        public static readonly StringAttributeDescriptor Category = new StringAttributeDescriptor("category", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor IgorPath = new StringAttributeDescriptor("igor_path", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor CppInclude = new StringAttributeDescriptor("cpp_include", IgorAttributeTargets.Module);
        public static readonly StringAttributeDescriptor HInclude = new StringAttributeDescriptor("h_include", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor ApiMacro = new StringAttributeDescriptor("api_macro", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor UEnum = new BoolAttributeDescriptor("uenum", IgorAttributeTargets.Enum, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor UStruct = new BoolAttributeDescriptor("ustruct", IgorAttributeTargets.Type, AttributeInheritance.Inherited);
        public static readonly BoolAttributeDescriptor UClass = new BoolAttributeDescriptor("uclass", IgorAttributeTargets.Type, AttributeInheritance.Inherited);
        public static readonly BoolAttributeDescriptor BlueprintType = new BoolAttributeDescriptor("blueprint_type", IgorAttributeTargets.Record);
        public static readonly BoolAttributeDescriptor UProperty = new BoolAttributeDescriptor("uproperty", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor BlueprintReadWrite = new BoolAttributeDescriptor("blueprint_read_write", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor BlueprintReadOnly = new BoolAttributeDescriptor("blueprint_read_only", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor EditAnywhere = new BoolAttributeDescriptor("edit_anywhere", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor EditDefaultsOnly = new BoolAttributeDescriptor("edit_defaults_only", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor VisibleAnywhere = new BoolAttributeDescriptor("visible_anywhere", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor VisibleDefaultsOnly = new BoolAttributeDescriptor("visible_defaults_only", IgorAttributeTargets.RecordField, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor HFile = new StringAttributeDescriptor("h_file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor CppFile = new StringAttributeDescriptor("cpp_file", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor HPath = new StringAttributeDescriptor("h_path", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly StringAttributeDescriptor CppPath = new StringAttributeDescriptor("cpp_path", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor JsonCustomSerializer = new BoolAttributeDescriptor("json.custom_serializer", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor LogCategory = new StringAttributeDescriptor("log_category", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly BoolAttributeDescriptor HttpClientLazy = new BoolAttributeDescriptor("http.client.lazy", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<UeHttpClientRequestSetup> HttpClientPathSetup = new EnumAttributeDescriptor<UeHttpClientRequestSetup>("http.client.path_setup", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<UeHttpClientRequestSetup> HttpClientQuerySetup = new EnumAttributeDescriptor<UeHttpClientRequestSetup>("http.client.query_setup", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<UeHttpClientRequestSetup> HttpClientHeaderSetup = new EnumAttributeDescriptor<UeHttpClientRequestSetup>("http.client.header_setup", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<UeHttpClientRequestSetup> HttpClientContentSetup = new EnumAttributeDescriptor<UeHttpClientRequestSetup>("http.client.content_setup", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        public static readonly EnumAttributeDescriptor<UeHttpClientRequestSetup> HttpClientSetup = new EnumAttributeDescriptor<UeHttpClientRequestSetup>("http.client.setup");
        public static readonly StringAttributeDescriptor HttpBaseUrl = new StringAttributeDescriptor("http.base_url");
        public static readonly JsonAttributeDescriptor Meta = new JsonAttributeDescriptor("meta", IgorAttributeTargets.Any);
        public static readonly BoolAttributeDescriptor Interfaces = new BoolAttributeDescriptor("interfaces", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static UeAttributes()
        {
            var props = typeof(UeAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
