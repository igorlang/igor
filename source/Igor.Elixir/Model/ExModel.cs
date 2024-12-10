using System.Collections.Generic;

namespace Igor.Elixir.Model
{
    public class ExModel
    {
        internal ExModel()
        {
        }

        internal readonly Dictionary<string, ExFile> Files = new Dictionary<string, ExFile>();

        public ExFile File(string path)
        {
            return Files.GetOrAdd(path, () => new ExFile(path));
        }
    }
}
