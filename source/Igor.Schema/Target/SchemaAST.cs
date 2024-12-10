using System.Collections.Generic;
using System.Linq;

namespace Igor.Schema.AST
{
    public partial class Statement
    {
        public string schemaCategory => Attribute(SchemaAttributes.Category, null);
        public string schemaGroup => Attribute(SchemaAttributes.Group, null);
        public string schemaInterface => Attribute(SchemaAttributes.Interface, null);
        public string schemaHelp => Attribute(SchemaAttributes.Help, Annotation);
        public bool schemaMultiline => Attribute(SchemaAttributes.Multiline, false);
        public bool schemaCompact => Attribute(SchemaAttributes.Compact, false);
        public bool schemaNotEmpty => Attribute(CoreAttributes.NotEmpty, false);
        public string schemaSource => Attribute(SchemaAttributes.Source, null);
        public string schemaEditorKey => Attribute(SchemaAttributes.EditorKey, null);
        public int? schemaIntMin => Attribute(SchemaAttributes.IntMin);
        public int? schemaIntMax => Attribute(SchemaAttributes.IntMax);
        public double? schemaFloatMin => Attribute(SchemaAttributes.FloatMin);
        public double? schemaFloatMax => Attribute(SchemaAttributes.FloatMax);
        public Dictionary<string, Json.ImmutableJson> schemaMeta => ListAttribute(SchemaAttributes.Meta)
            .SelectMany(meta => meta)
            .GroupBy(pair => pair.Key)
            .ToDictionary(group => group.Key, group => group.Last().Value);

        public PathOptions schemaPathOptions
        {
            get
            {
                var isPath = Attribute(SchemaAttributes.Path, false);
                if (isPath)
                {
                    var root = Attribute(SchemaAttributes.PathRoot);
                    var defaultPath = Attribute(SchemaAttributes.PathDefault);
                    var extension = Attribute(SchemaAttributes.PathExtension);
                    var includeExtension = Attribute(SchemaAttributes.PathIncludeExtension);
                    return new PathOptions(root, defaultPath, extension, includeExtension);
                }
                else
                    return null;
            }
        }

        public string schemaSyntax => Attribute(SchemaAttributes.Syntax);
    }

    public partial class RecordField
    {
        public bool schemaIgnore => Attribute(SchemaAttributes.Ignore, false);
        public DescriptorKind schemaDescriptorKind => Attribute(SchemaAttributes.Editor, Helper.TypeDescriptorKind(Type));
        public Descriptor schemaDescriptor => Helper.TypeDescriptor(schemaDescriptorKind, Type, DefaultValue, this, null, false);
    }

    public partial class TypeForm
    {
        public bool schemaEnabled => Attribute(CoreAttributes.Enabled, false);
        public bool schemaRoot => Attribute(SchemaAttributes.Root, false);
        public string schemaName => Attribute(SchemaAttributes.Name, Name);
        public virtual CustomType schemaType { get; }

        public DescriptorKind schemaDescriptorKind => Attribute(SchemaAttributes.Editor, schemaDefaultDescriptorKind);

        public abstract DescriptorKind schemaDefaultDescriptorKind { get; }
        public virtual IntTypeName? schemaIntTypeName => null;
        public virtual FloatTypeName? schemaFloatTypeName => null;

        public List<string> schemaGenericArgs => Arity == 0 ? null : Args.Select(arg => arg.Name).ToList();
    }

    public partial class EnumForm
    {
        public override DescriptorKind schemaDefaultDescriptorKind => DescriptorKind.Enum;
        public override CustomType schemaType => new EnumCustomType(schemaMeta, Fields.Select(f => f.Name).ToList());
    }

    public partial class VariantForm
    {
        public override CustomType schemaType
        {
            get
            {
                var children = Records.ToDictionary(ff => Helper.EnumValue(ff.TagValue), ff => ff.Name);
                var tagName = TagField.Name;
                return new VariantCustomType(schemaMeta, schemaFields, schemaParent, schemaInterfaces, tagName, children);
            }
        }
    }

    public partial class RecordForm
    {
        public override CustomType schemaType => new RecordCustomType(schemaMeta, schemaFields, schemaParent, schemaInterfaces, schemaGenericArgs, schemaGroup);
    }

    public partial class StructForm
    {
        public override DescriptorKind schemaDefaultDescriptorKind => DescriptorKind.Record;

        public Dictionary<string, Descriptor> schemaFields =>
            Fields.Where(f => !f.IsInherited && !f.schemaIgnore).ToDictionary(field => field.Name, field => field.schemaDescriptor);

        public List<string> schemaInterfaces => Interfaces.Select(Helper.InterfaceName).ToList();

        public string schemaParent => Ancestor == null ? null : Ancestor.schemaName;
    }

    public partial class UnionClause
    {
        public bool schemaIgnore => Attribute(SchemaAttributes.Ignore, false);

        public DescriptorKind schemaDescriptorKind => Attribute(SchemaAttributes.Editor, Helper.TypeDescriptorKind(Type));

        public Descriptor schemaDescriptor
        {
            get
            {
                if (Type == null)
                    return null;
                else
                    return Helper.TypeDescriptor(Helper.TypeDescriptorKind(Type), Type, null, this, null, false);
            }
        }
    }

    public partial class UnionForm
    {
        public override DescriptorKind schemaDefaultDescriptorKind => DescriptorKind.Union;
        public Dictionary<string, Descriptor> schemaClauses => Clauses.Where(f => !f.schemaIgnore).ToDictionary(field => field.Name, field => field.schemaDescriptor);

        public override CustomType schemaType => new UnionCustomType(schemaMeta, schemaClauses, schemaGenericArgs);
    }

    public partial class DefineForm
    {
        public override DescriptorKind schemaDefaultDescriptorKind => Helper.TypeDescriptorKind(Type);
        public override IntTypeName? schemaIntTypeName => Helper.IntTypeName(Type);
        public override FloatTypeName? schemaFloatTypeName => Helper.FloatTypeName(Type);
    }
}
