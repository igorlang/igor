using Igor.Python.AST;
using Igor.Python.Model;
using Igor.Python.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Python
{
    /// <summary>
    /// ITarget implementation for Python language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new PythonModel();

            var generators = new List<IPythonGenerator>
            {
                new PythonTypeGenerator(),
                new PythonJsonGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IPythonGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var files = new List<TargetFile>();
            foreach (var erl in model.Files.Values)
            {
                files.Add(new TargetFile(erl.FileName, PythonFileRenderer.Render(erl), erl.IsEmpty));
            }
            return files;
        }

        public string Name => "python";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => PythonAttributes.AllAttributes;
    }
}
