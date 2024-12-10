using Igor.Lua.AST;
using Igor.Lua.Model;
using Igor.Lua.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Lua
{
    /// <summary>
    /// ITarget implementation for Lua language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            var model = new LuaModel();

            var generators = new List<ILuaGenerator>
            {
                new LuaTypeGenerator(),
                new LuaJsonGenerator(),
                new LuaServiceGenerator(),
            };
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<ILuaGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                    gen.Generate(model, mod);

            var files = new List<TargetFile>();
            foreach (var file in model.Files.Values)
            {
                files.Add(new TargetFile(file.FileName, LuaFileRenderer.Render(file), file.IsEmpty));
            }
            return files;
        }

        public string Name => "lua";

        public System.Version DefaultVersion => null;

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => LuaAttributes.AllAttributes;
    }
}
