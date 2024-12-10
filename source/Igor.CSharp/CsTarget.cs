using Igor.Compiler;
using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.CSharp.Render;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.CSharp
{
    /// <summary>
    /// ITarget implementation for C# language
    /// </summary>
    public class Target : ITarget
    {
        public IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts)
        {
            var astModules = AstMapper.Map(modules);

            InitFrameworkVersion();

            var model = new CsModel();

            var generators = new List<ICsGenerator>();
            generators.Add(new CsTypeGenerator());
            generators.Add(new CsBinaryGenerator());
            generators.Add(new CsJsonGenerator());
            generators.Add(new CsStringGenerator());
            generators.Add(new CsXmlAttributeGenerator());
            generators.Add(new CsEqualityComparerGenerator());
            generators.Add(new CsServiceGenerator());
            generators.Add(new CsServiceMessagesGenerator());
            generators.Add(new CsBinaryServiceGenerator());
            generators.Add(new CsJsonServiceGenerator());
            generators.Add(new CsWebServiceGenerator());
            generators.AddRange(scripts.SelectMany(ReflectionHelper.CollectTypes<ICsGenerator>));

            foreach (var gen in generators)
                foreach (var mod in astModules)
                {
                    gen.Generate(model, mod);
                }

            return model.Files.Values.Select(file => new TargetFile(file.FileName, CsModuleRenderer.Render(file), file.IsEmpty)).ToList();
        }

        public string Name => "csharp";

        public System.Version DefaultVersion => new System.Version(5, 0);

        public IReadOnlyCollection<AttributeDescriptor> SupportedAttributes => CsAttributes.AllAttributes;

        private void InitFrameworkVersion()
        {
            var frameworkVersionString = Context.Instance.Attribute(CsAttributes.TargetFramework, null);
            CsFrameworkVersion frameworkVersion = null;
            if (frameworkVersionString != null && !CsFrameworkVersion.TryParse(frameworkVersionString, out frameworkVersion))
                Context.Instance.CompilerOutput.Error(Location.NoLocation, $"Failed to parse framework version '{frameworkVersionString}'", ProblemCode.TargetSpecificProblem);
            if (frameworkVersion == null)
                frameworkVersion = CsFrameworkVersion.Default;
            CsVersion.FrameworkVersion = frameworkVersion;
        }
    }
}
