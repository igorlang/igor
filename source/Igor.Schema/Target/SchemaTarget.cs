using Igor.Compiler;
using Igor.Schema.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Schema
{
    /// <summary>
    /// ITarget implementation for Igor schema
    /// </summary>
    public class SchemaTarget : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var types = astModules.SelectMany(mod => mod.Types).ToList();
            var roots = astModules.SelectMany(mod => mod.Types).Where(type => type.schemaRoot).ToList();
            var rootTypeName = Context.Instance.Attribute(SchemaAttributes.RootType, null);

            TypeForm rootType = null;
            if (rootTypeName != null)
            {
                rootType = astModules.SelectMany(mod => mod.Types).Where(type => type.Name == rootTypeName).FirstOrDefault();
                if (rootType == null)
                {
                    Context.Instance.CompilerOutput.Error(Location.NoLocation, $"Root type '{rootTypeName}' is not defined.", ProblemCode.TargetSpecificProblem);
                    return new TargetFile[] { };
                }
            }

            var count = roots.Count;

            if (rootType == null)
            {
                if (count == 0)
                {
                    Context.Instance.CompilerOutput.Error(Location.NoLocation, "No schema root defined. Add [schema root] attribute over root variant.", ProblemCode.TargetSpecificProblem);
                    return new TargetFile[] { };
                }

                if (count >= 2)
                {
                    foreach (var root in roots)
                    {
                        root.Error("Too many schema roots. Remove all [schema root] attributes but one.");
                    }
                    return new TargetFile[] { };
                }
            }
            
            {
                var root = rootType ?? roots.First();
                var customTypes =
                    from type in types
                    where type.schemaEnabled
                    let schemaType = type.schemaType
                    where schemaType != null
                    select Tuple.Create(type.Name, schemaType);
                var schema = new Schema(customTypes.ToDictionary(type => type.Item1, type => type.Item2), root.Name, version: "1.1");
                var json = SchemaJsonSerializer.Instance.Serialize(schema);
                return new[] { new TargetFile("schema.json", Json.JsonFormat.Format(json.ToString()), false) };
            }
        }

        public string Name => "schema";

        public System.Version DefaultVersion => new System.Version(1, 1);

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => SchemaAttributes.AllAttributes;
    }
}
