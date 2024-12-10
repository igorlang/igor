using Igor.Elixir.AST;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Igor.Elixir.Generators;
using Igor.Elixir.Model;
using Igor.Elixir.Render;

namespace Igor.Elixir
{
    /// <summary>
    /// ITarget implementation for Elixir language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new ExModel();

            var generators = new List<IElixirGenerator>
            {
                new ExTypeGenerator(),
                new ExStringGenerator(),
                new ExJsonGenerator(),
                new ExHttpServerGenerator(),
                new ExHttpClientGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IElixirGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var files = new List<TargetFile>();
            foreach (var exFile in model.Files.Values)
            {
                files.Add(new TargetFile(exFile.FileName, ExFileRenderer.Render(exFile), exFile.IsEmpty));
            }
            return files;
        }

        public string Name => "elixir";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => ExAttributes.AllAttributes;
    }
}
