using System.Collections.Generic;
using System.Reflection;

namespace Igor.Compiler
{
    /// <summary>
    /// Interface for script compiler implementations
    /// </summary>
    public interface IScriptCompiler
    {
        /// <summary>
        /// Compile a script file to an assembly
        /// </summary>
        /// <param name="filenames">Script source filenames</param>
        /// <param name="compilerOutput">CompilerOutput instance for error reporting</param>
        /// <returns>Compiled assembly</returns>
        Assembly CompileFiles(IReadOnlyCollection<string> filenames, CompilerOutput compilerOutput);
    }
}
