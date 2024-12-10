using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Collections.Generic;

namespace Igor.CSharp
{
    internal class CsXmlAttributeGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var type in mod.Types)
            {
                if (type.xmlEnabled && type.csEnabled && type.Attribute(CsAttributes.XmlAttributes, false))
                {
                    model.FileOf(type).Use("System.Xml.Serialization");
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenerateEnum(model, enumForm);
                            break;
                        case RecordForm recordForm:
                            GenerateStruct(model, recordForm);
                            break;
                        case VariantForm variantForm:
                            GenerateStruct(model, variantForm);
                            break;
                    }
                }
            }
        }

        private void GenerateEnum(CsModel model, EnumForm enumForm)
        {
            var e = model.TypeOf(enumForm);
            foreach (var field in enumForm.Fields)
            {
                if (field.csName != field.xmlName)
                {
                    e.Field(field.csName).AddAttribute($@"XmlEnum(Name = ""{field.xmlName}"")");
                }
            }
        }

        private void GenerateStruct(CsModel model, StructForm structForm)
        {
            var c = model.TypeOf(structForm);
            if (structForm.xmlElementName != structForm.csName)
                c.AddAttribute($@"XmlType(TypeName = ""{structForm.xmlElementName}"")");
            foreach (var field in structForm.Fields)
            {
                if (!field.IsLocal || field.IsTag)
                    continue;
                var f = c.Property(field.csName);
                var dataType = field.xsdXsType;
                if (field.xmlIgnore)
                {
                    f.AddAttribute("XmlIgnore");
                }
                else if (field.xmlAttribute)
                {
                    var args = new List<(string, string)>();
                    if (field.csName != field.xmlAttributeName)
                        args.Add(("AttributeName", field.xmlAttributeName.Quoted()));
                    if (dataType != null)
                        args.Add(("DataType", dataType.Quoted()));
                    f.AddAttribute("XmlAttribute", args);
                }
                else if (field.xmlText)
                {
                    f.AddAttribute("XmlText");
                }
                else if (field.xmlContent)
                {
                    if (field.Type is BuiltInType.List l && l.ItemType is StructForm t)
                    {
                        f.AddAttribute($@"XmlElement(""{t.xmlElementName}"")");
                    }
                }
                else
                {
                    var args = new List<(string, string)>();
                    if (field.csName != field.xmlElementName)
                        args.Add(("ElementName", field.xmlElementName.Quoted()));
                    if (dataType != null)
                        args.Add(("DataType", dataType.Quoted()));
                    if (args.Count > 0)
                        f.AddAttribute("XmlElement", args);
                }
            }
        }
    }
}
