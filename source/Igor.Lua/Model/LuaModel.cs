using System.Collections.Generic;

namespace Igor.Lua.Model
{
    public class LuaModel
    {
        internal LuaModel()
        {
        }

        internal Dictionary<string, LuaFile> Files { get; } = new Dictionary<string, LuaFile>();

        public LuaFile File(string path)
        {
            return Files.GetOrAdd(path, () => new LuaFile(path));
        }
    }
}
