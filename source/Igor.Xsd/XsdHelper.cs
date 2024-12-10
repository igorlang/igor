using Igor.Xsd.AST;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace Igor.Xsd
{
    public static class XsdHelper
    {
        public static readonly string XsdNs = "http://www.w3.org/2001/XMLSchema";

        private static T GetAttr<T>(Statement field, AttributeDescriptor<T> attribute, T @default)
        {
            if (field == null)
                return @default;
            else
                return field.Attribute(attribute, @default);
        }

        public static XsdType xsdType(IType type, Statement field, Dictionary<string, XsdType> genericArgs)
        {
            var xsType = GetAttr(field, CoreAttributes.XsdXsType, null);
            if (xsType != null)
                return new XsdBuiltInType(xsType);
            switch (type)
            {
                case BuiltInType.String _:
                case BuiltInType.Atom _:
                    return new XsdBuiltInType("string");
                case BuiltInType.Bool _: return new XsdBuiltInType("boolean");
                case BuiltInType.Integer _: return new XsdBuiltInType("integer");
                case BuiltInType.Float _: return new XsdBuiltInType("decimal");
                case TypeForm f when f.xsdXsType != null: return new XsdBuiltInType(f.xsdXsType);
                case TypeForm typeForm: return typeForm.xsdType(genericArgs);
                case GenericType generic:
                    {
                        var prototype = generic.Prototype;
                        var protoXsType = prototype.xsdXsType;
                        if (protoXsType != null)
                            return new XsdBuiltInType(protoXsType);
                        else if (prototype is DefineForm alias)
                        {
                            var newArgs = genericArgs == null ? new Dictionary<string, XsdType>() : new Dictionary<string, XsdType>(genericArgs);
                            for (int i = 0; i < alias.Args.Count; i++)
                            {
                                newArgs[alias.Args[i].Name] = xsdType(generic.Args[i], null, genericArgs);
                            }
                            return xsdType(alias.Type, null, newArgs);
                        }
                        else
                            return new XsdBuiltInType("any");
                    }
                case BuiltInType.List l:
                    if (GetAttr(field, CoreAttributes.XmlFlat, false))
                        return new XsdRepeated(xsdType(l.ItemType, null, genericArgs));
                    else
                        return new XsdList(xsdType(l.ItemType, null, genericArgs), GetAttr(field, CoreAttributes.XmlItemName, "item"));
                case BuiltInType.Dict d:
                    {
                        var keyName = GetAttr(field, CoreAttributes.XmlKeyName, "key");
                        var valueName = GetAttr(field, CoreAttributes.XmlValueName, "value");
                        if (GetAttr(field, CoreAttributes.XmlKVList, false))
                            return new XsdKvList(xsdType(d.KeyType, null, genericArgs), xsdType(d.ValueType, null, genericArgs), keyName, valueName);
                        else
                            return new XsdDict(xsdType(d.KeyType, null, genericArgs), xsdType(d.ValueType, null, genericArgs));
                    }
                case BuiltInType.Optional opt:
                    {
                        return xsdType(opt.ItemType, field, genericArgs).AsOptional();
                    }
                case GenericArgument genericArg:
                    {
                        if (genericArgs.ContainsKey(genericArg.Name))
                            return genericArgs[genericArg.Name];
                        else
                            return new XsdBuiltInType("any");
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Do not know xsd type {type}");
            }
        }

        public static XmlSchemaElement WrapElement(this XmlSchemaParticle content, string name = null, bool isMixed = false)
        {
            var childGroup = new XmlSchemaSequence();
            childGroup.Items.Add(content);
            var childType = new XmlSchemaComplexType { Particle = childGroup };
            if (isMixed)
                childType.IsMixed = true;
            return new XmlSchemaElement { Name = name, SchemaType = childType };
        }

        public static T Repeated<T>(this T particle) where T : XmlSchemaParticle
        {
            particle.MinOccurs = 0;
            particle.MaxOccursString = "unbounded";
            return particle;
        }

        public static T Optional<T>(this T particle) where T : XmlSchemaParticle
        {
            particle.MinOccurs = 0;
            return particle;
        }

        public static T Annotated<T>(this T annotated, string comment) where T : XmlSchemaAnnotated
        {
            if (!string.IsNullOrEmpty(comment))
            {
                if (annotated.Annotation == null)
                    annotated.Annotation = new XmlSchemaAnnotation();
                var doc = new XmlSchemaDocumentation();
                doc.Markup = TextToNodeArray(comment);
                annotated.Annotation.Items.Add(doc);
            }
            return annotated;
        }

        public static XmlNode[] TextToNodeArray(string text)
        {
            XmlDocument doc = new XmlDocument();
            return new XmlNode[] { doc.CreateTextNode(text) };
        }
    }
}
