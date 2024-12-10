using System.Collections.Generic;
using System.Linq;

namespace Igor.Schema.AST
{
    public partial class RecordField
    {
        public string diagramSpecialField => Attribute(DiagramSchemaAttributes.SpecialField, null);

        public Connector diagramConnector
        {
            get
            {
                var connector = Attribute(DiagramSchemaAttributes.Connector, null);
                if (connector != null)
                    return StructForm.ParseConnector(connector, Name, ConnectorType.Property, "OUT");
                else
                    return null;
            }
        }
    }

    public partial class StructForm
    {
        public string diagramName => Attribute(DiagramSchemaAttributes.Name, Name);

        public string diagramCaption => Attribute(DiagramSchemaAttributes.Caption, null);

        public string diagramArchetype => Attribute(DiagramSchemaAttributes.Archetype, null);

        public string diagramIcon => Attribute(DiagramSchemaAttributes.Icon, null);

        public bool? diagramShowIcon => Attribute(DiagramSchemaAttributes.ShowIcon);

        public string diagramColor => Attribute(DiagramSchemaAttributes.Color, null);

        public string diagramCustomType => Name;

        public Dictionary<string, string> diagramSpecialFields =>
            Fields.Where(f => f.diagramSpecialField != null).ToDictionary(field => field.Name, field => field.diagramSpecialField);

        public bool diagramEnabled => Attribute(CoreAttributes.Enabled, false);

        public List<Connector> diagramConnectors
        {
            get
            {
                var linkConnectors = ListAttribute(DiagramSchemaAttributes.Connector).Select(connector => ParseConnector(connector, null, ConnectorType.Asset, "IN"));
                var assetConnectors = Fields.Select(f => f.diagramConnector).Where(c => c != null);
                return linkConnectors.Concat(assetConnectors).ToList();
            }
        }

        public static Connector ParseConnector(ConnectorAttribute c, string field, ConnectorType defaultType, string defaultName)
        {
            return new Connector(
                c.name ?? defaultName,
                c.type == ConnectorType1.None ? defaultType : (ConnectorType)c.type,
                position: c.position,
                caption: c.caption,
                color: c.color,
                category: c.category,
                field: field);
        }
    }
}
