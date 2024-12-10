using System.Collections.Generic;

namespace Igor.JavaScript.Model
{
    /// <summary>
    /// JavaScript target model
    /// </summary>
    public class JsModel
    {
        internal JsModel()
        {
        }

        internal Dictionary<string, JsFile> Files { get; } = new Dictionary<string, JsFile>();

        /// <summary>
        /// Get file model by name
        /// </summary>
        /// <param name="path">File name with .ts extension, relative to output folder</param>
        /// <returns>File model</returns>
        public JsFile File(string path)
        {
            return Files.GetOrAdd(path, () => new JsFile(path));
        }
    }
}
