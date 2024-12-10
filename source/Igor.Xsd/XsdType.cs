using Igor.Xsd.AST;
using System;
using System.Xml;
using System.Xml.Schema;

namespace Igor.Xsd
{
    public abstract class XsdType
    {
        public virtual XmlSchemaObject Schema => null;

        public abstract XmlSchemaParticle GetParticle(string name, bool wrapElement);

        public virtual XmlSchemaAnnotated GetContent() => throw new NotSupportedException(GetType().ToString());

        public virtual XmlSchemaAttribute GetAttribute(string name) => throw new NotSupportedException(GetType().ToString());

        public virtual XmlSchemaSimpleContentExtension GetSimpleContentExtension() => throw new NotSupportedException(GetType().ToString());

        public abstract XsdType AsOptional();

        public virtual bool IsMixed => false;
    }

    public abstract class XsdSimpleType : XsdType
    {
        public virtual XmlQualifiedName GetQualifiedName() => null;

        public abstract XmlSchemaElement GetElement(string name);

        public override XmlSchemaParticle GetParticle(string name, bool wrapElement) => GetElement(name);

        public override XmlSchemaAttribute GetAttribute(string name)
        {
            return new XmlSchemaAttribute { SchemaTypeName = GetQualifiedName(), Name = name, Use = XmlSchemaUse.Required };
        }

        public override XmlSchemaSimpleContentExtension GetSimpleContentExtension()
        {
            return new XmlSchemaSimpleContentExtension { BaseTypeName = GetQualifiedName() };
        }

        public override XsdType AsOptional() => new XsdSimpleTypeOptional(this);
    }

    public class XsdSimpleTypeOptional : XsdSimpleType
    {
        public XsdSimpleType BaseType { get; }

        public XsdSimpleTypeOptional(XsdSimpleType baseType)
        {
            BaseType = baseType;
        }

        public override XmlSchemaElement GetElement(string name) => BaseType.GetElement(name).Optional();

        public override XmlSchemaAttribute GetAttribute(string name)
        {
            var attr = base.GetAttribute(name);
            attr.Use = XmlSchemaUse.Optional;
            return attr;
        }

        public override XsdType AsOptional() => this;
    }

    public abstract class XsdComplexType : XsdType
    {
        public abstract XmlSchemaElement GetElement(string name);

        public override XmlSchemaParticle GetParticle(string name, bool wrapElement) => GetElement(name);

        public override XsdType AsOptional() => new XsdComplexTypeOptional(this);
    }

    public class XsdComplexTypeOptional : XsdComplexType
    {
        public XsdComplexType BaseType { get; }

        public XsdComplexTypeOptional(XsdComplexType baseType)
        {
            BaseType = baseType;
        }

        public override XsdType AsOptional() => this;

        public override XmlSchemaElement GetElement(string name) => BaseType.GetElement(name).Optional();

        public override bool IsMixed => BaseType.IsMixed;
    }

    public abstract class XsdElement : XsdType
    {
        public abstract XmlSchemaElement GetElement();

        public override XmlSchemaParticle GetParticle(string name, bool wrapElement)
        {
            var element = GetElement();
            if (wrapElement)
                element = element.WrapElement(name, isMixed: IsMixed);
            return element;
        }

        public override XmlSchemaAnnotated GetContent() => GetElement();

        public override XsdType AsOptional() => new XsdElementOptional(this);
    }

    public class XsdElementOptional : XsdElement
    {
        public XsdElement BaseType { get; }

        public XsdElementOptional(XsdElement baseType)
        {
            BaseType = baseType;
        }

        public override XmlSchemaElement GetElement() => BaseType.GetElement().Optional();

        public override XsdType AsOptional() => this;

        public override bool IsMixed => BaseType.IsMixed;
    }

    public abstract class XsdGroup : XsdType
    {
        public abstract XmlSchemaParticle GetGroup();

        public override XmlSchemaAnnotated GetContent() => GetGroup();

        public override XmlSchemaParticle GetParticle(string name, bool wrapElement)
        {
            if (wrapElement)
                return GetGroup().WrapElement(name, isMixed: IsMixed);
            else
                return GetGroup();
        }

        public override XsdType AsOptional() => new XsdGroupOptional(this);
    }

    public class XsdGroupOptional : XsdGroup
    {
        public XsdGroup BaseType { get; }

        public XsdGroupOptional(XsdGroup baseType)
        {
            BaseType = baseType;
        }

        public override XmlSchemaParticle GetGroup() => BaseType.GetGroup().Optional();

        public override XsdType AsOptional() => this;

        public override bool IsMixed => BaseType.IsMixed;
    }

    public class XsdBuiltInType : XsdSimpleType
    {
        public string Name { get; }

        public XsdBuiltInType(string name)
        {
            this.Name = name;
        }

        public override XmlSchemaElement GetElement(string name) => new XmlSchemaElement { Name = name, SchemaTypeName = GetQualifiedName() };

        public override XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(Name, XsdHelper.XsdNs);
    }

    public class XsdEnum : XsdSimpleType
    {
        public EnumForm EnumForm { get; }

        public XsdEnum(EnumForm enumForm)
        {
            this.EnumForm = enumForm;
        }

        public override XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(EnumForm.xsdName);

        public override XmlSchemaObject Schema
        {
            get
            {
                var restriction = new XmlSchemaSimpleTypeRestriction();
                restriction.BaseTypeName = new XmlQualifiedName("string", XsdHelper.XsdNs);
                foreach (var f in EnumForm.Fields)
                    restriction.Facets.Add(new XmlSchemaEnumerationFacet { Value = f.xmlName }.Annotated(f.Annotation));
                var simpleType = new XmlSchemaSimpleType
                {
                    Name = EnumForm.xsdName,
                    Content = restriction
                };
                return simpleType.Annotated(EnumForm.Annotation);
            }
        }

        public override XmlSchemaElement GetElement(string name) => new XmlSchemaElement { Name = name, SchemaTypeName = GetQualifiedName() };
    }

    public class XsdRecordComplexType : XsdComplexType
    {
        public RecordForm RecordForm { get; }

        public XsdRecordComplexType(RecordForm recordForm)
        {
            this.RecordForm = recordForm;
        }

        public override XmlSchemaElement GetElement(string name) => new XmlSchemaElement { Name = name, SchemaTypeName = GetQualifiedName() };

        public XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(RecordForm.xsdName);

        public override XmlSchemaObject Schema => RecordForm.xsdGetComplexType(true).Annotated(RecordForm.Annotation);

        public override bool IsMixed => RecordForm.xsdMixed;
    }

    public class XsdRecordElement : XsdElement
    {
        public RecordForm RecordForm { get; }

        public XsdRecordElement(RecordForm recordForm)
        {
            this.RecordForm = recordForm;
        }

        public XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(RecordForm.xmlElementName);

        public override XmlSchemaElement GetElement() => new XmlSchemaElement { RefName = GetQualifiedName() };

        public override XmlSchemaObject Schema
        {
            get
            {
                var complexType = RecordForm.xsdGetComplexType(false);
                var element = new XmlSchemaElement
                {
                    Name = RecordForm.xmlElementName,
                    SchemaType = complexType,
                };
                if (RecordForm.Ancestor != null)
                {
                    element.SubstitutionGroup = new XmlQualifiedName(RecordForm.Ancestor.xmlElementName);
                }
                return element.Annotated(RecordForm.Annotation);
            }
        }

        public override bool IsMixed => RecordForm.xsdMixed;
    }

    public class XsdVariant : XsdElement
    {
        public VariantForm VariantForm { get; }

        public XsdVariant(VariantForm variantForm)
        {
            this.VariantForm = variantForm;
        }

        public XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(VariantForm.xmlElementName);

        public override XmlSchemaElement GetElement() => new XmlSchemaElement { RefName = GetQualifiedName() };

        public override XmlSchemaObject Schema
        {
            get
            {
                var element = new XmlSchemaElement
                {
                    Name = VariantForm.xmlElementName,
                    IsAbstract = true,
                };
                if (VariantForm.Ancestor != null)
                {
                    element.SubstitutionGroup = new XmlQualifiedName(VariantForm.Ancestor.xmlElementName);
                }
                return element.Annotated(VariantForm.Annotation);
            }
        }
    }

    public class XsdUnionGroup : XsdGroup
    {
        public UnionForm UnionForm { get; }

        public XsdUnionGroup(UnionForm unionForm)
        {
            UnionForm = unionForm;
        }

        public XmlQualifiedName GetQualifiedName() => new XmlQualifiedName(UnionForm.xsdName);

        public override XmlSchemaParticle GetGroup() => new XmlSchemaGroupRef { RefName = GetQualifiedName() };

        public override XmlSchemaObject Schema => UnionForm.xsdGetGroup(true).Annotated(UnionForm.Annotation);

        public override bool IsMixed => UnionForm.xsdMixed;
    }

    public class XsdList : XsdGroup
    {
        public XsdType ItemType { get; }
        public string ItemName { get; }

        public XsdList(XsdType itemType, string itemName)
        {
            this.ItemType = itemType;
            this.ItemName = itemName;
        }

        public override XmlSchemaParticle GetGroup() => ItemType.GetParticle(ItemName, false).Repeated();

        public override bool IsMixed => ItemType.IsMixed;
    }

    public class XsdRepeated : XsdComplexType
    {
        public XsdType ItemType { get; }

        public XsdRepeated(XsdType itemType)
        {
            this.ItemType = itemType;
        }

        public override XmlSchemaElement GetElement(string name)
        {
            switch (ItemType)
            {
                case XsdElement childElement:
                    return childElement.GetElement().Repeated().WrapElement(isMixed: IsMixed);
                case XsdComplexType childComplexType:
                    return childComplexType.GetElement(name).Repeated();
                case XsdSimpleType childSimpleType:
                    return childSimpleType.GetElement(name).Repeated();
                default:
                    return new XmlSchemaElement();
            }
        }

        public override bool IsMixed => ItemType.IsMixed;
    }

    public class XsdDict : XsdComplexType
    {
        public XsdType KeyType { get; private set; }
        public XsdType ValueType { get; private set; }

        public XsdDict(XsdType keyType, XsdType valueType)
        {
            this.KeyType = keyType;
            this.ValueType = valueType;
        }

        public override XmlSchemaElement GetElement(string name)
        {
            throw new NotImplementedException();
        }
    }

    public class XsdKvList : XsdComplexType
    {
        public XsdType KeyType { get; }
        public XsdType ValueType { get; }
        public string KeyName { get; }
        public string ValueName { get; }

        public XsdKvList(XsdType keyType, XsdType valueType, string keyName, string valueName)
        {
            this.KeyType = keyType;
            this.ValueType = valueType;
            this.KeyName = keyName;
            this.ValueName = valueName;
        }

        public override XmlSchemaElement GetElement(string name)
        {
            var sequence = new XmlSchemaSequence().Repeated();
            sequence.Items.Add(KeyType.GetParticle(KeyName, false));
            sequence.Items.Add(ValueType.GetParticle(ValueName, false));
            return new XmlSchemaElement { SchemaType = new XmlSchemaComplexType { Particle = sequence }, Name = name };
        }

        public override XmlSchemaAnnotated GetContent()
        {
            var sequence = new XmlSchemaSequence().Repeated();
            sequence.Items.Add(KeyType.GetParticle(KeyName, false));
            sequence.Items.Add(ValueType.GetParticle(ValueName, false));
            return new XmlSchemaElement { SchemaType = new XmlSchemaComplexType { Particle = sequence } };
        }
    }
}
