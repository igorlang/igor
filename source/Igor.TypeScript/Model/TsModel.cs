using System.Collections.Generic;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// TypeScript target model
    /// </summary>
    public class TsModel
    {
        internal TsModel()
        {
        }

        internal Dictionary<string, TsFile> Files { get; } = new Dictionary<string, TsFile>();

        /// <summary>
        /// Get file model by name
        /// </summary>
        /// <param name="name">Module name, used for import</param>
        /// <param name="path">File name with .ts extension, relative to output folder</param>
        /// <returns>File model</returns>
        public TsFile File(string name, string path)
        {
            return Files.GetOrAdd(path, () => new TsFile(name, path));
        }
    }
}
