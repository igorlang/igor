using Igor.UE4.AST;
using Igor.UE4.Model;
using Igor.UE4.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.UE4
{
    /// <summary>
    /// ITarget implementation for Unreal Engine 4 (C++ language)
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new UeModel();

            var generators = new List<IUeGenerator>
            {
                new UeTypeGenerator(),
                new UeServiceGenerator(),
                new UeJsonGenerator(),
                new UeJsonServiceGenerator(),
                new UeBinaryGenerator(),
                new UeBinaryServiceGenerator(),
                new UeHttpClientGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IUeGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);
            var files = new List<TargetFile>();

            foreach (var file in model.HFiles.Values)
            {
                files.Add(new TargetFile(file.FileName, UeHRenderer.Render(file), file.IsEmpty));
            }
            foreach (var file in model.CppFiles.Values)
            {
                files.Add(new TargetFile(file.FileName, UeCppRenderer.Render(file), file.IsEmpty));
            }
            return files;
        }

        public string Name => "ue4";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => UeAttributes.AllAttributes;
    }
}
