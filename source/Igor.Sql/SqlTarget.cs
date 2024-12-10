using Igor.Sql.AST;
using Igor.Sql.Generators;
using Igor.Sql.Model;
using Igor.Sql.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Sql
{
    public class SqlTarget : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new SqlModel();

            var generators = new List<ISqlGenerator>
            {
                new SqlTableGenerator()
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<ISqlGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var sqlContent = SqlRenderer.Render<SqlPostgresRenderer>(model);

            return new[] { new TargetFile("schema.sql", sqlContent, false) };
        }

        public string Name => "sql";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => SqlAttributes.AllAttributes;
    }
}
