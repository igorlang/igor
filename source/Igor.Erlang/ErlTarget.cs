using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Erlang.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Erlang
{
    /// <summary>
    /// ITarget implementation for Erlang language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new ErlModel();

            var generators = new List<IErlangGenerator>
            {
                new ErlTypeGenerator(),
                new Strings.ErlStringGenerator(),
                new Binary.ErlBinaryGenerator(),
                new Json.ErlJsonGenerator(),
                new Xml.ErlXmlGenerator(),
                new Http.ErlHttpFormGenerator(),
                new ErlServiceGenerator(),
                new Binary.ErlBinaryServiceGenerator(),
                new Json.ErlJsonServiceGenerator(),
                new Http.ErlWebClientGenerator(),
                new Http.ErlCowboyGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<IErlangGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var files = new List<TargetFile>();
            foreach (var erl in model.Modules.Values)
            {
                files.Add(new TargetFile(erl.FileName, ErlModuleRenderer.Render(erl), erl.IsEmpty));
            }
            foreach (var hrl in model.Headers.Values)
            {
                files.Add(new TargetFile(hrl.FileName, HrlRenderer.Render(hrl), hrl.IsEmpty));
            }
            return files;
        }

        public string Name => "erlang";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => ErlAttributes.AllAttributes;
    }
}
