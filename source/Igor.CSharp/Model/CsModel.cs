using System.Collections.Generic;

namespace Igor.CSharp.Model
{
    public class CsModel
    {
        internal CsModel()
        {
        }

        internal Dictionary<string, CsFile> Files { get; } = new Dictionary<string, CsFile>();

        public CsFile File(string path)
        {
            return Files.GetOrAdd(path, () => new CsFile(path));
        }
    }
}
