using Igor.JavaScript.AST;
using Igor.JavaScript.Model;
using Igor.JavaScript.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.JavaScript
{
    /// <summary>
    /// ITarget implementation for JavaScript language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new JsModel();

            var generators = new List<IJsGenerator>
            {
                new JsHttpClientGenerator()
                /*
                new TsTypeGenerator(),
                new Json.TsJsonGenerator(),
                new TsServiceGenerator(),
                new Json.TsJsonServiceGenerator(),
                new TsWebServiceClientGenerator(),*/
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IJsGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                {
                    gen.Generate(model, mod);
                }

            return model.Files.Values.Select(file => new TargetFile(file.FileName, JsRenderer.Render(file), file.IsEmpty)).ToList();
        }

        public string Name => "js";

        public System.Version DefaultVersion => new System.Version(6, 0);

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => JsAttributes.AllAttributes;
    }
}
