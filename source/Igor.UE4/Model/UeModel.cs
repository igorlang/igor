using System.Collections.Generic;

namespace Igor.UE4.Model
{
    /// <summary>
    /// Unreal Engine 4 target model
    /// </summary>
    public class UeModel
    {
        internal UeModel()
        {
        }

        internal readonly Dictionary<string, UeCppFile> CppFiles = new Dictionary<string, UeCppFile>();
        internal readonly Dictionary<string, UeHFile> HFiles = new Dictionary<string, UeHFile>();

        /// <summary>
        /// Get cpp file model by path
        /// </summary>
        /// <param name="path">File path with .cpp extension, relative to output folder</param>
        /// <returns>Cpp file model</returns>
        public UeCppFile CppFile(string path)
        {
            return CppFiles.GetOrAdd(path, () => new UeCppFile(path));
        }

        /// <summary>
        /// Get header file model by path
        /// </summary>
        /// <param name="path">File path with .h extension, relative to output folder</param>
        /// <returns>Header file model</returns>
        public UeHFile HFile(string path)
        {
            return HFiles.GetOrAdd(path, () => new UeHFile(path));
        }
    }
}
