using Igor.Xsd.AST;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Igor.Xsd
{
    /// <summary>
    /// ITarget implementation for XSD schema generation
    /// </summary>
    public class XsdTarget : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var schema = new XmlSchema();
            var namespaces = astModules.SelectMany(mod => mod.xmlNamespaces).GroupBy(ns => ns.prefix).Select(group => group.First());
            foreach (var ns in namespaces)
            {
                schema.Namespaces.Add(ns.prefix, ns.@namespace);
                schema.Includes.Add(new XmlSchemaImport { Namespace = ns.@namespace, SchemaLocation = ns.location });
            }
            var types = astModules.SelectMany(mod => mod.Definitions.OfType<TypeForm>()).Where(s => s.xmlEnabled).ToList();
            foreach (var type in types)
            {
                if (type is InterfaceForm)
                    continue;

                var xsdType = XsdHelper.xsdType(type, null, null);
                var schemaObject = xsdType.Schema;
                if (schemaObject != null)
                    schema.Items.Add(schemaObject);
            }
            return new[] { new TargetFile("schema.xsd", SchemaToString(schema), false) };
        }

        public string Name => "xsd";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => XsdAttributes.AllAttributes;

        private string SchemaToString(XmlSchema schema)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.Encoding = Encoding.UTF8;
            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
            {
                schema.Write(writer);
                return stringWriter.ToString();
            }
        }
    }
}
