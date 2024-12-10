using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Igor.Compiler;
using Igor.Text;

namespace Igor.Declarations
{
    public class GenericArgsDictionary
    {
        public static readonly GenericArgsDictionary Empty = new GenericArgsDictionary();

        private readonly Dictionary<GenericTypeVariable, IType> args = new Dictionary<GenericTypeVariable, IType>();

        public GenericArgsDictionary(TypeForm prototype, GenericTypeInstance instance)
        {
            for (var i = 0; i < prototype.Args.Count; i++)
            {
                args.Add(prototype.Args[i], instance.Args[i]);
            }
        }

        public GenericArgsDictionary(TypeForm prototype, GenericInterfaceInstance instance)
        {
            for (var i = 0; i < prototype.Args.Count; i++)
            {
                args.Add(prototype.Args[i], instance.Args[i]);
            }
        }

        public GenericArgsDictionary()
        {
        }

        private bool IsOpened(IType type)
        {
            switch (type)
            {
                case GenericTypeInstance instance: return instance.Args.Any(IsOpened);
                case GenericTypeVariable typeVar: return args.ContainsKey(typeVar);
                default: return false;
            }
        }

        private bool IsOpened(IInterface type)
        {
            switch (type)
            {
                case GenericInterfaceInstance instance: return instance.Args.Any(IsOpened);
                default: return false;
            }
        }

        public IType Instantiate(IType type, CompileContext context)
        {
            switch (type)
            {
                case object _ when args.Count == 0: return type;
                case object _ when !IsOpened(type): return type;
                case GenericTypeVariable typeVar: return args[typeVar];
                case GenericTypeInstance instance : return new GenericTypeInstance(instance.Location, instance.Prototype, instance.Args.Select(arg => Instantiate(arg, context)).ToList());
                default: return type;
            }
        }

        public IInterface Instantiate(IInterface type, CompileContext context)
        {
            switch (type)
            {
                case object _ when args.Count == 0: return type;
                case object _ when !IsOpened(type): return type;
                case GenericInterfaceInstance instance : return new GenericInterfaceInstance(instance.Location, instance.Prototype, instance.Args.Select(arg => Instantiate(arg, context)).ToList());
                default: return type;
            }
        }
    }

    public class Located : ILocated
    {
        public Location Location { get; }

        public Located(Location location)
        {
            Location = location;
        }
    }

    public class CompileContext
    {
        public SymbolTable BuiltInScope { get; } = new SymbolTable();
        public SymbolTable ModulesScope { get; } = new SymbolTable();
        public CompilerOutput Output { get; }

        public void RegisterBuiltInSymbols()
        {
            var boolType = new BuiltInType.Bool();
            BuiltInScope.Register(boolType, this, "bool");
            var sbyteType = new BuiltInType.Integer(IntegerType.SByte);
            BuiltInScope.Register(sbyteType, this, "sbyte");
            BuiltInScope.Register(sbyteType, this, "int8");
            var byteType = new BuiltInType.Integer(IntegerType.Byte);
            BuiltInScope.Register(byteType, this, "byte");
            BuiltInScope.Register(byteType, this, "uint8");
            var shortType = new BuiltInType.Integer(IntegerType.Short);
            BuiltInScope.Register(shortType, this, "short");
            BuiltInScope.Register(shortType, this, "int16");
            var ushortType = new BuiltInType.Integer(IntegerType.UShort);
            BuiltInScope.Register(ushortType, this, "ushort");
            BuiltInScope.Register(ushortType, this, "uint16");
            var intType = new BuiltInType.Integer(IntegerType.Int);
            BuiltInScope.Register(intType, this, "int");
            BuiltInScope.Register(intType, this, "int32");
            var uintType = new BuiltInType.Integer(IntegerType.UInt);
            BuiltInScope.Register(uintType, this, "uint");
            BuiltInScope.Register(uintType, this, "uint32");
            var longType = new BuiltInType.Integer(IntegerType.Long);
            BuiltInScope.Register(longType, this, "long");
            BuiltInScope.Register(longType, this, "int64");
            var ulongType = new BuiltInType.Integer(IntegerType.ULong);
            BuiltInScope.Register(ulongType, this, "ulong");
            BuiltInScope.Register(ulongType, this, "uint64");
            var floatType = new BuiltInType.Float(FloatType.Float);
            BuiltInScope.Register(floatType, this, "float");
            BuiltInScope.Register(floatType, this, "float32");
            var doubleType = new BuiltInType.Float(FloatType.Double);
            BuiltInScope.Register(doubleType, this, "double");
            BuiltInScope.Register(doubleType, this, "float64");
            var stringType = new BuiltInType.String();
            BuiltInScope.Register(stringType, this, "string");
            var binaryType = new BuiltInType.Binary();
            BuiltInScope.Register(binaryType, this, "binary");
            var atomType = new BuiltInType.Atom();
            BuiltInScope.Register(atomType, this, "atom");
            var jsonType = new BuiltInType.Json();
            BuiltInScope.Register(jsonType, this, "json");
            var listType = new BuiltInType.List();
            BuiltInScope.Register(listType, this, "list");
            var dictType = new BuiltInType.Dict();
            BuiltInScope.Register(dictType, this, "dict");
            var flagsType = new BuiltInType.Flags();
            BuiltInScope.Register(flagsType, this, "flags");
            BuiltInScope.Register(BuiltInType.Optional.Instance, this, "?");
        }

        public CompileContext(CompilerOutput output)
        {
            Output = output;
        }
    }

    public enum CompileStage
    {
        Stage1, // Register symbols
        Stage2, // Resolve symbols
        Stage3, // Inheritance hierarchy
        Stage4, // Bind values
    }

    public interface ISymbolDeclaration
    {
        SymbolName Name { get; }
    }

    public class SymbolName : Located
    {
        public string Name { get; }
        public bool IsValid { get; }

        public SymbolName(Location location, string name) : this(location, name, !string.IsNullOrEmpty(name))
        {
        }

        private SymbolName(Location location, string name, bool isValid) : base(location)
        {
            Name = name;
            IsValid = isValid;
        }

        public static SymbolName CreateInvalid(Location location) => new SymbolName(location, "<Invalid>", false);

        public override string ToString() => Name.Quoted("'");
    }

    public abstract class SymbolReference : Located
    {
        public string Name { get; }
        public bool IsValid { get; }
        private readonly List<ISymbolDeclaration> declarations = new List<ISymbolDeclaration>();

        public SymbolReference(Location location, string name, bool isValid) : base(location)
        {
            Name = name;
            IsValid = isValid;
        }

        public void Resolve(IScope scope, CompileContext context)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (IsValid)
            {
                if (!scope.Find(this, declarations))
                {
                    context.Output.Error(Location, $"Unresolved reference '{Name}'", ProblemCode.UnresolvedReference);
                }

                ResolveValue(declarations);
            }
        }

        protected abstract void ResolveValue(IReadOnlyList<ISymbolDeclaration> declarations);

    }

    public class SymbolReference<T> : SymbolReference where T : ILocated
    {
        public T Value { get; private set; }

        public SymbolReference(Location location, string name, bool isValid) : base(location, name, isValid)
        {
        }

        protected override void ResolveValue(IReadOnlyList<ISymbolDeclaration> declarations)
        {
            Value = declarations.OfType<T>().FirstOrDefault();
        }
    }

    public class CompilationUnit : Located
    {
        public IReadOnlyList<SymbolReference<Module>> Usings { get; }
        public IReadOnlyList<Module> Modules { get; }

        public CompilationUnit(Location location, IReadOnlyList<SymbolReference<Module>> usings, IReadOnlyList<Module> modules) : base(location)
        {
            Usings = usings;
            Modules = modules;
        }

        public CompilationUnit(Location location) : base(location)
        {
            Usings = Array.Empty<SymbolReference<Module>>();
            Modules = Array.Empty<Module>();
        }

        public void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    foreach (var mod in Modules)
                    {
                        mod.CompilationUnit = this;
                        context.ModulesScope.Register(mod, context);
                    }

                    break;
                case CompileStage.Stage2:
                    Usings.ForEach(use => use.Resolve(context.ModulesScope, context));
                    break;
            }

            Modules.ForEach(d => d.Compile(context, stage));
        }
    }

    public abstract class AttributeHost : Located, IAttributeHost
    {
        public virtual IAttributeHost ScopeHost => null;
        public virtual IAttributeHost ParentTypeHost => null;
        public virtual IAttributeHost InheritedHost => null;
        public virtual IEnumerable<IAttributeHost> NestedHosts => Enumerable.Empty<IAttributeHost>();

        /// <summary>
        /// The attribute collection. Obsolete and will be removed in future releases.
        /// Use Attribute or ListAttribute functions instead.
        /// </summary>
        public IReadOnlyList<AttributeDefinition> Attributes { get; protected set; }

        protected AttributeHost(Location location, IReadOnlyList<AttributeDefinition> attributes) : base(location)
        {
            Attributes = attributes;
        }
    }

    /// <summary>
    /// Base type for named AST declaration.
    /// Statements can host Igor attributes.
    /// </summary>
    public abstract class Statement : AttributeHost, ISymbolDeclaration
    {
        /// <summary>
        /// Name of the entity defined by this declaration
        /// </summary>
        public SymbolName Name { get; }

        /// <summary>
        /// String representation of this AST, for debug purposes
        /// </summary>
        /// <returns>String representation of this AST</returns>
        public override string ToString() => Name.Name;

        protected Statement(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name) : base(location, attributes)
        {
            Name = name;
        }

        public virtual void Compile(CompileContext context, CompileStage stage)
        {
        }
    }

    /// <summary>
    /// Module AST
    /// </summary>
    public class Module : Statement
    {
        /// <summary>
        /// List of top-level definitions (forms)
        /// </summary>
        public IReadOnlyList<Form> Definitions { get; }

        public override IEnumerable<IAttributeHost> NestedHosts => Definitions;

        public Module(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<Form> definitions) : base(location, attributes, name)
        {
            Definitions = definitions;
            InnerScope = new FallbackScope(MemberTable, ImportScope);
        }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    foreach (var definition in Definitions)
                    {
                        definition.Module = this;
                        MemberTable.Register(definition, context);
                    }
                    break;
                case CompileStage.Stage2:
                    ImportScope.AddScope(context.BuiltInScope);
                    ImportScope.AddScopes(CompilationUnit.Usings.Select(use => use.Value?.MemberTable).WhereNotNull());
                    break;
            }
            Definitions.ForEach(d => d.Compile(context, stage));
        }

        public SymbolTable MemberTable { get; } = new SymbolTable();
        public UnionScope ImportScope { get; } = new UnionScope();
        public IScope InnerScope { get; }
        public CompilationUnit CompilationUnit { get; set; }
    }

    /// <summary>
    /// Base AST type for top-level declarations (forms) contained in modules: enums, records, services, etc.
    /// </summary>
    public abstract class Form : Statement
    {
        public override IAttributeHost ScopeHost => Module;

        /// <summary>
        /// AST of module in which this form is declared
        /// </summary>
        public Module Module { get; set; }

        protected Form(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name) : base(location, attributes, name)
        {
        }
    }

    /// <summary>
    /// Base AST type for type declarations
    /// </summary>
    public abstract class TypeForm : Form, IType
    {
        public IAttributeHost TypeHost => this;
        public IReadOnlyList<GenericTypeVariable> Args { get; }
        public int Arity => Args?.Count ?? 0;
        protected TypeForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args) : base(location, attributes, name)
        {
            Args = args;
        }

        public SymbolTable ArgsTable { get; } = new SymbolTable();

        public IScope InnerScope { get; private set; }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    foreach (var arg in Args)
                    {
                        ArgsTable.Register(arg, context);
                    }
                    InnerScope = new FallbackScope(ArgsTable, Module.InnerScope);
                    break;
            }
        }
    }

    /// <summary>
    /// Enum field declaration AST
    /// </summary>
    public class EnumField : Statement
    {
        public override IAttributeHost ScopeHost => Enum;

        /// <summary>
        /// Integer value of enum field
        /// </summary>
        public long? UserValue { get; }

        public long Value { get; set; }

        /// <summary>
        /// Enum AST this field belongs to
        /// </summary>
        public EnumForm Enum { get; internal set; }

        public EnumField(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, long? userValue) : base(location, attributes, name)
        {
            UserValue = userValue;
        }
    }

    /// <summary>
    /// Enum type declaration AST
    /// </summary>
    public class EnumForm : TypeForm
    {
        /// <summary>
        /// The list of enum fields
        /// </summary>
        public IReadOnlyList<EnumField> Fields { get; internal set; }

        public override IEnumerable<IAttributeHost> NestedHosts => Fields;

        public EnumForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<EnumField> fields) : base(location, attributes, name, Array.Empty<GenericTypeVariable>())
        {
            Fields = fields;
        }

        public SymbolTable MemberTable { get; } = new SymbolTable();

        public override void Compile(CompileContext context, CompileStage stage)
        {
            base.Compile(context, stage);
            switch (stage)
            {
                case CompileStage.Stage1:
                    long index = 1;
                    foreach (var field in Fields)
                    {
                        field.Enum = this;
                        MemberTable.Register(field, context);
                        field.Value = field.UserValue ?? index;
                        index = field.Value + 1;
                    }
                    break;
            }
            Fields.ForEach(f => f.Compile(context, stage));
        }
    }

    /// <summary>
    /// Record, variant or interface field declaration AST
    /// </summary>
    public class RecordFieldDeclaration : Statement
    {
        public RecordFieldDeclaration(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, ITypeReference typeReference, ValueReference defaultValueReference, bool isTag) : base(location, attributes, name)
        {
            TypeReference = typeReference;
            DefaultValueReference = defaultValueReference;
            IsTag = isTag;
        }

        public override IAttributeHost ScopeHost => Struct;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        /// <summary>
        /// Type AST of this field
        /// </summary>
        public ITypeReference TypeReference { get; }

        public IType Type { get; private set; }

        /// <summary>
        /// Default value of the field
        /// </summary>
        public ValueReference DefaultValueReference { get; }

        /// <summary>
        /// Is the field a variant tag?
        /// When deserializing variants, tag field is deserialized first, to define which record it is.
        /// </summary>
        public bool IsTag { get; }

        /// <summary>
        /// Struct (record, interface or variant) this field belongs to.
        /// </summary>
        public StructForm Struct { get; internal set; }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage2:
                    TypeReference.Resolve(Struct.InnerScope, context);
                    Type = TypeReference.ResolvedType;
                    break;

                case CompileStage.Stage3:
                    if (DefaultValueReference != null && Type != null)
                        DefaultValueReference.Resolve(Type, context);
                    break;
            }
        }
    }

    public class RecordField : Statement
    {
        public StructForm Struct { get; }
        public IReadOnlyList<RecordField> InterfaceDeclarations { get; }
        public RecordField InheritedDeclaration { get; }

        public RecordField(StructForm ownerStruct, Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IType type, Value defaultValue, bool isTag, bool isLocal, RecordField inheritedDeclaration, IReadOnlyList<RecordField> interfaceDeclarations) : base(location, attributes, name)
        {
            Struct = ownerStruct;
            Type = type;
            DefaultValue = defaultValue;
            IsLocal = isLocal;
            IsTag = isTag;
            InheritedDeclaration = inheritedDeclaration;
            InterfaceDeclarations = interfaceDeclarations;
        }

        public override IAttributeHost ScopeHost => Struct;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        public IType Type { get; }

        public Value DefaultValue { get; }

        public bool IsTag { get; }

        /// <summary>
        /// Is the field local (not inherited)?
        /// </summary>
        public bool IsLocal { get; }

        public bool IsInherited => InheritedDeclaration != null;
    }


    /// <summary>
    /// Base AST type for struct declarations: records, variants and interfaces
    /// </summary>
    public abstract class StructForm : TypeForm
    {
        protected StructForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, IReadOnlyList<RecordFieldDeclaration> fieldDeclarations, SymbolReference<VariantForm> ancestor, IReadOnlyList<InterfaceReference> interfaces, bool isException) : base(location, attributes, name, args)
        {
            LocalFieldDeclarations = fieldDeclarations;
            AncestorReference = ancestor;
            InterfaceReferences = interfaces;
            IsException = isException;
        }

        public override IAttributeHost InheritedHost => AncestorReference.Value;

        public IReadOnlyList<RecordFieldDeclaration> LocalFieldDeclarations { get; }
        public IReadOnlyList<RecordField> Fields => fields;


        public override IEnumerable<IAttributeHost> NestedHosts => LocalFieldDeclarations;

        /// <summary>
        /// Ancestor variant (or null if there's no ancestor)
        /// </summary>
        public SymbolReference<VariantForm> AncestorReference { get; }

        public VariantForm Ancestor => AncestorReference?.Value;

        /// <summary>
        /// List of interfaces this struct implements
        /// </summary>
        public IReadOnlyList<InterfaceReference> InterfaceReferences { get; }

        public IReadOnlyList<IInterface> Interfaces { get; private set; }

        /// <summary>
        /// Variant tag field (or null if there's no tag field)
        /// </summary>
        public RecordField TagField { get; private set; }

        /// <summary>
        /// Is this struct an exception?
        /// </summary>
        public bool IsException { get; }

        public SymbolTable MemberTable { get; } = new SymbolTable();

        public override void Compile(CompileContext context, CompileStage stage)
        {
            base.Compile(context, stage);
            switch (stage)
            {
                case CompileStage.Stage1:
                    foreach (var field in LocalFieldDeclarations)
                    {
                        field.Struct = this;
                        MemberTable.Register(field, context);
                    }
                    break;

                case CompileStage.Stage2:
                    foreach (var intf in InterfaceReferences)
                    {
                        intf.Resolve(Module.InnerScope, context);
                    }

                    Interfaces = InterfaceReferences.Select(intf => intf.ResolvedType).ToList();

                    if (AncestorReference != null)
                    {
                        AncestorReference.Resolve(Module.InnerScope, context);
                        AncestorReference.Value.Descendants.Add(this);
                    }
                    break;

                case CompileStage.Stage4:
                    TryCompileInheritance(context);
                    CompileTagField(context);
                    CompileTagValue(context);
                    break;
            }
            LocalFieldDeclarations.ForEach(f => f.Compile(context, stage));
        }

        private bool isCompilingInheritance = false;
        private bool isInheritanceCompiled = false;

        private readonly List<VariantForm> ancestorVariants = new List<VariantForm>();
        private readonly List<RecordField> fields = new List<RecordField>();

        protected void TryCompileInheritance(CompileContext context)
        {
            if (isCompilingInheritance)
                throw new InvalidOperationException();
            if (isInheritanceCompiled)
                return;
            isCompilingInheritance = true;
            CompileInheritance(context);
            isCompilingInheritance = false;
            isInheritanceCompiled = true;
        }

        private class FieldInheritanceInfo
        {
            public string Name { get; }
            public RecordFieldDeclaration Local { get; set; }
            public RecordField Inherited { get; set; }
            public List<(RecordField field, IType type)> Interfaces { get; } = new List<(RecordField, IType)>();

            public FieldInheritanceInfo(string name) => Name = name;
        }

        protected void CompileInheritance(CompileContext context)
        {
            var ancestor = AncestorReference?.Value;
            if (ancestor != null)
            {
                if (ancestor.isCompilingInheritance)
                {
                    context.Output.Error(Location, $"Circular variant dependency involving {Name} and {ancestor.Name}", ProblemCode.CircularVariantDependency);
                    ancestor.Descendants.Remove(this);
                }
                else
                {
                    ancestor.TryCompileInheritance(context);
                    ancestorVariants.Add(ancestor);
                    ancestorVariants.AddRange(ancestor.ancestorVariants);
                }
            }

            List<FieldInheritanceInfo> fieldsInfo = new List<FieldInheritanceInfo>();

            FieldInheritanceInfo GetOrCreateInfo(SymbolName name)
            {
                var result = fieldsInfo.FirstOrDefault(f => f.Name == name.Name);
                if (result == null)
                {
                    result = new FieldInheritanceInfo(name.Name);
                    fieldsInfo.Add(result);
                }

                return result;
            }

            foreach (var variantForm in Enumerable.Reverse(ancestorVariants))
            {
                foreach (var field in variantForm.Fields)
                {
                    GetOrCreateInfo(field.Name).Inherited = field;
                }
            }

            foreach (var intfReference in InterfaceReferences)
            {
                var intf = intfReference.Reference?.Value;
                if (intf != null)
                {
                    if (intf.isCompilingInheritance)
                    {
                        context.Output.Error(Location, $"Circular interface dependency involving {Name} and {intf.Name}", ProblemCode.CircularInterfaceDependency);
                    }
                    else
                    {
                        intf.TryCompileInheritance(context);
                        var genericArgsDictionary = intf.Args == null ? GenericArgsDictionary.Empty : new GenericArgsDictionary(intf, intfReference.ResolvedType as GenericInterfaceInstance);
                        foreach (var field in intf.fields)
                        {
                            var type = genericArgsDictionary.Instantiate(field.Type, context);
                            GetOrCreateInfo(field.Name).Interfaces.Add((field, type));
                        }
                    }
                }
            }

            foreach (var field in LocalFieldDeclarations)
            {
                GetOrCreateInfo(field.Name).Local = field;
            }

            foreach (var fieldInfo in fieldsInfo)
            {
                var location = fieldInfo.Local?.Location ?? fieldInfo.Inherited?.Location ?? fieldInfo.Interfaces[0].field.Location;
                var name = fieldInfo.Local?.Name ?? fieldInfo.Inherited?.Name ?? fieldInfo.Interfaces[0].field.Name;
                var attributes = new List<AttributeDefinition>();
                if (fieldInfo.Local != null)
                    attributes.AddRange(fieldInfo.Local.Attributes);
                if (fieldInfo.Inherited != null)
                    attributes.AddRange(fieldInfo.Inherited.Attributes);
                foreach (var intf in fieldInfo.Interfaces)
                    attributes.AddRange(intf.field.Attributes);
                bool isLocal = fieldInfo.Local != null;
                bool isTag = (fieldInfo.Local?.IsTag ?? false) || (fieldInfo.Inherited?.IsTag ?? false);
                IType type = null;
                if (fieldInfo.Local != null)
                {
                    type = fieldInfo.Local.Type;
                    if (type != null)
                    {
                        if (fieldInfo.Inherited?.Type != null && !Equals(fieldInfo.Local.Type, fieldInfo.Inherited.Type))
                            context.Output.Error(location, $"Field '{Name.Name}.{name.Name}' has type '{type}' which is different from the type '{fieldInfo.Inherited.Type}' inherited from {fieldInfo.Inherited.Struct.Name}", ProblemCode.InheritedFieldTypeMismatch);
                        foreach (var intf in fieldInfo.Interfaces)
                        {
                            if (intf.type != null && !Equals(type, intf.type))
                                context.Output.Error(location, $"Field '{Name.Name}.{name.Name}' has type '{type}' which is different from the type '{intf.type}' defined in implemented interface {intf.field.Struct.Name}", ProblemCode.InterfaceFieldTypeMismatch);
                        }
                    }
                }
                else if (fieldInfo.Inherited != null)
                {
                    type = fieldInfo.Inherited.Type;
                    if (type != null)
                    {
                        foreach (var intf in fieldInfo.Interfaces)
                        {
                            if (intf.type != null && !Equals(type, intf.type))
                                context.Output.Error(Location, $"Field '{Name.Name}.{name.Name}' inherited from {fieldInfo.Inherited.Struct.Name} has type '{type}' which is different from the type '{intf.type}' defined in implemented interface {intf.field.Struct.Name}", ProblemCode.InheritedFieldTypeMismatch);
                        }
                    }
                }
                else if (fieldInfo.Interfaces.Any())
                {
                    type = fieldInfo.Interfaces[0].type;
                    if (type != null)
                    {
                        var implementedIn = fieldInfo.Interfaces[0].field.Struct;
                        foreach (var intf in fieldInfo.Interfaces)
                        {
                            if (intf.type != null && !Equals(type, intf.type))
                                context.Output.Error(Location, $"Field '{Name.Name}.{name.Name}' defined in implemented interface {implementedIn.Name} has type '{type}' which is different from the type '{intf.type}' defined in implemented interface {intf.field.Struct.Name}", ProblemCode.InterfaceFieldTypeMismatch);
                        }
                    }
                }

                var value = fieldInfo.Local?.DefaultValueReference?.ResolvedValue ?? fieldInfo.Inherited?.DefaultValue ?? fieldInfo.Interfaces.FirstOrDefault().field?.DefaultValue;
                var interfaces = fieldInfo.Interfaces.Select(t => t.field).ToList();
                fields.Add(new RecordField(this, location, attributes, name, type, value, isTag, isLocal, fieldInfo.Inherited, interfaces));
            }
        }

        public void CompileTagField(CompileContext context)
        {
            var tagFields = Fields.Where(f => f.IsTag).ToList();
            TagField = tagFields.FirstOrDefault();
            switch (this)
            {
                case InterfaceForm _:
                    foreach (var tagField in tagFields)
                    {
                        context.Output.Error(tagField.Location, "Tag fields are not allowed in interfaces", ProblemCode.TagFieldNotAllowedInInterfaces);
                    }
                    break;
                case RecordForm _:
                    foreach (var tagField in tagFields)
                    {
                        if (!tagField.IsInherited)
                            context.Output.Error(tagField.Location, "Tag fields are not allowed in records", ProblemCode.TagFieldNotAllowedInRecords);
                    }
                    break;
                case VariantForm _ when AncestorReference == null:
                    if (tagFields.Count == 0)
                    {
                        context.Output.Error(Location, "Tag field is required", ProblemCode.TagFieldRequired);
                    }

                    if (tagFields.Count > 1)
                    {
                        context.Output.Error(Location, $"Variant {Name} has more than one tag", ProblemCode.TooManyTagFields);
                    }

                    if (TagField != null && TagField.Type != null && !(TagField.Type is EnumForm))
                    {
                        context.Output.Error(Location, $"Tag field {TagField.Name} must have enum type", ProblemCode.TagTypeMustBeEnum);
                    }
                    break;
                case VariantForm _:
                    foreach (var tagField in tagFields)
                    {
                        if (!tagField.IsInherited)
                            context.Output.Error(tagField.Location, "Tag fields are not allowed in variant descendents", ProblemCode.TagFieldNotAllowedInVariantDescendents);
                    }
                    break;
            }
        }

        protected virtual void CompileTagValue(CompileContext context)
        {
        }
    }

    /// <summary>
    /// Record declaration AST
    /// </summary>
    public class RecordForm : StructForm
    {
        public RecordForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, IReadOnlyList<RecordFieldDeclaration> fields, SymbolReference<VariantForm> ancestor, IReadOnlyList<InterfaceReference> interfaces, bool isException, SymbolReference<EnumField> tagValue) : base(location, attributes, name, args, fields, ancestor, interfaces, isException)
        {
            TagValueReference = tagValue;
        }

        /// <summary>
        /// Tag field value for this record 
        /// </summary>
        public SymbolReference<EnumField> TagValueReference { get; }

        public Value TagValue { get; private set; }

        protected override void CompileTagValue(CompileContext context)
        {
            if (TagValueReference == null && AncestorReference != null)
                context.Output.Error(Location, "Tag value is not specified for variant record", ProblemCode.RecordTagRequired);
            else if (TagValueReference != null && AncestorReference == null)
                context.Output.Error(TagValueReference.Location, "Tag value is not allowed in non variant record", ProblemCode.RecordTagNotAllowedInNonVariantRecords);
            if (TagValueReference != null && AncestorReference != null && TagField?.Type is EnumForm enumTag)
            {
                TagValueReference.Resolve(enumTag.MemberTable, context);
                if (TagValueReference.Value != null)
                {
                    TagValue = new Value.Enum(TagValueReference.Location, TagValueReference.Value);
                }
            }
        }
    }

    /// <summary>
    /// Interface declaration AST
    /// </summary>
    public class InterfaceForm : StructForm, IInterface
    {
        public InterfaceForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, IReadOnlyList<RecordFieldDeclaration> fields, IReadOnlyList<InterfaceReference> interfaces) : base(location, attributes, name, args, fields, null, interfaces, false)
        {
        }
    }

    /// <summary>
    /// Variant declaration AST
    /// </summary>
    public class VariantForm : StructForm
    {
        public VariantForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, IReadOnlyList<RecordFieldDeclaration> fields, SymbolReference<VariantForm> ancestor, IReadOnlyList<InterfaceReference> interfaces, bool isException) : base(location, attributes, name, args, fields, ancestor, interfaces, isException)
        {
        }

        /// <summary>
        /// List of direct descendants (both records and variants)
        /// </summary>
        public IList<StructForm> Descendants { get; } = new List<StructForm>();

        /// <summary>
        /// List of all record descendants, both direct and indirect
        /// </summary>
        public IReadOnlyList<RecordForm> Records { get; private set; }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            base.Compile(context, stage);
            if (stage == CompileStage.Stage4)
            {
                GetRecords();
            }
        }

        private IReadOnlyList<RecordForm> GetRecords()
        {
            if (Records == null)
            {
                var result = new List<RecordForm>();
                foreach (var descendant in Descendants)
                {
                    if (descendant is VariantForm variantChild)
                        result.AddRange(variantChild.GetRecords());
                    else if (descendant is RecordForm recordChild)
                        result.Add(recordChild);
                }

                Records = result;
            }

            return Records;
        }
    }

    /// <summary>
    /// Union clause declaration AST
    /// </summary>
    public class UnionClause : Statement
    {
        public override IAttributeHost ParentTypeHost => Type?.TypeHost;
        public override IAttributeHost ScopeHost => Union;

        public ITypeReference TypeReference { get; }
        public IType Type => TypeReference?.ResolvedType;
        public UnionForm Union { get; set; }
        public bool IsSingleton => TypeReference == null;

        public UnionClause(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, ITypeReference typeReference) : base(location, attributes, name)
        {
            TypeReference = typeReference;
        }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage2:
                    TypeReference?.Resolve(Union.InnerScope, context);
                    break;
            }
        }
    }

    /// <summary>
    /// Union type declaration AST
    /// </summary>
    public class UnionForm : TypeForm
    {
        public IReadOnlyList<UnionClause> Clauses { get; internal set; }
        public override IEnumerable<IAttributeHost> NestedHosts => Clauses;

        public SymbolTable MemberTable { get; } = new SymbolTable();

        public UnionForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, IReadOnlyList<UnionClause> clauses) : base(location, attributes, name, args)
        {
            Clauses = clauses;
        }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            base.Compile(context, stage);
            switch (stage)
            {
                case CompileStage.Stage1:
                    foreach (var clause in Clauses)
                    {
                        clause.Union = this;
                        MemberTable.Register(clause, context);
                    }
                    break;
            }
            Clauses.ForEach(f => f.Compile(context, stage));
        }
    }

    /// <summary>
    /// Alias (define) type declaration AST
    /// </summary>
    public class DefineForm : TypeForm
    {
        public override IAttributeHost ParentTypeHost => Type.TypeHost;


        /// <summary>
        /// Alias target type. This define type is defined as an alias for that type.
        /// </summary>
        public ITypeReference TypeReference { get; }

        public IType Type => TypeReference.ResolvedType;

        public DefineForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<GenericTypeVariable> args, ITypeReference typeReference) : base(location, attributes, name, args)
        {
            TypeReference = typeReference;
        }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            base.Compile(context, stage);
            switch (stage)
            {
                case CompileStage.Stage2:
                    TypeReference.Resolve(InnerScope, context);
                    break;
            }
        }
    }

    public class TableFieldReference : Located
    {
        public TableFieldReference(Location location, SymbolReference<TableForm> table, SymbolReference<TableField> field) : base(location)
        {
            Table = table;
            Field = field;
        }

        public SymbolReference<TableForm> Table { get; }
        public SymbolReference<TableField> Field { get; }

        public void Resolve(IScope scope, CompileContext context)
        {
            Table.Resolve(scope, context);
            if (Table.Value != null)
                Field.Resolve(Table.Value.MemberTable, context);
        }
    }

    public class TableField : Statement
    {
        public TableField(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, ITypeReference typeReference, ValueReference defaultValue, TableFieldReference foreignKey) : base(location, attributes, name)
        {
            TypeReference = typeReference;
            DefaultValueReference = defaultValue;
            ForeignKeyReference = foreignKey;
        }

        public override IAttributeHost ScopeHost => Table;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;
        public ITypeReference TypeReference { get; }
        public IType Type => TypeReference.ResolvedType;
        public ValueReference DefaultValueReference { get; }
        public Value DefaultValue => DefaultValueReference?.ResolvedValue;
        public TableForm Table { get; set; }

        public TableFieldReference ForeignKeyReference { get; }
        public TableField ForeignKey => ForeignKeyReference?.Field?.Value;

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage2:
                    TypeReference.Resolve(Table.InnerScope, context);
                    ForeignKeyReference?.Resolve(Table.InnerScope, context);
                    break;

                case CompileStage.Stage3:
                    if (DefaultValueReference != null && Type != null)
                        DefaultValueReference.Resolve(Type, context);
                    break;
            }
        }
    }

    public class TableForm : Form
    {
        public IReadOnlyList<TableField> Fields { get; }
        public override IEnumerable<IAttributeHost> NestedHosts => Fields;

        public SymbolTable MemberTable { get; } = new SymbolTable();
        public IScope InnerScope { get; private set; }

        public TableForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<TableField> fields) : base(location, attributes, name)
        {
            Fields = fields;
        }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    InnerScope = Module.InnerScope;
                    foreach (var field in Fields)
                    {
                        field.Table = this;
                        MemberTable.Register(field, context);
                    }
                    break;
            }
            Fields.ForEach(f => f.Compile(context, stage));
        }
    }

    /// <summary>
    /// Service function argument AST
    /// </summary>
    public class FunctionArgument : AttributeHost, ISymbolDeclaration
    {
        public FunctionArgument(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, ITypeReference typeReference) : base(location, attributes)
        {
            Name = name;
            TypeReference = typeReference;
        }

        /// <summary>
        /// Argument name
        /// </summary>
        public SymbolName Name { get; }

        /// <summary>
        /// Argument type
        /// </summary>
        public ITypeReference TypeReference { get; }

        public IType Type { get; private set; }

        /// <summary>
        /// Function AST this argument belongs to
        /// </summary>
        public ServiceFunction Function { get; internal set; }

        /// <summary>
        /// Zero-based index of this argument
        /// </summary>
        public int Index { get; internal set; }

        public override string ToString() => Name.Name;

        public void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage2:
                    TypeReference.Resolve(Function.Service.Module.InnerScope, context);
                    Type = TypeReference.ResolvedType;
                    break;
            }
        }
    }

    /// <summary>
    /// AST of reference to exception thrown by service functions
    /// </summary>
    public class FunctionThrow : Located
    {
        public FunctionThrow(Location location, ITypeReference exceptionReference) : base(location)
        {
            ExceptionReference = exceptionReference;
        }

        /// <summary>
        /// Thrown exception AST exception record
        /// </summary>
        public ITypeReference ExceptionReference { get; }
        public RecordForm Exception { get; private set; }

        /// <summary>
        /// Unique integer ID for serialization
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Function AST
        /// </summary>
        public ServiceFunction Function { get; internal set; }

        public void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage2:
                    ExceptionReference.Resolve(Function.Service.Module.InnerScope, context);
                    if (ExceptionReference.ResolvedType != null)
                    {
                        if (ExceptionReference.ResolvedType is RecordForm rec && rec.IsException)
                            Exception = rec;
                        else
                            context.Output.Error(ExceptionReference.Location, "Thrown type must be exception", ProblemCode.ThrownTypeMustBeException);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Service function declaration AST
    /// </summary>
    public class ServiceFunction : Statement
    {
        public ServiceFunction(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, Direction direction, IReadOnlyList<FunctionArgument> arguments, IReadOnlyList<FunctionArgument> returnArguments, IReadOnlyList<FunctionThrow> throws, bool isRpc) : base(location, attributes, name)
        {
            Direction = direction;
            Arguments = arguments;
            ReturnArguments = returnArguments;
            Throws = throws;
            IsRpc = isRpc;
        }

        public override IAttributeHost ScopeHost => Service;

        /// <summary>
        /// Function direction (c->s or s->c)
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// List of function arguments
        /// </summary>
        public IReadOnlyList<FunctionArgument> Arguments { get; }

        /// <summary>
        /// List of returned result arguments. Null if function is not an RPC.
        /// </summary>
        public IReadOnlyList<FunctionArgument> ReturnArguments { get; }

        /// <summary>
        /// List of thrown exception references
        /// </summary>
        public IReadOnlyList<FunctionThrow> Throws { get; }

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
        public bool IsRpc { get; }

        public SymbolTable ArgumentsScope { get; } = new SymbolTable();
        public SymbolTable ReturnArgumentsScope { get; } = new SymbolTable();

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    {
                        int index = 0;
                        foreach (var argument in Arguments)
                        {
                            argument.Index = index;
                            argument.Function = this;
                            ArgumentsScope.Register(argument, context);
                            index++;
                        }

                        if (ReturnArguments != null)
                        {
                            index = 0;
                            foreach (var argument in ReturnArguments)
                            {
                                argument.Index = index;
                                argument.Function = this;
                                ReturnArgumentsScope.Register(argument, context);
                                index++;
                            }
                        }

                        if (Throws != null)
                        {
                            index = 1;
                            foreach (var thr in Throws)
                            {
                                thr.Id = index;
                                thr.Function = this;
                                index++;
                            }
                        }
                    }
                    break;
            }
            Arguments.ForEach(arg => arg.Compile(context, stage));
            ReturnArguments?.ForEach(arg => arg.Compile(context, stage));
            Throws?.ForEach(thr => thr.Compile(context, stage));
        }
    }

    /// <summary>
    /// Service declaration AST
    /// </summary>
    public class ServiceForm : Form
    {
        public ServiceForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<ServiceFunction> functions) : base(location, attributes, name)
        {
            Functions = functions;
        }

        /// <summary>
        /// List of functions
        /// </summary>
        public IReadOnlyList<ServiceFunction> Functions { get; }

        public override IEnumerable<IAttributeHost> NestedHosts => Functions;

        public SymbolTable ClientToServerScope { get; } = new SymbolTable();
        public SymbolTable ServerToClientScope { get; } = new SymbolTable();

        public SymbolTable InnerScope(Direction direction) => direction == Direction.ClientToServer ? ClientToServerScope : ServerToClientScope;

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    var index = 0;
                    foreach (var function in Functions)
                    {
                        function.Service = this;
                        function.Index = index;
                        index++;
                        InnerScope(function.Direction).Register(function, context);
                    }
                    break;
            }
            Functions.ForEach(f => f.Compile(context, stage));
        }
    }

    /// <summary>
    /// Web service declaration AST
    /// </summary>
    public class WebServiceForm : Form
    {
        public WebServiceForm(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, IReadOnlyList<WebResource> resources) : base(location, attributes, name)
        {
            Resources = resources;
        }

        public IReadOnlyList<WebResource> Resources { get; }
        public override IEnumerable<IAttributeHost> NestedHosts => Resources;

        public SymbolTable MemberTable { get; } = new SymbolTable();
        public IScope InnerScope { get; private set; }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    InnerScope = Module.InnerScope;
                    foreach (var resource in Resources)
                    {
                        resource.WebService = this;
                        MemberTable.Register(resource, context);
                    }
                    break;
            }
            Resources.ForEach(f => f.Compile(context, stage));
        }
    }

    /// <summary>
    /// Web resource declaration AST
    /// </summary>
    public class WebResource : Statement
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
        public IReadOnlyList<WebPathSegment> Path { get; }

        /// <summary>
        /// List of URL query parameters
        /// </summary>
        public IReadOnlyList<WebQueryParameter> Query { get; }

        /// <summary>
        /// HTTP method
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// List of possible responses
        /// </summary>
        public IReadOnlyList<WebResponse> Responses { get; }

        /// <summary>
        /// List of request headers
        /// </summary>
        public IReadOnlyList<WebHeader> RequestHeaders { get; }

        /// <summary>
        /// Request content AST, if available. Can be null if request has no body.
        /// </summary>
        public WebContent RequestContent { get; }

        /// <summary>
        /// Request body type AST. Can be null if request has no body.
        /// </summary>
        public IType RequestBodyType => RequestContent?.Type;

        public IType ResponseBodyType => Responses.FirstOrDefault()?.Content?.Type;

        public IEnumerable<WebVariable> PathVariables => Path.Where(path => path.Var != null).Select(v => v.Var);
        public IEnumerable<WebVariable> QueryVariables => Query.Where(v => v.Var != null).Select(v => v.Var);
        public IEnumerable<WebVariable> RequestHeadersVariables => RequestHeaders.Where(h => h.Var != null).Select(v => v.Var);
        public IEnumerable<WebVariable> RequestVariables => PathVariables.Concat(QueryVariables).Concat(RequestHeadersVariables);

        public WebResource(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, HttpMethod method, IReadOnlyList<WebPathSegment> path, IReadOnlyList<WebQueryParameter> query, IReadOnlyList<WebHeader> requestHeaders, WebContent requestContent, IReadOnlyList<WebResponse> responses) : base(location, attributes, name)
        {
            Method = method;
            Path = path;
            Query = query;
            RequestHeaders = requestHeaders;
            RequestContent = requestContent;
            Responses = responses;
        }

        public SymbolTable RequestMemberTable { get; } = new SymbolTable();
        public IScope InnerScope { get; private set; }

        public override void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    InnerScope = WebService.InnerScope;
                    foreach (var response in Responses)
                        response.Resource = this;
                    break;
            }

            CompileRequest(context, stage);
            foreach (var response in Responses)
                response.Compile(context, stage);
        }

        private void CompileRequest(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    {
                        foreach (var variable in RequestVariables)
                        {
                            RequestMemberTable.Register(variable, context);
                            variable.Resource = this;
                        }

                        if (RequestContent != null)
                            RequestContent.Resource = this;

                        if (RequestContent?.Var != null)
                        {
                            RequestMemberTable.Register(RequestContent.Var, context);
                            RequestContent.Var.Resource = this;
                        }
                    }
                    break;

                case CompileStage.Stage2:
                    {
                        foreach (var variable in RequestVariables)
                        {
                            variable.TypeReference.Resolve(InnerScope, context);
                        }

                        if (RequestContent != null)
                        {
                            if (RequestContent.Var != null)
                            {
                                RequestContent.Var.TypeReference.Resolve(InnerScope, context);
                            }
                            else
                            {
                                RequestContent.TypeReference.Resolve(InnerScope, context);
                            }
                        }
                    }
                    break;


                case CompileStage.Stage3:
                    {
                        foreach (var variable in RequestVariables)
                        {
                            variable.DefaultValueReference?.Resolve(variable.Type, context);
                        }

                        if (RequestContent?.Var?.Type != null)
                        {
                            RequestContent.Var.DefaultValueReference?.Resolve(RequestContent.Var.Type, context);
                        }
                    }
                    break;
            }
        }
    }

    public class WebStatusCode : Located
    {
        public WebStatusCode(Location location, int code, string reasonPhrase) : base(location)
        {
            Code = code;
            ReasonPhrase = reasonPhrase;
        }

        public int Code { get; }
        public string ReasonPhrase { get; }
    }

    public class WebResponse : AttributeHost
    {
        public WebResponse(Location location, IReadOnlyList<AttributeDefinition> attributes, IReadOnlyList<WebHeader> headers, WebContent content, WebStatusCode status) : base(location, attributes)
        {
            Headers = headers;
            Content = content;
            Status = status;
        }

        public IReadOnlyList<WebHeader> Headers { get; }
        public WebContent Content { get; }
        public WebStatusCode Status { get; }
        public int StatusCode => Status?.Code ?? 200;

        public IEnumerable<WebVariable> HeadersVariables => Headers.Where(v => v.Var != null).Select(v => v.Var);

        public WebResource Resource { get; internal set; }
        public SymbolTable MemberTable { get; } = new SymbolTable();
        public IScope InnerScope { get; private set; }

        public void Compile(CompileContext context, CompileStage stage)
        {
            switch (stage)
            {
                case CompileStage.Stage1:
                    {
                        InnerScope = Resource.InnerScope;
                        foreach (var variable in HeadersVariables)
                        {
                            variable.Resource = Resource;
                            MemberTable.Register(variable, context);
                        }

                        if (Content != null)
                            Content.Resource = Resource;

                        if (Content?.Var != null)
                        {
                            Content.Var.Resource = Resource;
                            MemberTable.Register(Content.Var, context);
                        }
                    }
                    break;

                case CompileStage.Stage2:
                    {
                        foreach (var variable in HeadersVariables)
                        {
                            variable.TypeReference.Resolve(InnerScope, context);
                        }

                        if (Content != null)
                        {
                            if (Content.Var != null)
                            {
                                Content.Var.TypeReference.Resolve(InnerScope, context);
                            }
                            else
                            {
                                Content.TypeReference.Resolve(InnerScope, context);
                            }
                        }

                    }
                    break;

                case CompileStage.Stage3:
                    {
                        foreach (var variable in HeadersVariables)
                        {
                            variable.DefaultValueReference?.Resolve(variable.Type, context);
                        }

                        if (Content?.Var?.Type != null)
                        {
                            Content.Var.DefaultValueReference?.Resolve(Content.Var.Type, context);
                        }
                    }
                    break;
            }
        }
    }

    public class WebContent : Located
    {
        public IType Type => Var == null ? TypeReference.ResolvedType : Var.Type;
        public DataFormat Format { get; }
        public WebVariable Var { get; }
        public ITypeReference TypeReference { get; }
        public WebResource Resource { get; internal set; }

        public WebContent(Location location, DataFormat format, WebVariable variable) : base(location)
        {
            Format = format;
            Var = variable;
        }

        public WebContent(Location location, DataFormat format, ITypeReference type) : base(location)
        {
            Format = format;
            TypeReference = type;
        }
    }

    public class WebHeader
    {
        public string Name { get; }
        public string StaticValue { get; }
        public WebVariable Var { get; }

        public WebHeader(string name, string value)
        {
            Name = name;
            StaticValue = value;
        }

        public WebHeader(string name, WebVariable var)
        {
            Name = name;
            Var = var;
        }
    }

    public class WebPathSegment
    {
        public string StaticValue { get; }
        public WebVariable Var { get; }

        public WebPathSegment(string staticValue) => StaticValue = staticValue;

        public WebPathSegment(WebVariable variable) => Var = variable;
    }

    public class WebQueryParameter
    {
        public string Parameter { get; }
        public string StaticValue { get; }
        public WebVariable Var { get; }
        public DataFormat Format { get; }
        public WebQueryParameter(string parameter, string value)
        {
            Parameter = parameter;
            StaticValue = value;
            Format = DataFormat.Text;
        }

        public WebQueryParameter(string parameter, WebVariable value)
        {
            Parameter = parameter;
            Var = value;
            Format = value.Format;
        }
    }

    public enum WebParameterType
    {
        Content,
        Path,
        Query,
        Header,
    }

    public class WebVariable : Statement
    {
        public override IAttributeHost ScopeHost => Resource;
        public override IAttributeHost ParentTypeHost => Type.TypeHost;

        public ITypeReference TypeReference { get; }
        public IType Type => TypeReference.ResolvedType;
        public ValueReference DefaultValueReference { get; }
        public Value DefaultValue => DefaultValueReference?.ResolvedValue;
        public WebResource Resource { get; internal set; }
        public DataFormat Format { get; }
        public WebParameterType ParameterType { get; }

        public WebVariable(Location location, IReadOnlyList<AttributeDefinition> attributes, SymbolName name, ITypeReference type, ValueReference defaultValue, DataFormat format, WebParameterType parameterType) : base(location, attributes, name)
        {
            TypeReference = type;
            DefaultValueReference = defaultValue;
            Format = format;
            ParameterType = parameterType;
        }
    }

    public abstract class ValueReference : Located, IEquatable<ValueReference>
    {
        public class Bool : ValueReference
        {
            public bool Value { get; }

            public Bool(Location location, bool value) : base(location) => Value = value;

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is Bool v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value ? "true" : "false";

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is BuiltInType.Bool)
                    return new Value.Bool(Location, Value);
                else
                    return null;
            }
        }

        public class Integer : ValueReference
        {
            public long Value { get; }
            public NumberBase Base { get; }

            public Integer(Location location, long value, NumberBase numberBase) : base(location)
            {
                Value = value;
                Base = numberBase;
            }

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is Integer v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is BuiltInType.Integer intType)
                {
                    if (Value >= Primitive.MinValue(intType.Type) && Value <= Primitive.MaxValue(intType.Type))
                        return new Value.Integer(Location, Value);
                    else
                    {
                        ReportConvertError = false;
                        context.Output.Error(Location, $"Value {Value} is outside of {intType.Type.ToString().ToLowerInvariant()} range", ProblemCode.IntegerRangeViolation);
                        return null;
                    }
                }
                else if (type is BuiltInType.Float)
                    return new Value.Float(Location, Value);
                else
                    return null;
            }
        }

        public class Float : ValueReference
        {
            public double Value { get; }

            public Float(Location location, double value) : base(location) => Value = value;

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is Float v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is BuiltInType.Float)
                    return new Value.Float(Location, Value);
                else
                    return null;
            }
        }

        public class String : ValueReference
        {
            public string Value { get; }

            public String(Location location, string value) : base(location) => Value = value;

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is String v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.Quoted();

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is BuiltInType.String)
                    return new Value.String(Location, Value);
                else
                    return null;
            }
        }

        public class EmptyList : ValueReference
        {
            public EmptyList(Location location) : base(location) { }

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is EmptyList;

            public override int GetHashCode() => 1002;

            public override string ToString() => "[]";

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is GenericTypeInstance genericList && genericList.Prototype is BuiltInType.List)
                    return new Value.EmptyList(Location);
                else if (type is GenericTypeInstance genericDict && genericDict.Prototype is BuiltInType.Dict)
                    return new Value.EmptyDict(Location);
                else if (type is BuiltInType.Json)
                    return new Value.EmptyList(Location);
                else
                    return null;
            }
        }

        public class EmptyObject : ValueReference
        {
            public EmptyObject(Location location) : base(location) { }

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is EmptyList;

            public override int GetHashCode() => 1002;

            public override string ToString() => "[]";

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is BuiltInType.Json)
                    return new Value.EmptyObject(Location);
                else
                    return null;
            }
        }

        public class Enum : ValueReference
        {
            public SymbolReference<EnumField> Field { get; }

            public Enum(Location location, SymbolReference<EnumField> field) : base(location) => this.Field = field;

            public override bool Equals(ValueReference other) => ReferenceEquals(this, other) || other is Enum v && v.Field == Field;

            public override int GetHashCode() => Field.GetHashCode();

            public override string ToString() => Field.Name;

            protected override Value ResolveValue(IType type, CompileContext context)
            {
                if (type is EnumForm enumForm)
                {
                    ReportConvertError = false;
                    Field.Resolve(enumForm.MemberTable, context);
                    if (Field.Value != null)
                        return new Value.Enum(Location, Field.Value);
                }
                return null;
            }
        }

        public abstract bool Equals(ValueReference other);

        public override bool Equals(object obj)
        {
            return obj is ValueReference other && Equals(this, other);
        }

        protected ValueReference(Location location) : base(location)
        {
        }

        public override int GetHashCode() => base.GetHashCode();

        protected abstract Value ResolveValue(IType type, CompileContext context);

        public void Resolve(IType type, CompileContext context)
        {
            ResolvedValue = ResolveValue(PinDownType(type), context);
            if (ResolvedValue == null && ReportConvertError)
                context.Output.Error(Location, $"Cannot convert value '{this}' to target type '{type}'", ProblemCode.CannotConvertValueToType);
        }

        private IType PinDownType(IType type)
        {
            {
                if (type is GenericTypeInstance generic && generic.Prototype is BuiltInType.Optional && generic.Args.Any())
                    return PinDownType(generic.Args[0]);
            }
            {
                if (type is GenericTypeInstance generic && generic.Prototype is DefineForm defineForm && generic.Args.Any())
                    return PinDownType(generic.Prototype);
            }
            {
                if (type is DefineForm defineForm && defineForm.Type != null)
                    return PinDownType(defineForm.Type);
            }
            return type;
        }

        protected bool ReportConvertError = true;

        public Value ResolvedValue { get; private set; }
    }

    public abstract class Value : Located, IEquatable<Value>
    {
        public class Bool : Value
        {
            public bool Value { get; }

            public Bool(Location location, bool value) : base(location) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Bool v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value ? "true" : "false";
        }

        public class Integer : Value
        {
            public long Value { get; }

            public Integer(Location location, long value) : base(location) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Integer v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class Float : Value
        {
            public double Value { get; }

            public Float(Location location, double value) : base(location) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Float v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class String : Value
        {
            public string Value { get; }

            public String(Location location, string value) : base(location) => Value = value;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is String v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.Quoted();
        }

        public class EmptyList : Value
        {
            public EmptyList(Location location) : base(location) { }

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is EmptyList;

            public override int GetHashCode() => 1002;

            public override string ToString() => "[]";
        }

        public class EmptyDict : Value
        {
            public EmptyDict(Location location) : base(location) { }

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is EmptyDict;

            public override int GetHashCode() => 1002;

            public override string ToString() => "[]";
        }

        public class EmptyObject : Value
        {
            public EmptyObject(Location location) : base(location) { }

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is EmptyObject;

            public override int GetHashCode() => 1002;

            public override string ToString() => "{}";
        }

        public class Enum : Value
        {
            public EnumField Field { get; }

            public Enum(Location location, EnumField field) : base(location) => this.Field = field;

            public override bool Equals(Value other) => ReferenceEquals(this, other) || other is Enum v && v.Field == Field;

            public override int GetHashCode() => Field.GetHashCode();

            public override string ToString() => Field.Name.ToString();
        }

        public abstract bool Equals(Value other);

        public override bool Equals(object obj)
        {
            return obj is Value other && Equals(this, other);
        }

        protected Value(Location location) : base(location)
        {
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public class GenericTypeVariable : Located, ISymbolDeclaration, IType
    {
        /// <summary>
        /// Name of generic type variable
        /// </summary>
        public SymbolName Name { get; }

        public int Arity => 0;

        public GenericTypeVariable(Location location, SymbolName name) : base(location)
        {
            Name = name;
        }

        public IAttributeHost TypeHost => null;
    }

    public class InterfaceReference : Located
    {
        public SymbolReference<InterfaceForm> Reference { get; }
        public IReadOnlyList<ITypeReference> Args { get; }
        public IInterface ResolvedType { get; private set; }

        public InterfaceReference(Location location, SymbolReference<InterfaceForm> reference, IReadOnlyList<ITypeReference> args) : base(location)
        {
            Reference = reference;
            Args = args;
        }

        public void Resolve(IScope scope, CompileContext context)
        {
            Reference.Resolve(scope, context);
            if (Args != null)
            {
                foreach (var arg in Args)
                {
                    arg.Resolve(scope, context);
                }
                ResolvedType = new GenericInterfaceInstance(Location, Reference.Value, Args.Select(arg => arg.ResolvedType).ToList());
            }
            else
            {
                ResolvedType = Reference.Value;
            }
        }
    }

    public interface ITypeReference : ILocated
    {
        IType ResolvedType { get; }
        bool Resolve(IScope scope, CompileContext context);
    }

    public class TypeReference : Located, ITypeReference
    {
        public bool IsOptional { get; }

        public IType ResolvedType { get; private set; }

        public TypeReference(Location location, SymbolReference<IType> reference, IReadOnlyList<ITypeReference> args, bool isOptional) : base(location)
        {
            Reference = reference;
            Args = args;
            IsOptional = isOptional;
        }

        public SymbolReference<IType> Reference { get; }
        public IReadOnlyList<ITypeReference> Args { get; }

        public bool Resolve(IScope scope, CompileContext context)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            Reference.Resolve(scope, context);
            IType result = Reference.Value;
            if (result != null)
            {
                if (Args != null)
                {
                    if (Args.Count != result.Arity)
                    {
                        if (result.Arity == 0)
                            context.Output.Error(Location, $"Non-generic type '{result}' cannot be used with type arguments", ProblemCode.NonGenericTypeWithTypeArgs);
                        else
                            context.Output.Error(Location, $"Generic type '{result}' requires {result.Arity} type arguments", ProblemCode.GenericArityMismatch);
                    }

                    foreach (var arg in Args)
                    {
                        arg.Resolve(scope, context);
                    }

                    result = new GenericTypeInstance(Location, result, Args.Select(arg => arg.ResolvedType).ToList());
                }
                else
                {
                    if (result.Arity != 0)
                    {
                        context.Output.Error(Location, $"Generic type '{result}' requires {result.Arity} type arguments", ProblemCode.GenericArgsRequired);
                    }
                }

                if (IsOptional)
                {
                    result = new GenericTypeInstance(Location, BuiltInType.Optional.Instance, new[] { result });
                }
            }

            ResolvedType = result;
            return ResolvedType != null;
        }
    }

    public class OneOfTypeReference : Located, ITypeReference
    {
        public OneOfTypeReference(Location location, IReadOnlyList<ITypeReference> types, bool optional) : base(location)
        {
            Types = types;
            IsOptional = optional;
        }

        public IReadOnlyList<ITypeReference> Types { get; }
        public bool IsOptional { get; }

        public IType ResolvedType { get; private set; }

        public bool Resolve(IScope scope, CompileContext context)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            foreach (var arg in Types)
            {
                arg.Resolve(scope, context);
            }

            ResolvedType = new GenericTypeInstance(Location, new BuiltInType.OneOf(Types.Count), Types.Select(arg => arg.ResolvedType).ToList());

            if (ResolvedType != null && IsOptional)
            {
                ResolvedType = new GenericTypeInstance(Location, BuiltInType.Optional.Instance, new[] { ResolvedType });
            }

            return ResolvedType != null;
        }
    }

    public interface IType : ILocated
    {
        IAttributeHost TypeHost { get; }
        int Arity { get; }
    }

    public interface IInterface : ILocated
    {
        IAttributeHost TypeHost { get; }
        int Arity { get; }
    }

    public class GenericTypeInstance : Located, IType
    {
        public IType Prototype { get; }
        public IReadOnlyList<IType> Args { get; }
        public int Arity => 0;

        public GenericTypeInstance(Location location, IType prototype, IReadOnlyList<IType> args) : base(location)
        {
            Prototype = prototype;
            Args = args;
        }

        public IAttributeHost TypeHost => Prototype.TypeHost;

        public override bool Equals(object obj)
        {
            return obj is GenericTypeInstance other && Equals(Prototype, other.Prototype) && Enumerable.SequenceEqual(Args, other.Args);
        }
    }

    public class GenericInterfaceInstance : Located, IInterface
    {
        public IInterface Prototype { get; }
        public IReadOnlyList<IType> Args { get; internal set; }
        public int Arity => 0;

        public GenericInterfaceInstance(Location location, IInterface prototype, IReadOnlyList<IType> args) : base(location)
        {
            Prototype = prototype;
            Args = args;
        }

        public IAttributeHost TypeHost => Prototype.TypeHost;
    }

    /// <summary>
    /// Igor built-in type instances
    /// </summary>
    public abstract class BuiltInType : Located, IType, ISymbolDeclaration
    {
        /// <summary>
        /// Built-in Igor bool type instance
        /// </summary>
        public class Bool : BuiltInType
        {
            public Bool() : base("bool") { }

            public override bool Equals(object obj) => obj is Bool;
        }

        /// <summary>
        /// Built-in Igor integer type instance
        /// </summary>
        public class Integer : BuiltInType
        {
            public IntegerType Type { get; }

            public Integer(IntegerType type) : base(type.ToString().ToLower()) => this.Type = type;

            public override bool Equals(object obj) => obj is Integer other && Type == other.Type;
        }

        /// <summary>
        /// Built-in Igor float type instance
        /// </summary>
        public class Float : BuiltInType
        {
            public FloatType Type { get; }

            public Float(FloatType type) : base(type.ToString().ToLower()) => this.Type = type;

            public override bool Equals(object obj) => obj is Float other && Type == other.Type;
        }

        /// <summary>
        /// Built-in Igor string type instance
        /// </summary>
        public class String : BuiltInType
        {
            public String() : base("string") { }

            public override bool Equals(object obj) => obj is String;
        }

        /// <summary>
        /// Built-in Igor binary type instance
        /// </summary>
        public class Binary : BuiltInType
        {
            public Binary() : base("binary") { }

            public override bool Equals(object obj) => obj is Binary;
        }

        /// <summary>
        /// Built-in Igor atom type instance
        /// </summary>
        public class Atom : BuiltInType
        {
            public Atom() : base("atom") { }

            public override bool Equals(object obj) => obj is Atom;
        }

        /// <summary>
        /// Built-in Igor json type instance
        /// </summary>
        public class Json : BuiltInType
        {
            public Json() : base("json") { }

            public override bool Equals(object obj) => obj is Json;
        }

        /// <summary>
        /// Built-in Igor list type instance
        /// </summary>
        public class List : BuiltInType
        {
            public List() : base("list") { }
            public override int Arity => 1;

            public override bool Equals(object obj) => obj is List;
        }

        /// <summary>
        /// Built-in Igor dict type instance
        /// </summary>
        public class Dict : BuiltInType
        {
            public Dict() : base("dict") { }
            public override int Arity => 2;
            public override bool Equals(object obj) => obj is Dict;
        }

        /// <summary>
        /// Built-in Igor optional type instance (?T)
        /// </summary>
        public class Optional : BuiltInType
        {
            public Optional() : base("?") { }
            public override int Arity => 1;

            public static readonly Optional Instance = new Optional();
            public override bool Equals(object obj) => obj is Optional;
        }

        /// <summary>
        /// Built-in Igor flags type instance
        /// </summary>
        public class Flags : BuiltInType
        {
            public Flags() : base("flags") { }
            public override int Arity => 1;
            public override bool Equals(object obj) => obj is Flags;
        }

        public class OneOf : BuiltInType
        {
            int arity;
            public OneOf(int arity) : base("oneof") { this.arity = arity; }
            public override int Arity => arity;
            public override bool Equals(object obj) => obj is OneOf other && other.Arity == Arity;
        }

        public virtual IAttributeHost TypeHost => null;
        public virtual int Arity => 0;

        protected BuiltInType(string name) : base(Location.NoLocation)
        {
            Name = new SymbolName(Location.NoLocation, name);
        }
        public SymbolName Name { get; }

        public override string ToString() => Name.Name;
    }
}
