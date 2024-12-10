using System.Collections.Generic;

namespace Igor.Go.Model
{
    public class GoPackage
    {
        internal GoPackage(string name)
        {
            Name = name;
        }

        public string Name { get; }

        internal Dictionary<string, GoFile> Files { get; } = new Dictionary<string, GoFile>();

        public GoFile File(string path)
        {
            return Files.GetOrAdd(path, () => new GoFile(path, Name));
        }
    }

    public class GoModel
    {
        internal GoModel()
        {
        }

        internal Dictionary<string, GoPackage> Packages { get; } = new Dictionary<string, GoPackage>();

        public GoPackage Package(string package)
        {
            return Packages.GetOrAdd(package, () => new GoPackage(package));
        }
    }
}
