using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor
{
    public static class AttributeHelper
    {
        public static T Attribute<T>(IAttributeHost host, AttributeDescriptor<T> attribute, T defaultValue)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            var attributeValue = GetAttributeValue(host, attribute);
            if (attributeValue == null)
                return defaultValue;
            else
                return attribute.GetValue(attributeValue, defaultValue);
        }

        public static T? Attribute<T>(IAttributeHost host, StructAttributeDescriptor<T> attribute) where T : struct
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            var attributeValue = GetAttributeValue(host, attribute);
            if (attributeValue == null)
                return null;
            else
                return attribute.GetValue(attributeValue);
        }

        public static T Attribute<T>(IAttributeHost host, ClassAttributeDescriptor<T> attribute) where T : class
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            var attributeValue = GetAttributeValue(host, attribute);
            if (attributeValue == null)
                return null;
            else
                return attribute.GetValue(attributeValue);
        }

        public static List<T> ListAttribute<T>(this IAttributeHost host, AttributeDescriptor<T> attribute)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            var result = new List<T>();
            foreach (var attributeValue in GetAttributeValues(host, attribute))
            {
                if (attribute.Convert(attributeValue, out T val))
                    result.Add(val);
            }
            return result;
        }

        private static bool Check(AttributeDefinition attr, string language, string name)
        {
            var attrLanguage = attr.Target;
            return ((attrLanguage == null || attrLanguage == "*" || attrLanguage == language)) && (attr.Name == name);
        }

        private static AttributeValue GetOwnAttributeValue(IAttributeHost host, AttributeDescriptor attribute)
        {
            return host.Attributes.FirstOrDefault(attr => Check(attr, Context.Instance.Target, attribute.Name))?.Value;
        }

        private static AttributeValue GetEnvironmentAttributeValue(AttributeDescriptor attribute)
        {
            if (Context.Instance.Attributes.TryGetValue(attribute.Name, out var value))
                return value;
            else
                return null;
        }

        private static IEnumerable<AttributeValue> GetOwnAttributeValues(IAttributeHost host, AttributeDescriptor attribute)
        {
            return host.Attributes.Where(attr => Check(attr, Context.Instance.Target, attribute.Name)).Select(attr => attr.Value);
        }

        private static AttributeValue GetAttributeValue(IAttributeHost host, AttributeDescriptor attribute)
        {
            var own = GetOwnAttributeValue(host, attribute);
            switch (attribute.Inheritance)
            {
                default:
                    return own;

                case AttributeInheritance.Scope:
                    if (own != null)
                        return own;
                    if (host.ScopeHost != null)
                        return GetAttributeValue(host.ScopeHost, attribute);
                    return GetEnvironmentAttributeValue(attribute);

                case AttributeInheritance.Type:
                    if (own != null)
                        return own;
                    if (host.ParentTypeHost != null)
                        return GetAttributeValue(host.ParentTypeHost, attribute);
                    return null;

                case AttributeInheritance.Inherited:
                    if (own != null)
                        return own;
                    if (host.InheritedHost != null)
                        return GetAttributeValue(host.InheritedHost, attribute);
                    return null;
            }
        }

        private static IEnumerable<AttributeValue> GetAttributeValues(IAttributeHost host, AttributeDescriptor attribute)
        {
            var owns = GetOwnAttributeValues(host, attribute);
            foreach (var own in owns)
                yield return own;
            switch (attribute.Inheritance)
            {
                default:
                case AttributeInheritance.None:
                    break;

                case AttributeInheritance.Scope:
                    if (host.ScopeHost != null)
                        foreach (var parent in GetAttributeValues(host.ScopeHost, attribute))
                            yield return parent;
                    var environmentValue = GetEnvironmentAttributeValue(attribute);
                    if (environmentValue != null)
                        yield return environmentValue;
                    break;

                case AttributeInheritance.Type:
                    if (host.ParentTypeHost != null)
                        foreach (var type in GetAttributeValues(host.ParentTypeHost, attribute))
                            yield return type;
                    break;

                case AttributeInheritance.Inherited:
                    if (host.InheritedHost != null)
                        foreach (var anc in GetAttributeValues(host.InheritedHost, attribute))
                            yield return anc;
                    break;
            }
        }
    }
}
