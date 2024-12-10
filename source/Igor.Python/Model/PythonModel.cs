using System.Collections.Generic;

namespace Igor.Python.Model
{
    public class PythonModel
    {
        internal PythonModel()
        {
        }

        internal Dictionary<string, PythonFile> Files { get; } = new Dictionary<string, PythonFile>();

        public PythonFile File(string path)
        {
            return Files.GetOrAdd(path, () => new PythonFile(path));
        }
    }
}
