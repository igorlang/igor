using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor
{
    internal static class EnumParser<T> where T : struct
    {
        private static readonly Dictionary<string, T> values = Enum.GetValues(typeof(T)).Cast<object>().ToDictionary(e => ((Enum)e).GetIgorEnumName(), e => (T)e);

        public static bool TryParse(string name, out T value)
        {
            return values.TryGetValue(name, out value);
        }
    }

    /// <summary>
    /// Reflection routines and extension methods
    /// </summary>
    public static class ReflectionHelper
    {
        public static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Name == "Nullable`1";
        }

        public static bool IsEnumerable(Type type)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static Type EnumerableItemType(Type type)
        {
            return type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Select(t => t.GetGenericArguments()[0]).FirstOrDefault();
        }

        public static bool IsCollection(Type type)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public static bool IsReadOnlyCollection(Type type)
        {
            return type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>));
        }

        public static IReadOnlyList<T> CollectTypes<T>(Assembly assembly)
        {
            var results = new List<T>();
            foreach (var type in assembly.GetTypes())
            {
                foreach (var intf in type.GetInterfaces())
                {
                    if (intf.Equals(typeof(T)))
                        results.Add((T)Activator.CreateInstance(type));
                }
            }
            return results;
        }

        public static IEnumerable<Type> CollectTypesWithAttribute(Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes().Where(type => type.IsDefined(attributeType));
        }

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static string GetIgorEnumName(this Enum enumVal)
        {
            var type = enumVal.GetType();
            var result = enumVal.ToString().ToLowerInvariant();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(IgorEnumValueAttribute), false);
            if (attributes.Length > 0)
                result = ((IgorEnumValueAttribute)attributes[0]).Name;
            return result;
        }

        public static bool ParseIgorEnum<T>(string name, out T value) where T : struct
        {
            return EnumParser<T>.TryParse(name, out value);
        }

        public static bool ParseIgorEnum(Type enumType, string name, out object value)
        {
            foreach (Enum enumValue in enumType.GetEnumValues())
            {
                if (enumValue.GetIgorEnumName() == name)
                {
                    value = enumValue;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public static IEnumerable<string> GetIgorEnumValues(Type enumType)
        {
            foreach (Enum enumValue in enumType.GetEnumValues())
            {
                yield return enumValue.GetIgorEnumName();
            }
        }
    }
}
