using Igor.Schema.AST;
using Json;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Schema
{
    public static class Helper
    {
        public static string InterfaceName(IInterface intf)
        {
            switch (intf)
            {
                case InterfaceForm intfForm: return intfForm.Name;
                case GenericInterface genericIntf: return InterfaceName(genericIntf.Prototype);
                default: return null;
            }
        }

        private static bool? BoolValue(Value value) => (value as Value.Bool)?.Value;

        private static int? IntValue(Value value) => (int?)(value as Value.Integer)?.Value;

        private static double? FloatValue(Value value) => (value as Value.Float)?.Value ?? IntValue(value);

        private static string StringValue(Value value) => (value as Value.String)?.Value;

        public static string EnumValue(Value value) => (value as Value.Enum)?.Field?.Name;

        public static ImmutableJson ListValue(Value value) => value is Value.List ? ImmutableJson.EmptyArray : null;

        public static ImmutableJson DictValue(Value value) => value is Value.List || value is Value.Dict ? ImmutableJson.EmptyObject : null;

        public static DescriptorKind TypeDescriptorKind(IType type)
        {
            switch (type)
            {
                case BuiltInType.Bool _: return DescriptorKind.Bool;
                case BuiltInType.Integer _: return DescriptorKind.Int;
                case BuiltInType.Float _: return DescriptorKind.Float;
                case BuiltInType.String _: return DescriptorKind.String;
                case BuiltInType.Binary _: return DescriptorKind.Binary;
                case BuiltInType.Atom _: return DescriptorKind.String;
                case BuiltInType.List _: return DescriptorKind.List;
                case BuiltInType.Dict _: return DescriptorKind.Dict;
                case BuiltInType.Optional opt: return TypeDescriptorKind(opt.ItemType);
                case BuiltInType.Flags _: return DescriptorKind.List;
                case BuiltInType.Json _: return DescriptorKind.Json;
                case TypeForm typeForm: return typeForm.schemaDescriptorKind;
                case GenericArgument _: return DescriptorKind.GenericArgument;
                case GenericType genericType:
                    {
                        if (genericType.Prototype is DefineForm)
                            return TypeDescriptorKind(genericType.Prototype);
                        else
                            return DescriptorKind.GenericInstance;
                    }
                default:
                    throw new EUnknownType(type.ToString());
            }
        }

        public static IntTypeName? IntTypeName(IType type)
        {
            switch (type)
            {
                case BuiltInType.Integer t:
                    switch (t.Type)
                    {
                        case IntegerType.Byte: return Igor.Schema.IntTypeName.Uint8;
                        case IntegerType.SByte: return Igor.Schema.IntTypeName.Int8;
                        case IntegerType.UShort: return Igor.Schema.IntTypeName.Uint16;
                        case IntegerType.Short: return Igor.Schema.IntTypeName.Int16;
                        case IntegerType.UInt: return Igor.Schema.IntTypeName.Uint32;
                        case IntegerType.Int: return Igor.Schema.IntTypeName.Int32;
                        case IntegerType.ULong: return Igor.Schema.IntTypeName.Uint64;
                        case IntegerType.Long: return Igor.Schema.IntTypeName.Int64;
                        default: return null;
                    }
                case BuiltInType.Optional opt: return IntTypeName(opt.ItemType);
                case TypeForm typeForm: return typeForm.schemaIntTypeName;
                case GenericType genericType:
                {
                    if (genericType.Prototype is DefineForm)
                        return IntTypeName(genericType.Prototype);
                    else
                        return null;
                }
                default:
                    return null;
            }
        }

        public static FloatTypeName? FloatTypeName(IType type)
        {
            switch (type)
            {
                case BuiltInType.Float t:
                    switch (t.Type)
                    {
                        case FloatType.Float: return Igor.Schema.FloatTypeName.Float32;
                        case FloatType.Double: return Igor.Schema.FloatTypeName.Float64;
                        default: return null;
                    }
                case BuiltInType.Optional opt: return FloatTypeName(opt.ItemType);
                case TypeForm typeForm: return typeForm.schemaFloatTypeName;
                case GenericType genericType:
                    {
                        if (genericType.Prototype is DefineForm)
                            return FloatTypeName(genericType.Prototype);
                        else
                            return null;
                    }
                default:
                    return null;
            }
        }

        public static bool? IsLowCardinality(IType type)
        {
            switch (type)
            {
                case BuiltInType.Atom _: return true;
                case BuiltInType.Optional opt: return IsLowCardinality(opt.ItemType);
                case DefineForm defineForm: return IsLowCardinality(defineForm.Type);
                case GenericType genericType:
                    {
                        if (genericType.Prototype is DefineForm)
                            return IsLowCardinality(genericType.Prototype);
                        else
                            return null;
                    }
                default:
                    return null;
            }
        }

        private static Descriptor TypeDescriptor(IType type, Statement host, Dictionary<string, Descriptor> genericArgs)
        {
            return TypeDescriptor(TypeDescriptorKind(type), type, null, host, genericArgs, false);
        }

        private static Statement GetHost(IType t)
        {
            switch (t)
            {
                case TypeForm typeForm: return typeForm;
                case BuiltInType.Optional opt: return GetHost(opt.ItemType);
                default: return null;
            }
        }

        public static Descriptor TypeDescriptor(DescriptorKind kind, IType type, Value @default, Statement host, Dictionary<string, Descriptor> genericArgs, bool optional)
        {
            if (type is BuiltInType.Optional opt)
            {
                return TypeDescriptor(kind, opt.ItemType, @default, host, genericArgs, true);
            }
            if (type is DefineForm defineForm)
            {
                return TypeDescriptor(kind, defineForm.Type, @default, host, genericArgs, optional);
            }
            if (type is GenericType genericInstance && genericInstance.Prototype is DefineForm genericAlias)
            {
                var newArgs = genericArgs == null ? new Dictionary<string, Descriptor>() : new Dictionary<string, Descriptor>(genericArgs);
                for (int i = 0; i < genericAlias.Args.Count; i++)
                {
                    newArgs[genericAlias.Args[i].Name] = TypeDescriptor(genericInstance.Args[i], GetHost(genericInstance.Args[i]) ?? host, genericArgs);
                }
                return TypeDescriptor(kind, genericAlias.Type, @default, GetHost(genericInstance) ?? host, newArgs, optional);
            }

            switch (kind)
            {
                case DescriptorKind.Bool:
                    return new BoolDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, BoolValue(@default));

                case DescriptorKind.Int:
                    return new IntDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, IntValue(@default), host.intMin, host.intMax, IntTypeName(type));

                case DescriptorKind.Float:
                    return new FloatDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, FloatValue(@default), host.floatMin, host.floatMax, FloatTypeName(type));

                case DescriptorKind.String:
                    return new StringDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, StringValue(@default), host.schemaMultiline, host.schemaNotEmpty, IsLowCardinality(type), host.schemaSource, host.schemaPathOptions, host.schemaSyntax);

                case DescriptorKind.Binary:
                    return new BinaryDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta);

                case DescriptorKind.List:
                    if (type is BuiltInType.List list)
                    {
                        var t = list.ItemType;
                        return new ListDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, TypeDescriptor(t, GetHost(t) ?? host, genericArgs), ListValue(@default));
                    }
                    else if (type is BuiltInType.Flags flags)
                    {
                        var t = flags.ItemType;
                        return new ListDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, TypeDescriptor(t, GetHost(t) ?? host, genericArgs));
                    }
                    break;

                case DescriptorKind.Dict:
                    if (type is BuiltInType.Dict dict)
                        return new DictDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, TypeDescriptor(dict.KeyType, GetHost(dict.KeyType) ?? host, genericArgs), TypeDescriptor(dict.ValueType, GetHost(dict.ValueType) ?? host, genericArgs), DictValue(@default));
                    break;

                case DescriptorKind.Enum:
                    if (type is EnumForm e)
                        return new EnumDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, e.schemaName, EnumValue(@default));
                    break;

                case DescriptorKind.Record:
                    if (type is StructForm s)
                        return new RecordDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, s.schemaName, host.schemaCompact);
                    break;

                case DescriptorKind.Union:
                    if (type is UnionForm u)
                        return new UnionDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, u.schemaName, host.schemaCompact);
                    break;

                case DescriptorKind.Key:
                    if (host.schemaSource != null)
                        return new StringDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, StringValue(@default), host.schemaMultiline, host.schemaNotEmpty, IsLowCardinality(type), host.schemaSource);
                    else
                        return new KeyDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, host.schemaCategory, host.schemaInterface);

                case DescriptorKind.Localized:
                    if (type is StructForm loc)
                        return new LocalizedDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, loc.schemaName, host.schemaMultiline);
                    break;

                case DescriptorKind.Datetime:
                    return new DateTimeDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta);

                case DescriptorKind.Json:
                    return new JsonDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta);

                case DescriptorKind.Custom:
                    return new CustomDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta);

                case DescriptorKind.GenericArgument:
                    if (type is GenericArgument genericArg)
                    {
                        if (genericArgs != null && genericArgs.ContainsKey(genericArg.Name))
                            return genericArgs[genericArg.Name];
                        else
                            return new GenericArgumentDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, genericArg.Name);
                    }
                    break;

                case DescriptorKind.GenericInstance:
                    if (type is GenericType genericTypeInstance)
                    {
                        var prototype = (TypeForm)genericTypeInstance.Prototype;
                        if (prototype is StructForm str)
                        {
                            var args = genericTypeInstance.Args.Select(arg => TypeDescriptor(arg, GetHost(prototype) ?? host, genericArgs)).ToList();
                            return new GenericInstanceDescriptor(optional, host.schemaHelp, host.schemaEditorKey, host.schemaMeta, prototype.schemaName, args);
                        }
                    }
                    break;
            }
            throw new EInternal("Failed to create descriptor for " + type);
        }
    }
}
