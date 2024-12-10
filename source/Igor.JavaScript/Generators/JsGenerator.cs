using Igor.JavaScript.AST;
using Igor.JavaScript.Model;

namespace Igor.JavaScript
{
    /// <summary>
    /// Interface for JavaScript generator.
    /// Custom JavaScript generators should implement this interface.
    /// </summary>
    public interface IJsGenerator
    {
        /// <summary>
        /// Called to generate/modify target model for the given Igor module
        /// </summary>
        /// <param name="model">JavaScript model</param>
        /// <param name="mod">Igor module AST</param>
        void Generate(JsModel model, Module mod);
    }
}
