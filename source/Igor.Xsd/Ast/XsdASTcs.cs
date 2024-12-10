using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Igor.Xsd.AST
{
    public partial class Module
    {
        public IEnumerable<XmlNamespaceAttribute> xmlNamespaces => ListAttribute(CoreAttributes.XmlXmlns);
    }

    public partial class TypeForm
    {
        public string xsdName => Attribute(XsdAttributes.XsdName, Name.Format(xsdTypeNotation, false));
        public Notation xsdTypeNotation => Attribute(XsdAttributes.XsdNotation, Notation.None);

        public abstract XsdType xsdType(Dictionary<string, XsdType> genericArgs);
    }

    public partial class EnumForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs) => new XsdEnum(this);
    }

    public partial class InterfaceForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs) => throw new NotSupportedException();
    }

    public partial class RecordField
    {
        public XsdType xsdType(Dictionary<string, XsdType> genericArgs) => XsdHelper.xsdType(Type, this, genericArgs);

        public string xsdRef => Attribute(XsdAttributes.XsdRef);
    }

    public partial class RecordForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs)
        {
            switch (xmlType)
            {
                case XmlType.Element:
                    return new XsdRecordElement(this);
                case XmlType.ComplexType:
                    return new XsdRecordComplexType(this);
                default:
                    throw new NotImplementedException(xmlType.ToString());
            }
        }

        public bool xsdMixed
        {
            get
            {
                var fields = xmlSerializedFields.Where(f => !f.xmlAttribute);
                if (fields.Count() > 1 && fields.Any(f => f.xmlText))
                    return true;
                var content = fields.FirstOrDefault(f => f.xmlContent);
                if (content != null)
                    return content.xsdType(null).IsMixed;
                return false;
            }
        }

        public XmlSchemaComplexType xsdGetComplexType(bool named)
        {
            var complexType = new XmlSchemaComplexType();
            if (named)
                complexType.Name = xsdName;
            if (xsdMixed)
                complexType.IsMixed = true;

            var fields = xmlSerializedFields.ToList();
            var xmlAttributes = fields.Where(f => f.xmlAttribute);
            var elements = fields.Where(f => !f.xmlAttribute && !f.xmlText);
            var text = fields.FirstOrDefault(f => f.xmlText);
            var attributeCollection = complexType.Attributes;

            if (elements.Any())
            {
                XmlSchemaGroupBase group;
                if (xmlOrdered || elements.Count() <= 1 || fields.Any(f => f.xmlContent))
                    group = new XmlSchemaSequence();
                else
                    group = new XmlSchemaAll();
                complexType.Particle = group;

                foreach (var f in elements)
                {
                    var xType = f.xsdType(null);
                    if (f.xmlContent)
                    {
                        group.Items.Add(xType.GetContent().Annotated(f.Annotation));
                    }
                    else
                    {
                        group.Items.Add(xType.GetParticle(f.xmlElementName, true).Annotated(f.Annotation));
                    }
                }
            }
            else if (text != null)
            {
                var simpleContentExtension = text.xsdType(null).GetSimpleContentExtension().Annotated(text.Annotation);
                var simpleContent = new XmlSchemaSimpleContent { Content = simpleContentExtension };
                attributeCollection = simpleContentExtension.Attributes;
                complexType.ContentModel = simpleContent;
            }

            foreach (var f in xmlAttributes)
            {
                var xsdRef = f.xsdRef;
                XmlSchemaAttribute attr;
                if (xsdRef == null)
                    attr = f.xsdType(null).GetAttribute(f.xmlAttributeName);
                else
                    attr = new XmlSchemaAttribute { RefName = new XmlQualifiedName(xsdRef), Use = f.IsOptional ? XmlSchemaUse.Optional : XmlSchemaUse.Required };
                attributeCollection.Add(attr.Annotated(f.Annotation));
            }

            return complexType;
        }
    }

    public partial class VariantForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs)
        {
            return new XsdVariant(this);
        }
    }

    public partial class DefineForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs)
        {
            return XsdHelper.xsdType(Type, null, genericArgs);
        }
    }

    public partial class UnionClause
    {
        public XsdType xsdType(Dictionary<string, XsdType> genericArgs)
        {
            return XsdHelper.xsdType(Type, this, genericArgs);
        }
    }

    public partial class UnionForm
    {
        public override XsdType xsdType(Dictionary<string, XsdType> genericArgs)
        {
            return new XsdUnionGroup(this);
        }

        public bool xsdMixed => Clauses.Any(c => c.xmlText);

        public XmlSchemaGroup xsdGetGroup(bool named)
        {
            var choice = new XmlSchemaChoice();
            foreach (var clause in Clauses)
            {
                if (!clause.xmlText)
                {
                    XmlSchemaAnnotated element;
                    if (clause.Type != null)
                    {
                        if (clause.xmlContent)
                            element = clause.xsdType(null).GetContent();
                        else
                            element = clause.xsdType(null).GetParticle(clause.xmlElementName, true);
                    }
                    else
                    {
                        element = new XmlSchemaElement { Name = clause.xmlElementName, SchemaType = new XmlSchemaComplexType() };
                    }
                    choice.Items.Add(element.Annotated(clause.Annotation));
                }
            }

            var group = new XmlSchemaGroup { Particle = choice };
            if (named)
                group.Name = xsdName;
            return group;
        }
    }
}
