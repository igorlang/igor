using Igor.Go.AST;
using Igor.Go.Model;
using Igor.Go.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Go
{
    /// <summary>
    /// ITarget implementation for Go language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new GoModel();

            var generators = new List<IGoGenerator>
            {
                new GoTypeGenerator(),
                new GoVariantInterfaceGenerator(),
                new GoJsonGenerator(),
                new GoWebServiceGenerator()
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IGoGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var files = new List<TargetFile>();
            foreach (var package in model.Packages.Values)
            {
                foreach (var file in package.Files.Values)
                {
                    files.Add(new TargetFile(System.IO.Path.Combine(package.Name, file.FileName), GoFileRenderer.Render(file), file.IsEmpty));
                }
            }
            return files;
        }

        public string Name => "go";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => GoAttributes.AllAttributes;
    }
}
