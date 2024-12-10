using System.Collections.Generic;
using System.Reflection;

namespace Igor
{
    /// <summary>
    /// Interface that should be implemented by all Igor targets.
    /// </summary>
    public interface ITarget
    {
        /// <summary>
        /// Called to generate target files from the source Igor modules.
        /// </summary>
        /// <param name="modules">List of module declarations (only source modules, imported modules are not included)</param>
        /// <param name="scripts">List of compiled extension scripts assemblies</param>
        /// <returns>List of generated files</returns>
        IReadOnlyCollection<TargetFile> Generate(IReadOnlyList<Declarations.Module> modules, IReadOnlyList<Assembly> scripts);

        /// <summary>
        /// Target name. Used as an argument to -t compiler option, and as an attribute target.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Default target version, if Version option is not provided by the user.
        /// Return null if target doesn't support versions.
        /// </summary>
        System.Version DefaultVersion { get; }

        /// <summary>
        /// Returns the list of built-in target-specific attributes.
        /// </summary>
        IReadOnlyCollection<AttributeDescriptor> SupportedAttributes { get; }
    }
}
