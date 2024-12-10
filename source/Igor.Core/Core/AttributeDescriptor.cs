using Igor.Text;
using System;

namespace Igor
{
    /// <summary>
    /// Igor attribute inheritance types
    /// </summary>
    public enum AttributeInheritance
    {
        /// <summary>
        /// No inheritance
        /// </summary>
        None,

        /// <summary>
        /// Scope inheritance. Nested statements inherit attribute value from their scope.
        /// For example, types and services inherit attribute values set for module they are declared in.
        /// Scope inherited attributes can also be provided via command line.
        /// </summary>
        Scope,

        /// <summary>
        /// Type inheritance. Typed declarations (e.g. record fields) inherit attribute values from type declarations.
        /// Alias (define) types inherit attribute values from alias target type.
        /// </summary>
        Type,

        /// <summary>
        /// Variant inheritance. Records inherit attribute values from their ancestor variants.
        /// </summary>
        Inherited,
    }

    /// <summary>
    /// Igor attribute targets. Defines which statements attribute is valid for.
    /// </summary>
    [Flags]
    public enum IgorAttributeTargets
    {
        None,
        Module,
        Record,
        Variant,
        Interface,
        Enum,
        RecordField,
        EnumField,
        Union,
        UnionClause,
        Table,
        TableField,
        Service,
        ServiceFunction,
        WebService,
        WebResource,
        Type,
        Any,
    }

    /// <summary>
    /// Base class for Igor attribute descriptors
    /// </summary>
    public abstract class AttributeDescriptor
    {
        /// <summary>
        /// Attribute name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// C# type
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Attribute inheritance type
        /// </summary>
        public AttributeInheritance Inheritance { get; }

        /// <summary>
        /// Valid attribute targets
        /// </summary>
        public IgorAttributeTargets Targets { get; }

        /// <summary>
        /// If attribute is obsolete, contains deprecation message to be displayed to the user, otherwise contains null
        /// </summary>
        public string DeprecationMessage { get; set; }

        protected AttributeDescriptor(string name, IgorAttributeTargets targets, AttributeInheritance inheritance = AttributeInheritance.None)
        {
            this.Name = name;
            this.Inheritance = inheritance;
            this.Targets = targets;
        }

        public abstract bool ValidateValue(AttributeValue value);

        /// <summary>
        /// Supported attribute values description string, to be displayed in errors and warnings
        /// </summary>
        public abstract string SupportedValues { get; }
    }

    /// <summary>
    /// Base template class for Igor attribute descriptors
    /// </summary>
    /// <typeparam name="T">Attribute type</typeparam>
    public abstract class AttributeDescriptor<T> : AttributeDescriptor
    {
        public override Type Type => typeof(T);

        protected AttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public abstract bool Convert(AttributeValue source, out T result);

        public T GetValue(AttributeValue source, T defaultValue)
        {
            if (Convert(source, out T result))
                return result;
            else
                return defaultValue;
        }

        public override bool ValidateValue(AttributeValue value)
        {
            return Convert(value, out var result);
        }
    }

    public abstract class StructAttributeDescriptor<T> : AttributeDescriptor<T> where T : struct
    {
        protected StructAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public T? GetValue(AttributeValue source)
        {
            if (Convert(source, out T result))
                return result;
            else
                return null;
        }
    }

    public abstract class ClassAttributeDescriptor<T> : AttributeDescriptor<T> where T : class
    {
        protected ClassAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public T GetValue(AttributeValue source)
        {
            if (Convert(source, out T result))
                return result;
            else
                return null;
        }
    }

    public class BoolAttributeDescriptor : StructAttributeDescriptor<bool>
    {
        public BoolAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public override bool Convert(AttributeValue source, out bool result)
        {
            result = default(bool);
            if (source is AttributeValue.Bool b)
            {
                result = b.Value;
                return true;
            }
            else
                return false;
        }

        public override string SupportedValues => "true | false";
    }

    public class IntAttributeDescriptor : StructAttributeDescriptor<int>
    {
        public IntAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public override bool Convert(AttributeValue source, out int result)
        {
            result = default(int);
            if (source is AttributeValue.Integer b)
            {
                result = (int)b.Value;
                return true;
            }
            else
                return false;
        }

        public override string SupportedValues => "<integer>";
    }

    public class FloatAttributeDescriptor : StructAttributeDescriptor<double>
    {
        public FloatAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public override bool Convert(AttributeValue source, out double result)
        {
            result = default(double);
            if (source is AttributeValue.Integer b)
            {
                result = (double)b.Value;
                return true;
            }
            else if (source is AttributeValue.Float f)
            {
                result = f.Value;
                return true;
            }
            else
                return false;
        }

        public override string SupportedValues => "<float>";
    }

    public class StringAttributeDescriptor : ClassAttributeDescriptor<string>
    {
        public StringAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public override bool Convert(AttributeValue source, out string result)
        {
            result = default(string);
            if (source is AttributeValue.String s)
            {
                result = s.Value;
                return true;
            }
            else
                return false;
        }

        public override string SupportedValues => "<string>";
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class IgorEnumValueAttribute : Attribute
    {
        public string Name { get; }

        public IgorEnumValueAttribute(string name)
        {
            Name = name;
        }
    }

    public class EnumAttributeDescriptor<T> : StructAttributeDescriptor<T> where T : struct
    {
        public EnumAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        public override bool Convert(AttributeValue source, out T result)
        {
            result = default(T);
            if (source is AttributeValue.Enum s && ReflectionHelper.ParseIgorEnum(s.Value, out result))
                return true;
            else
                return false;
        }

        public override string SupportedValues => ReflectionHelper.GetIgorEnumValues(typeof(T)).JoinStrings(" | ");
    }

    public class ObjectAttributeDescriptor<T> : ClassAttributeDescriptor<T> where T : class, new()
    {
        public ObjectAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        private static bool Convert(AttributeValue val, Type t, out object value)
        {
            value = null;
            switch (val)
            {
                case AttributeValue.Bool v when t == typeof(bool):
                    {
                        value = v.Value;
                        return true;
                    }
                case AttributeValue.Bool v when t == typeof(bool?):
                    {
                        value = (bool?)v.Value;
                        return true;
                    }
                case AttributeValue.String v when t == typeof(string):
                    {
                        value = v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(int):
                    {
                        value = (int)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(int):
                    {
                        value = (int)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(float):
                    {
                        value = (float)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(double):
                    {
                        value = (double)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(int?):
                    {
                        value = (int?)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(float?):
                    {
                        value = (float?)v.Value;
                        return true;
                    }
                case AttributeValue.Integer v when t == typeof(double?):
                    {
                        value = (double?)v.Value;
                        return true;
                    }
                case AttributeValue.Float v when t == typeof(float):
                    {
                        value = (float)v.Value;
                        return true;
                    }
                case AttributeValue.Float v when t == typeof(double):
                    {
                        value = (double)v.Value;
                        return true;
                    }
                case AttributeValue.Float v when t == typeof(float?):
                    {
                        value = (float?)v.Value;
                        return true;
                    }
                case AttributeValue.Float v when t == typeof(double?):
                    {
                        value = (double?)v.Value;
                        return true;
                    }
                case AttributeValue.Enum v when t.IsEnum:
                    {
                        if (ReflectionHelper.ParseIgorEnum(t, v.Value, out value))
                            return true;
                        else
                            return false;
                    }
                case AttributeValue.Enum v when ReflectionHelper.IsNullable(t) && t.GenericTypeArguments[0].IsEnum:
                    try
                    {
                        value = Enum.Parse(t, v.Value, true);
                        return true;
                    }
                    catch
                    {
                        return true;
                    }
                case AttributeValue.Object v:
                    {
                        value = Activator.CreateInstance(t);
                        foreach (var definition in v.Definitions)
                        {
                            var prop = t.GetProperty(definition.Name);
                            if (prop != null)
                            {
                                if (Convert(definition.Value, prop.PropertyType, out var propertyValue))
                                    prop.SetValue(value, propertyValue);
                            }
                        }
                        return true;
                    }
                case var _ when ReflectionHelper.IsNullable(t):
                    return true;
                default:
                    return false;
            }
        }

        public override bool Convert(AttributeValue source, out T result)
        {
            result = default(T);
            if (source is AttributeValue.Object)
            {
                var b = Convert(source, typeof(T), out object val);
                result = (T)val;
                return b;
            }
            else
                return false;
        }

        public override string SupportedValues => "<object>";
    }

    public class JsonAttributeDescriptor : ClassAttributeDescriptor<Json.ImmutableJsonObject>
    {
        public JsonAttributeDescriptor(string name, IgorAttributeTargets targets = IgorAttributeTargets.Any, AttributeInheritance inheritance = AttributeInheritance.None)
            : base(name, targets, inheritance)
        {
        }

        private static bool ConvertJson(AttributeValue val, out Json.ImmutableJson value)
        {
            value = null;
            switch (val)
            {
                case AttributeValue.Bool v:
                    {
                        value = v.Value;
                        return true;
                    }
                case AttributeValue.String v:
                    {
                        value = v.Value;
                        return true;
                    }
                case AttributeValue.Integer v:
                    {
                        value = (int)v.Value;
                        return true;
                    }
                case AttributeValue.Float v:
                    {
                        value = (float)v.Value;
                        return true;
                    }
                case AttributeValue.Object v:
                    {
                        var json = new Json.JsonObject();
                        foreach (var definition in v.Definitions)
                        {
                            if (ConvertJson(definition.Value, out var jsonValue))
                                json[definition.Name] = jsonValue;
                        }
                        value = json;
                        return true;
                    }
                default:
                    return false;
            }
        }

        public override bool Convert(AttributeValue source, out Json.ImmutableJsonObject result)
        {
            if (ConvertJson(source, out var value) && value.IsObject)
            {
                result = value.AsObject;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override string SupportedValues => "<object>";
    }
}
