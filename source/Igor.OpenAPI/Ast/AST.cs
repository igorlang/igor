using Igor.Compiler;
using Igor.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Igor.OpenAPI.AST
{
    /// <summary>
    /// AST type marker interface
    /// </summary>
    public interface IType
    {
        IAttributeHost TypeHost { get; }
    }

    /// <summary>
    /// AST interface marker interface
    /// </summary>
    public interface IInterface
    {
    }

    /// <summary>
    /// Base type for named AST declaration.
    /// Statements can host Igor attributes.
    /// </summary>
    public abstract partial class Statement : IAttributeHost
    {
        public virtual IAttributeHost ScopeHost => null;
        public virtual IAttributeHost ParentTypeHost => null;
        public virtual IAttributeHost InheritedHost => null;

        /// <summary>
        /// List of nested declarations
        /// </summary>
        public virtual IEnumerable<IAttributeHost> NestedHosts => Enumerable.Empty<IAttributeHost>();

        /// <summary>
        /// Declaration location in Igor source file
        /// </summary>
        public Location Location { get; internal set; }

        /// <summary>
        /// The attribute collection. Obsolete and will be removed in future releases.
        /// Use Attribute or ListAttribute functions instead.
        /// </summary>
        public IReadOnlyList<AttributeDefinition> Attributes { get; internal set; }

        [Obsolete("Use Attribute function instead")]
        public Statement attributes => this;

        /// <summary>
        /// Name of the entity defined by this declaration
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Annotation string (if present in Igor source file) or null (if not present)
        /// </summary>
        public string Annotation => Attribute(CoreAttributes.Annotation);

        /// <summary>
        /// String representation of this AST, for debug purposes
        /// </summary>
        /// <returns>String representation of this AST</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Report an error related to this statement declaration.
        /// </summary>
        /// <param name="message">Error description</param>
        public void Error(string message) => Context.Instance.CompilerOutput.Error(Location, $"{Name}: {message}", ProblemCode.TargetSpecificProblem);

        /// <summary>
        /// Report a warning related to this statement declaration.
        /// </summary>
        /// <param name="message">Warning description</param>
        public void Warning(string message) => Context.Instance.CompilerOutput.Warning(Location, $"{Name}: {message}", ProblemCode.TargetSpecificProblem);

        /// <summary>
        /// Report a hint related to this statement declaration.
        /// </summary>
        /// <param name="message">Hint description</param>
        public void Hint(string message) => Context.Instance.CompilerOutput.Hint(Location, $"{Name}: {message}", ProblemCode.TargetSpecificProblem);

        /// <summary>
        /// Get a single attribute value set for this AST statement or inherited using inheritance type defined by attribute descriptor
        /// (or default value if value is unset in Igor source or environment)
        /// </summary>
        /// <typeparam name="T">Attribute value type argument</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Attribute value or default</returns>
        public T Attribute<T>(AttributeDescriptor<T> attribute, T defaultValue) => AttributeHelper.Attribute(this, attribute, defaultValue);

        /// <summary>
        /// Get a single attribute value set for this AST statement or inherited using inheritance type defined by attribute descriptor
        /// (or null if value is unset in Igor source or environment).
        /// This is a function overload for value/struct types: numbers, bools and enums.
        /// </summary>
        /// <typeparam name="T">Attribute value type argument (must be value/struct type)</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <returns>Nullable attribute value (null if value is not found)</returns>
        public T? Attribute<T>(StructAttributeDescriptor<T> attribute) where T : struct => AttributeHelper.Attribute(this, attribute);

        /// <summary>
        /// Get a single attribute value set for this AST statement or inherited using inheritance type defined by attribute descriptor
        /// (or null if value is unset in Igor source or environment).
        /// This is a function overload for reference/class types: strings and objects.
        /// </summary>
        /// <typeparam name="T">Attribute value type argument (must be reference/class type)</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <returns>Attribute value (null if value is not found)</returns>
        public T Attribute<T>(ClassAttributeDescriptor<T> attribute) where T : class => AttributeHelper.Attribute(this, attribute);

        /// <summary>
        /// Get a list of all attribute values set for this AST statement or inherited using inheritance type defined by attribute descriptor.
        /// Empty list is returned if no values are found.
        /// </summary>
        /// <typeparam name="T">Attribute value type argument</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <returns>List of found attribute values</returns>
        public IReadOnlyList<T> ListAttribute<T>(AttributeDescriptor<T> attribute) => AttributeHelper.ListAttribute(this, attribute);

        public int? intMin => Attribute(CoreAttributes.IntMin);
        public int? intMax => Attribute(CoreAttributes.IntMax);
        public double? floatMin => Attribute(CoreAttributes.FloatMin);
        public double? floatMax => Attribute(CoreAttributes.FloatMax);
    }

    /// <summary>
    /// Module AST
    /// </summary>
    public partial class Module : Statement
    {
        /// <summary>
        /// List of top-level definitions (forms)
        /// </summary>
        public IReadOnlyList<Form> Definitions { get; internal set; }

        /// <summary>
        /// List of enum types declared in this module
        /// </summary>
        public IEnumerable<EnumForm> Enums => Definitions.OfType<EnumForm>();

        /// <summary>
        /// List of struct types (records, variants, interfaces) declared in this module
        /// </summary>
        public IEnumerable<StructForm> Structs => Definitions.OfType<StructForm>();

        /// <summary>
        /// List of records declared in this module
        /// </summary>
        public IEnumerable<RecordForm> Records => Definitions.OfType<RecordForm>();

        /// <summary>
        /// List of all types declared in this module
        /// </summary>
        public IEnumerable<TypeForm> Types => Definitions.OfType<TypeForm>();

        /// <summary>
        /// List of services declared in this module
        /// </summary>
        public IEnumerable<ServiceForm> Services => Definitions.OfType<ServiceForm>();

        /// <summary>
        /// List of web services declared in this module
        /// </summary>
        public IEnumerable<WebServiceForm> WebServices => Definitions.OfType<WebServiceForm>();

        /// <summary>
        /// List of tables declared in this module
        /// </summary>
        public IEnumerable<TableForm> Tables => Definitions.OfType<TableForm>();

        public override IEnumerable<IAttributeHost> NestedHosts => Definitions;
    }

    /// <summary>
    /// Base AST type for top-level declarations (forms) contained in modules: enums, records, services, etc.
    /// </summary>
    public abstract partial class Form : Statement
    {
        public override IAttributeHost ScopeHost => Module;

        /// <summary>
        /// AST of module in which this form is declared
        /// </summary>
        public Module Module { get; internal set; }

        /// <summary>
        /// Is JSON serialization enabled (the value of json.enabled attribute)
        /// </summary>
        public bool jsonEnabled => Attribute(CoreAttributes.JsonEnabled, false);

        /// <summary>
        /// Is binary serialization enabled (the value of binary.enabled attribute)
        /// </summary>
        public bool binaryEnabled => Attribute(CoreAttributes.BinaryEnabled, false);

        /// <summary>
        /// Is XML serialization enabled (the value of xml.enabled attribute)
        /// </summary>
        public bool xmlEnabled => Attribute(CoreAttributes.XmlEnabled, false);

        /// <summary>
        /// Is string serialization enabled (the value of string.enabled attribute)
        /// </summary>
        public bool stringEnabled => Attribute(CoreAttributes.StringEnabled, false);
    }

    public enum XmlType
    {
        ComplexType,
        SimpleType,
        Element,
        Content,
    }

    /// <summary>
    /// Base AST type for type declarations
    /// </summary>
    public abstract partial class TypeForm : Form, IType
    {
        public IAttributeHost TypeHost => this;

        /// <summary>
        /// List of generic arguments (type variables)
        /// </summary>
        public IReadOnlyList<GenericArgument> Args { get; internal set; }

        /// <summary>
        /// Generic arity (number of type arguments)
        /// </summary>
        public int Arity => Args.Count;

        /// <summary>
        /// Is type generic
        /// </summary>
        public bool IsGeneric => Arity > 0;

        private bool xmlElement => Attribute(CoreAttributes.XmlElement, false);
        private bool xmlSimpleType => Attribute(CoreAttributes.XmlSimpleType, false);
        private bool xmlComplexType => Attribute(CoreAttributes.XmlComplexType, false);
        private bool xmlContent => Attribute(CoreAttributes.XmlContent, false);
        public string xsdXsType => Attribute(CoreAttributes.XsdXsType, null);

        public XmlType xmlType
        {
            get
            {
                if (this is RecordForm f && f.Ancestor != null)
                    return f.Ancestor.xmlType;
                if (xmlElement)
                    return XmlType.Element;
                if (xmlSimpleType)
                    return XmlType.SimpleType;
                if (xmlContent)
                    return XmlType.Content;
                if (xmlComplexType)
                    return XmlType.ComplexType;
                if (this is EnumForm)
                    return XmlType.SimpleType;
                return XmlType.Element;
            }
        }
    }

    /// <summary>
    /// Generic argument type variable
    /// </summary>
    public partial class GenericArgument : IType
    {
        /// <summary>
        /// Location in Igor source file
        /// </summary>
        public Location Location { get; internal set; }

        /// <summary>
        /// Name of generic type variable
        /// </summary>
        public string Name { get; internal set; }

        public IAttributeHost TypeHost => null;
    }

    /// <summary>
    /// Enum field declaration AST
    /// </summary>
    public partial class EnumField : Statement
    {
        public override IAttributeHost ScopeHost => Enum;

        /// <summary>
        /// Integer value of enum field
        /// </summary>
        public long Value { get; internal set; }

        /// <summary>
        /// Enum AST this field belongs to
        /// </summary>
        public EnumForm Enum { get; internal set; }

        /// <summary>
        /// Notation used to translate Igor name to JSON string values (the value of json.field_notation attribute)
        /// </summary>
        public Notation jsonFieldNotation => Attribute(CoreAttributes.JsonFieldNotation, Notation.None);

        /// <summary>
        /// JSON serialization value for this enum field, translated using jsonFieldNotation or overriden by json.key attribute
        /// </summary>
        public string jsonKey => Attribute(CoreAttributes.JsonKey, Name.Format(jsonFieldNotation));

        /// <summary>
        /// Notation used to translate Igor name to XML string values (the value of xml.enum_notation attribute)
        /// </summary>
        public Notation xmlEnumNotation => Attribute(CoreAttributes.XmlEnumNotation, Notation.None);

        /// <summary>
        /// XML serialization value for this enum field, translated using xmlEnumNotation or overriden by xml.name attribute
        /// </summary>
        public string xmlName => Attribute(CoreAttributes.XmlName, Name.Format(xmlEnumNotation));

        /// <summary>
        /// Notation used to translate Igor name to string values (the value of string.field_notation attribute)
        /// </summary>
        public Notation stringFieldNotation => Attribute(CoreAttributes.StringFieldNotation, Notation.None);

        /// <summary>
        /// String serialization value for this enum field, translated using xmlEnumNotation or overriden by string.value attribute
        /// </summary>
        public string stringValue => Attribute(CoreAttributes.StringValue, Name.Format(stringFieldNotation));
    }

    /// <summary>
    /// Enum type declaration AST
    /// </summary>
    public partial class EnumForm : TypeForm
    {
        [Obsolete("Use IntType instead")]
        public IntegerType intType => IntType;

        /// <summary>
        /// Base integer type. This is the minimum range integer type that includes all field values, or the value of int_type attribute.
        /// </summary>
        public IntegerType IntType => Attribute(CoreAttributes.IntType, Primitive.BestIntegerType(Fields.Select(f => f.Value)));

        /// <summary>
        /// Enum represents bit flags
        /// </summary>
        public bool Flags => Attribute(CoreAttributes.Flags, false);

        /// <summary>
        /// Serialize enum values to JSON as integer numbers
        /// </summary>
        public bool jsonNumber => Attribute(CoreAttributes.JsonNumber, false);

        /// <summary>
        /// The list of enum fields
        /// </summary>
        public IReadOnlyList<EnumField> Fields { get; internal set; }

        public override IEnumerable<IAttributeHost> NestedHosts => Fields;
    }

    /// <summary>
    /// Record, variant or interface field declaration AST
    /// </summary>
    public partial class RecordField : Statement
    {
        public override IAttributeHost ScopeHost => Struct;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        /// <summary>
        /// Type AST of this field
        /// </summary>
        public IType Type { get; internal set; }

        /// <summary>
        /// Default value of the field
        /// </summary>
        public Value DefaultValue { get; internal set; }

        /// <summary>
        /// Is the field a variant tag?
        /// When deserializing variants, tag field is deserialized first, to define which record it is.
        /// </summary>
        public bool IsTag { get; internal set; }

        /// <summary>
        /// Is the field inherited from the ancestor variant?
        /// </summary>
        public bool IsInherited => InheritedDeclaration != null;

        /// <summary>
        /// Is the field local (not inherited)?
        /// </summary>
        public bool IsLocal { get; internal set; }

        /// <summary>
        /// Struct (record, interface or variant) this field belongs to.
        /// </summary>
        public StructForm Struct { get; internal set; }

        public IReadOnlyList<RecordField> InterfaceDeclarations { get; internal set; }
        public RecordField InheritedDeclaration { get; internal set; }

        /// <summary>
        /// Should this field be skipped during binary serialization (value of binary.ignore attribute)
        /// </summary>
        public bool binaryIgnore => Attribute(CoreAttributes.BinaryIgnore, false);

        /// <summary>
        /// Notation used to translate Igor name to JSON string values (value of json.field_notation attribute)
        /// </summary>
        public Notation jsonFieldNotation => Attribute(CoreAttributes.JsonFieldNotation, Notation.None);

        /// <summary>
        /// JSON serialization object key for this field, translated using jsonFieldNotation or overriden by json.key attribute
        /// </summary>
        public string jsonKey => Attribute(CoreAttributes.JsonKey, Name.Format(jsonFieldNotation));

        /// <summary>
        /// Should this field be skipped during JSON serialization (value of json.ignore attribute)
        /// </summary>
        public bool jsonIgnore => Attribute(CoreAttributes.JsonIgnore, false);

        /// <summary>
        /// Should this field be skipped during XML serialization (value of xml.ignore attribute)
        /// </summary>
        public bool xmlIgnore => Attribute(CoreAttributes.XmlIgnore, false);

        public Notation xmlElementNotation => Attribute(CoreAttributes.XmlElementNotation, Attribute(CoreAttributes.XmlNotation, Notation.None));
        public Notation xmlAttributeNotation => Attribute(CoreAttributes.XmlAttributeNotation, Attribute(CoreAttributes.XmlNotation, Notation.None));
        public string xmlElementName => Attribute(CoreAttributes.XmlName, Name.Format(xmlElementNotation));
        public string xmlAttributeName => Attribute(CoreAttributes.XmlName, Name.Format(xmlAttributeNotation));
        public bool xmlContent => Attribute(CoreAttributes.XmlContent, false);
        public bool xmlAttribute => Attribute(CoreAttributes.XmlAttribute, false);
        public bool xmlText => Attribute(CoreAttributes.XmlText, false);
        public string xsdXsType => Attribute(CoreAttributes.XsdXsType, null);

        /// <summary>
        /// Default value AST (or null if there's no default value)
        /// </summary>
        public Value Default => IsTag && Struct is RecordForm rec ? rec.TagValue : DefaultValue;

        /// <summary>
        /// Does the field has default value?
        /// </summary>
        public bool HasDefault => Default != null;

        /// <summary>
        /// Is field optional?
        /// </summary>
        public bool IsOptional => Type is BuiltInType.Optional || Struct.IsPatch;

        /// <summary>
        /// Returns non-optional type of this field
        /// </summary>
        public IType NonOptType => Type is BuiltInType.Optional optional ? optional.ItemType : Type;
    }

    /// <summary>
    /// Base AST type for struct declarations: records, variants and interfaces
    /// </summary>
    public abstract partial class StructForm : TypeForm
    {
        public override IAttributeHost InheritedHost => Ancestor;

        /// <summary>
        /// List of all fields (both local and inherited)
        /// </summary>
        public IReadOnlyList<RecordField> Fields { get; internal set; }

        public override IEnumerable<IAttributeHost> NestedHosts => Fields;

        /// <summary>
        /// Ancestor variant (or null if there's no ancestor)
        /// </summary>
        public VariantForm Ancestor { get; internal set; }

        /// <summary>
        /// List of interfaces this struct implements
        /// </summary>
        public IReadOnlyList<IInterface> Interfaces { get; internal set; }

        /// <summary>
        /// Variant tag field (or null if there's no tag field)
        /// </summary>
        public RecordField TagField { get; internal set; }

        /// <summary>
        /// Is this struct an exception?
        /// </summary>
        public bool IsException { get; internal set; }

        /// <summary>
        /// List of fields included into JSON serialization
        /// </summary>
        public IEnumerable<RecordField> jsonSerializedFields => Fields.Where(f => !f.jsonIgnore);
        public Notation jsonNotation => Attribute(CoreAttributes.JsonNotation, Notation.None);
        public string jsonKey => Name.Format(jsonNotation);

        /// <summary>
        /// Whether generated JSON serializer should include nulls for optional fields.
        /// If this property is not set, serializer may prefer the most convenient or efficient implementation.
        /// </summary>
        public bool? jsonNulls => Attribute(CoreAttributes.JsonNulls);

        /// <summary>
        /// List of fields included into binary serialization
        /// </summary>
        public IEnumerable<RecordField> binarySerializedFields => Fields.Where(f => !f.binaryIgnore && !f.IsTag);
        public bool binaryHeader => Attribute(CoreAttributes.BinaryHeader, true);

        /// <summary>
        /// List of fields included into XML serialization
        /// </summary>
        public IEnumerable<RecordField> xmlSerializedFields => Fields.Where(f => !f.xmlIgnore && !f.IsTag);
        public Notation xmlElementNotation => Attribute(CoreAttributes.XmlElementNotation, Attribute(CoreAttributes.XmlNotation, Notation.None));
        public string xmlElementName => Attribute(CoreAttributes.XmlName, Name.Format(xmlElementNotation));
        public bool xmlOrdered => Attribute(CoreAttributes.XmlOrdered, false);

        /// <summary>
        /// Is it a patch record (value of patch_record attribute)?
        /// </summary>
        public bool IsPatch => Attribute(CoreAttributes.PatchRecord, false);
    }

    /// <summary>
    /// Record declaration AST
    /// </summary>
    public partial class RecordForm : StructForm
    {
        /// <summary>
        /// Tag field value for this record
        /// </summary>
        public Value TagValue { get; internal set; }
    }

    /// <summary>
    /// Interface declaration AST
    /// </summary>
    public partial class InterfaceForm : StructForm, IInterface
    {
    }

    /// <summary>
    /// Variant declaration AST
    /// </summary>
    public partial class VariantForm : StructForm
    {
        /// <summary>
        /// List of direct descendants (both records and variants)
        /// </summary>
        public IReadOnlyList<StructForm> Descendants { get; internal set; }

        /// <summary>
        /// List of all record descendants, both direct and indirect
        /// </summary>
        public IReadOnlyList<RecordForm> Records { get; internal set; }
    }

    /// <summary>
    /// Union clause declaration AST
    /// </summary>
    public partial class UnionClause : Statement
    {
        public override IAttributeHost ParentTypeHost => Type?.TypeHost;
        public override IAttributeHost ScopeHost => Union;

        public IType Type { get; internal set; }
        public UnionForm Union { get; internal set; }
        public bool IsSingleton => Type == null;
        public Notation jsonFieldNotation => Attribute(CoreAttributes.JsonFieldNotation, Notation.None);
        public string jsonKey => Attribute(CoreAttributes.JsonKey, Name.Format(jsonFieldNotation));
        public Notation xmlElementNotation => Attribute(CoreAttributes.XmlElementNotation, Attribute(CoreAttributes.XmlNotation, Notation.None));
        public string xmlElementName => Attribute(CoreAttributes.XmlName, Name.Format(xmlElementNotation));
        public bool xmlText => Attribute(CoreAttributes.XmlText, false);
        public bool xmlContent => Attribute(CoreAttributes.XmlContent, false);
    }

    /// <summary>
    /// Union type declaration AST
    /// </summary>
    public partial class UnionForm : TypeForm
    {
        public IReadOnlyList<UnionClause> Clauses { get; internal set; }
        public override IEnumerable<IAttributeHost> NestedHosts => Clauses;
    }

    /// <summary>
    /// Alias (define) type declaration AST
    /// </summary>
    public partial class DefineForm : TypeForm
    {
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        /// <summary>
        /// Alias target type. This define type is defined as an alias for that type.
        /// </summary>
        public IType Type { get; internal set; }
    }

    public partial class TableField : Statement
    {
        public override IAttributeHost ScopeHost => Table;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;
        public IType Type { get; internal set; }
        public Value DefaultValue { get; internal set; }
        public TableForm Table { get; internal set; }
        public TableField ForeignKey { get; internal set; }
    }

    public partial class TableForm : Form
    {
        public IReadOnlyList<TableField> Fields { get; internal set; }
        public override IEnumerable<IAttributeHost> NestedHosts => Fields;
    }

    /// <summary>
    /// Service function argument AST
    /// </summary>
    public partial class FunctionArgument
    {
        /// <summary>
        /// Argument name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Argument type
        /// </summary>
        public IType Type { get; internal set; }

        /// <summary>
        /// Function AST this argument belongs to
        /// </summary>
        public ServiceFunction Function { get; internal set; }

        /// <summary>
        /// Zero-based index of this argument
        /// </summary>
        public int Index { get; internal set; }

        public override string ToString() => Name;
    }

    /// <summary>
    /// AST of reference to exception thrown by service functions
    /// </summary>
    public partial class FunctionThrow
    {
        /// <summary>
        /// Thrown exception AST exception record
        /// </summary>
        public RecordForm Exception { get; internal set; }

        /// <summary>
        /// Unique integer ID for serialization
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Function AST
        /// </summary>
        public ServiceFunction Function { get; internal set; }
    }

    /// <summary>
    /// Service function declaration AST
    /// </summary>
    public partial class ServiceFunction : Statement
    {
        public override IAttributeHost ScopeHost => Service;

        public Notation jsonNotation => Attribute(CoreAttributes.JsonNotation, Notation.None);
        public string jsonKey => Name.Format(jsonNotation);

        /// <summary>
        /// Function direction (c->s or s->c)
        /// </summary>
        public Direction Direction { get; internal set; }

        /// <summary>
        /// List of function arguments
        /// </summary>
        public IReadOnlyList<FunctionArgument> Arguments { get; internal set; }

        /// <summary>
        /// List of returned result arguments. Null if function is not an RPC.
        /// </summary>
        public IReadOnlyList<FunctionArgument> ReturnArguments { get; internal set; }

        /// <summary>
        /// List of thrown exception references
        /// </summary>
        public IReadOnlyList<FunctionThrow> Throws { get; internal set; }

        /// <summary>
        /// Function index in the service
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Service AST this function belongs to
        /// </summary>
        public ServiceForm Service { get; internal set; }

        /// <summary>
        /// Is function an RPC?
        /// </summary>
        public bool IsRpc { get; internal set; }

        internal FunctionType Type(Direction direction)
        {
            if (Direction == direction)
                return IsRpc ? FunctionType.SendRpc : FunctionType.SendCast;
            else
                return IsRpc ? FunctionType.RecvRpc : FunctionType.RecvCast;
        }
    }

    /// <summary>
    /// Service declaration AST
    /// </summary>
    public partial class ServiceForm : Form
    {
        /// <summary>
        /// List of functions
        /// </summary>
        public IReadOnlyList<ServiceFunction> Functions { get; internal set; }

        public override IEnumerable<IAttributeHost> NestedHosts => Functions;
    }

    [Flags]
    internal enum FunctionType
    {
        SendCast = 1,
        SendRpc = 2,
        RecvCast = 4,
        RecvRpc = 8,
        Cast = SendCast | SendRpc,
        Rpc = SendRpc | RecvRpc,
        Send = SendCast | SendRpc,
        Recv = RecvCast | RecvRpc,
    }

    /// <summary>
    /// Web service declaration AST
    /// </summary>
    public partial class WebServiceForm : Form
    {
        public IReadOnlyList<WebResource> Resources { get; internal set; }
        public override IEnumerable<IAttributeHost> NestedHosts => Resources;
        public bool webServerEnabled => Attribute(CoreAttributes.HttpServer, false);
        public bool webClientEnabled => Attribute(CoreAttributes.HttpClient, false);
    }

    /// <summary>
    /// Web resource declaration AST
    /// </summary>
    public partial class WebResource : Statement
    {
        public override IAttributeHost ScopeHost => WebService;

        public override IEnumerable<IAttributeHost> NestedHosts =>
            (RequestContent?.Var).YieldIfNotNull().Concat(RequestVariables).
            Concat(Responses.SelectMany(r => (r.Content?.Var).YieldIfNotNull().Concat(r.HeadersVariables)));

        /// <summary>
        /// Web service AST where this resource is defined
        /// </summary>
        public WebServiceForm WebService { get; internal set; }

        /// <summary>
        /// List of URL path segments
        /// </summary>
        public IReadOnlyList<WebPathSegment> Path { get; internal set; }

        /// <summary>
        /// List of URL query parameters
        /// </summary>
        public IReadOnlyList<WebQueryParameter> Query { get; internal set; }

        /// <summary>
        /// HTTP method
        /// </summary>
        public HttpMethod Method { get; internal set; }

        /// <summary>
        /// List of possible responses
        /// </summary>
        public List<WebResponse> Responses { get; internal set; }

        /// <summary>
        /// List of request headers
        /// </summary>
        public IReadOnlyList<WebHeader> RequestHeaders { get; internal set; }

        /// <summary>
        /// Request content AST, if available. Can be null if request has no body.
        /// </summary>
        public WebContent RequestContent { get; internal set; }

        /// <summary>
        /// Request body type AST. Can be null if request has no body.
        /// </summary>
        public IType RequestBodyType => RequestContent?.Type;

        public IType ResponseBodyType => Responses.FirstOrDefault()?.Content?.Type;

        public IEnumerable<WebVariable> PathVariables => Path.Where(v => !v.IsStatic).Select(v => v.Var);
        public IEnumerable<WebVariable> QueryVariables => Query.Where(v => !v.IsStatic).Select(v => v.Var);
        public IEnumerable<WebVariable> RequestHeadersVariables => RequestHeaders.Where(v => !v.IsStatic).Select(v => v.Var);
        public IEnumerable<WebVariable> RequestVariables => PathVariables.Concat(QueryVariables).Concat(RequestHeadersVariables);
        public WebHeader RequestContentTypeHeader => RequestHeaders.FirstOrDefault(h => h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase));
    }

    public partial class WebStatusCode
    {
        public int Code { get; internal set; }
        public string ReasonPhrase { get; internal set; }
    }

    public partial class WebResponse
    {
        public IReadOnlyList<WebHeader> Headers { get; internal set; }
        public WebContent Content { get; internal set; }
        public WebStatusCode Status { get; internal set; }
        public int StatusCode => Status?.Code ?? 200;
        public bool IsSuccess => Status == null || Status.Code < 300;

        /// <summary>
        /// AST of resource owning this response
        /// </summary>
        public WebResource Resource { get; internal set; }

        public IEnumerable<WebVariable> HeadersVariables => Headers.Where(v => !v.IsStatic).Select(v => v.Var);
        public WebHeader ContentTypeHeader => Headers.FirstOrDefault(h => h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase));
    }

    public partial class WebContent
    {
        /// <summary>
        /// Content Type
        /// </summary>
        public IType Type { get; internal set; }

        [Obsolete("Use Type instead.")]
        public IType ContentType => Type;

        /// <summary>
        /// Data format of the content type
        /// </summary>
        public DataFormat Format { get; internal set; }

        /// <summary>
        /// Resource this content belongs to
        /// </summary>
        public WebResource Resource { get; internal set; }

        /// <summary>
        /// Content variable. May be null, if only type was provided
        /// </summary>
        public WebVariable Var { get; internal set; }

        /// <summary>
        /// Annotation string (if present in Igor source file) or null (if not present)
        /// </summary>
        public string Annotation => Var?.Annotation;
    }

    public partial class WebHeader
    {
        /// <summary>
        /// Header name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Static header value. 
        /// Either this value or <cref>Var</cref> is always present.
        /// </summary>
        public string StaticValue { get; internal set; }

        /// <summary>
        /// Path variable.
        /// Either this value or <cref>StaticValue</cref> is always present.
        /// </summary>
        public WebVariable Var { get; internal set; }

        /// <summary>
        /// True if header has a static value.
        /// </summary>
        public bool IsStatic => Var == null;
    }

    public partial class WebPathSegment
    {
        /// <summary>
        /// Static string segment. 
        /// Either this value or <cref>Var</cref> is always present.
        /// </summary>
        public string StaticValue { get; internal set; }
        
        /// <summary>
        /// Path variable.
        /// Either this value or <cref>StaticValue</cref> is always present.
        /// </summary>
        public WebVariable Var { get; internal set; }

        /// <summary>
        /// True if this segment is a static segment.
        /// </summary>
        public bool IsStatic => Var == null;
    }

    public partial class WebQueryParameter
    {
        public string Parameter { get; internal set; }

        /// <summary>
        /// Static query parameter value. 
        /// Either this value or <cref>Var</cref> is always present.
        /// </summary>
        public string StaticValue { get; internal set; }

        /// <summary>
        /// Path variable.
        /// Either this value or <cref>StaticValue</cref> is always present.
        /// </summary>
        public WebVariable Var { get; internal set; }

        /// <summary>
        /// True if query parameter has a static value.
        /// </summary>
        public bool IsStatic => Var == null;

        /// <summary>
        /// Data format for query variable
        /// </summary>
        public DataFormat Format { get; internal set; }
    }

    public enum WebParameterType
    {
        Content,
        Path,
        Query,
        Header,
    }

    public partial class WebVariable : Statement
    {
        public override IAttributeHost ScopeHost => Resource;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        public IType Type { get; internal set; }
        public Value DefaultValue { get; internal set; }
        public bool IsOptional => Type is BuiltInType.Optional;
        public WebResource Resource { get; internal set; }
        public WebParameterType ParameterType { get; internal set; }
    }

    public abstract class Value : IEquatable<Value>
    {
        public class Bool : Value
        {
            public bool Value { get; }

            public Bool(bool value) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Bool v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value ? "true" : "false";
        }

        public class Integer : Value
        {
            public long Value { get; }

            public Integer(long value) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Integer v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class Float : Value
        {
            public double Value { get; }

            public Float(double value) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Float v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class String : Value
        {
            public string Value { get; }

            public String(string value) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is String v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.Quoted();
        }

        public class EmptyObject : Value
        {
            public override bool Equals(Value other) => other is EmptyObject;

            public static readonly EmptyObject Empty = new EmptyObject();

            public override int GetHashCode() => 1001;

            public override string ToString() => "{}";
        }

        public class List : Value
        {
            public IReadOnlyList<Value> Value { get; }

            public List(IReadOnlyList<Value> value) => this.Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is List v && v.Value == Value;

            public static readonly List Empty = new List(new List<Value>(0));

            public override int GetHashCode() => 1002;

            public override string ToString() => "[]";
        }

        public class Dict : Value
        {
            public IReadOnlyDictionary<Value, Value> Value { get; }

            public Dict(IReadOnlyDictionary<Value, Value> value) => this.Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Dict v && v.Value == Value;

            public static readonly Dict Empty = new Dict(new Dictionary<Value, Value>(0));

            public override int GetHashCode() => 1003;

            public override string ToString() => "{}";
        }

        public class Enum : Value
        {
            public EnumField Field { get; }

            public Enum(EnumField field) => this.Field = field;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Enum v && v.Field == Field;

            public override int GetHashCode() => Field.GetHashCode();

            public override string ToString() => Field.Name;
        }

        public class Record : Value
        {
            public IReadOnlyDictionary<RecordField, Value> Value { get; }
            public StructForm Form { get; }

            public Record(StructForm form, IReadOnlyDictionary<RecordField, Value> value)
            {
                this.Form = form;
                this.Value = value;
            }

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Record v && v.Form == Form && v.Value == Value;

            public override int GetHashCode() => Form.GetHashCode();

            public override string ToString() => "{}";
        }

        public abstract bool Equals(Value other);

        public override bool Equals(object obj)
        {
            return obj is Value other && Equals(this, other);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Generic type instance AST
    /// </summary>
    public partial class GenericType : IType
    {
        /// <summary>
        /// Generic type prototype declaration AST
        /// </summary>
        public TypeForm Prototype { get; internal set; }

        /// <summary>
        /// Generic type arguments
        /// </summary>
        public IReadOnlyList<IType> Args { get; internal set; }

        public IAttributeHost TypeHost => Prototype;
    }

    /// <summary>
    /// Generic interface instance AST
    /// </summary>
    public partial class GenericInterface : IInterface
    {
        /// <summary>
        /// Generic interface prototype declaration AST
        /// </summary>
        public InterfaceForm Prototype { get; internal set; }

        /// <summary>
        /// Generic type arguments
        /// </summary>
        public IReadOnlyList<IType> Args { get; internal set; }
    }

    /// <summary>
    /// Igor built-in type instances
    /// </summary>
    public abstract class BuiltInType : IType, IEquatable<BuiltInType>
    {
        /// <summary>
        /// Built-in Igor bool type instance
        /// </summary>
        public class Bool : BuiltInType
        {
            public override string ToString() => "bool";

            public override bool Equals(BuiltInType other) => other is Bool;

            public override int GetHashCode() => (int)PrimitiveType.Bool;
        }

        /// <summary>
        /// Built-in Igor integer type instance
        /// </summary>
        public class Integer : BuiltInType
        {
            [Obsolete("Use Type instead")]
            public IntegerType type => Type;

            public IntegerType Type { get; }

            public Integer(IntegerType type) => this.Type = type;

            public override string ToString() => Type.ToString();

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is Integer t && t.Type == Type;

            public override int GetHashCode() => (int)Type;
        }

        /// <summary>
        /// Built-in Igor float type instance
        /// </summary>
        public class Float : BuiltInType
        {
            [Obsolete("Use Type instead")]
            public FloatType type => Type;

            public FloatType Type { get; }

            public Float(FloatType type) => this.Type = type;

            public override string ToString() => Type.ToString();

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is Float t && t.Type == Type;

            public override int GetHashCode() => (int)Type;
        }

        /// <summary>
        /// Built-in Igor string type instance
        /// </summary>
        public class String : BuiltInType
        {
            public override string ToString() => "string";

            public override bool Equals(BuiltInType other) => other is String;

            public override int GetHashCode() => (int)PrimitiveType.String;
        }

        /// <summary>
        /// Built-in Igor binary type instance
        /// </summary>
        public class Binary : BuiltInType
        {
            public override string ToString() => "binary";

            public override bool Equals(BuiltInType other) => other is Binary;

            public override int GetHashCode() => (int)PrimitiveType.Binary;
        }

        /// <summary>
        /// Built-in Igor atom type instance
        /// </summary>
        public class Atom : BuiltInType
        {
            public override string ToString() => "atom";

            public override bool Equals(BuiltInType other) => other is Atom;

            public override int GetHashCode() => (int)PrimitiveType.Atom;
        }

        /// <summary>
        /// Built-in Igor json type instance
        /// </summary>
        public class Json : BuiltInType
        {
            public override string ToString() => "json";

            public override bool Equals(BuiltInType other) => other is Json;

            public override int GetHashCode() => (int)PrimitiveType.Json;
        }

        /// <summary>
        /// Built-in Igor list type instance
        /// </summary>
        public class List : BuiltInType
        {
            public IType ItemType { get; }

            public List(IType itemType) => this.ItemType = itemType;

            public override string ToString() => $"list<{ItemType}>";

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is List t && t.ItemType.Equals(ItemType);

            public override int GetHashCode() => ItemType.GetHashCode();
        }

        /// <summary>
        /// Built-in Igor dict type instance
        /// </summary>
        public class Dict : BuiltInType
        {
            public IType KeyType { get; }
            public IType ValueType { get; }

            public Dict(IType keyType, IType valueType)
            {
                this.KeyType = keyType;
                this.ValueType = valueType;
            }

            public override string ToString() => $"dict<{KeyType},{ValueType}>";

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is Dict t && t.KeyType.Equals(KeyType) && t.ValueType.Equals(ValueType);

            public override int GetHashCode() => KeyType.GetHashCode() + ValueType.GetHashCode() * 113;
        }

        /// <summary>
        /// Built-in Igor optional type instance (?T)
        /// </summary>
        public class Optional : BuiltInType
        {
            public IType ItemType { get; }

            public Optional(IType itemType) => this.ItemType = itemType;

            public override string ToString() => $"?{ItemType}";

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is Optional t && t.ItemType.Equals(ItemType);

            public override int GetHashCode() => ItemType.GetHashCode();

            public override IAttributeHost TypeHost => ItemType.TypeHost;
        }

        /// <summary>
        /// Built-in Igor flags type instance
        /// </summary>
        public class Flags : BuiltInType
        {
            public IType ItemType { get; }

            public Flags(IType itemType) => this.ItemType = itemType;

            public override string ToString() => $"flags<{ItemType}>";

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is Flags t && t.ItemType.Equals(ItemType);

            public override int GetHashCode() => ItemType.GetHashCode();
        }

        /// <summary>
        /// Built-in Igor oneof type instance
        /// </summary>
        public class OneOf : BuiltInType
        {
            public IReadOnlyList<IType> Types { get; }

            public OneOf(IReadOnlyList<IType> types) => this.Types = types;

            public override string ToString() => Types.JoinStrings(" | ", t => t.ToString());

            public override bool Equals(BuiltInType other) => ReferenceEquals(this, other) || other is OneOf t && Enumerable.SequenceEqual(Types, t.Types);

            public override int GetHashCode() => Types.Count.GetHashCode();
        }

        public abstract bool Equals(BuiltInType other);

        public override bool Equals(object obj)
        {
            return obj is BuiltInType other && Equals(this, other);
        }

        public override int GetHashCode() => base.GetHashCode();

        public virtual IAttributeHost TypeHost => null;
    }
}
