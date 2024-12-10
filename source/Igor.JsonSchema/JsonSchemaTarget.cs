using Igor.Compiler;
using Igor.JsonSchema.AST;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.JsonSchema
{
    /// <summary>
    /// ITarget implementation for JavaScript language
    /// </summary>
    public class JsonSchemaTarget : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var types = astModules.SelectMany(mod => mod.Types).ToList();
            var rootTypeName = Context.Instance.Attribute(JsonSchemaAttributes.RootType, null);
            if (rootTypeName == null)
            {
                Context.Instance.CompilerOutput.Error(Location.NoLocation, "Root type is not defined. Provide '-set root_type=TypeName' attribute.", ProblemCode.TargetSpecificProblem);
                return new TargetFile[] { };
            }

            TypeForm rootType = rootType = astModules.SelectMany(mod => mod.Types).Where(type => type.Name == rootTypeName).FirstOrDefault();
            if (rootType == null)
            {
                Context.Instance.CompilerOutput.Error(Location.NoLocation, $"Root type '{rootTypeName}' is not defined.", ProblemCode.TargetSpecificProblem);
                return new TargetFile[] { };
            }

            var builder = new JsonSchemaBuilder(rootType);
            var schema = builder.Build();
            var json = SchemaObjectJsonSerializer.Instance.Serialize(schema);
            return new[] { new TargetFile("json_schema.json", Json.JsonFormat.Format(json.ToString()), false) };
        }

        public string Name => "json_schema";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => JsonSchemaAttributes.AllAttributes;
    }
}
