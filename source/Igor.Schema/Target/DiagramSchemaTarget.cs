using Igor.Schema.AST;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Schema
{
    /// <summary>
    /// ITarget implementation for Igor diagram schema
    /// </summary>
    public class DiagramSchemaTarget : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);
            var records = astModules.SelectMany(mod => mod.Definitions.OfType<RecordForm>()).ToList();
            var prototypes = records.Where(rec => rec.diagramArchetype != null).Select(rec => new Prototype(
                name: rec.diagramName,
                archetype: rec.diagramArchetype,
                color: rec.diagramColor,
                icon: rec.diagramIcon,
                caption: rec.diagramCaption,
                showIcon: rec.diagramShowIcon,
                customType: rec.diagramCustomType,
                specialFields: rec.diagramSpecialFields,
                connectors: rec.diagramConnectors)).ToList();
            var diagramTags = records.Where(rec => rec.diagramEnabled).Select(rec => Helper.EnumValue(rec.TagValue)).ToList();
            var schema = new DiagramSchema(prototypes, diagramTags);
            var json = DiagramSchemaJsonSerializer.Instance.Serialize(schema);
            return new[] { new TargetFile("diagram_schema.json", Json.JsonFormat.Format(json.ToString()), false) };
        }

        public string Name => "diagram";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => DiagramSchemaAttributes.AllAttributes;
    }
}
