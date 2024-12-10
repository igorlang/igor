using Igor.UE4.AST;
using Igor.UE4.Model;

namespace Igor.UE4
{
    /// <summary>
    /// Interface for UE4 generator.
    /// Custom UE4 generators should implement this interface.
    /// </summary>
    public interface IUeGenerator
    {
        /// <summary>
        /// Called to generate/modify target model for the given Igor module
        /// </summary>
        /// <param name="model">UE4 model</param>
        /// <param name="mod">Igor module AST</param>
        void Generate(UeModel model, Module mod);
    }
}
