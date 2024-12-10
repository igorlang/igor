using Igor.TypeScript.AST;
using Igor.TypeScript.Model;

namespace Igor.TypeScript
{
    /// <summary>
    /// Interface for TypeScript generator.
    /// Custom TypeScript generators should implement this interface.
    /// </summary>
    public interface ITsGenerator
    {
        /// <summary>
        /// Called to generate/modify target model for the given Igor module
        /// </summary>
        /// <param name="model">TypeScript model</param>
        /// <param name="mod">Igor module AST</param>
        void Generate(TsModel model, Module mod);
    }
}
