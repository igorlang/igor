using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using Igor.TypeScript.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.TypeScript
{
    /// <summary>
    /// ITarget implementation for TypeScript language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new TsModel();

            var generators = new List<ITsGenerator>
            {
                new TsTypeGenerator(),
                new Json.TsJsonGenerator(),
                new TsServiceGenerator(),
                new Json.TsJsonServiceGenerator(),
                new TsWebServiceClientGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<ITsGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                {
                    gen.Generate(model, mod);
                }

            return model.Files.Values.Select(file => new TargetFile(file.FileName, TsModuleRenderer.Render(file), file.IsEmpty)).ToList();
        }

        public string Name => "ts";

        public System.Version DefaultVersion => new System.Version(4, 3);

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => TsAttributes.AllAttributes;
    }
}
