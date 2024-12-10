using System.Collections.Generic;

namespace Igor.Erlang.Model
{
    public class ErlModel
    {
        internal ErlModel()
        {
        }

        internal readonly Dictionary<string, ErlModule> Modules = new Dictionary<string, ErlModule>();
        internal readonly Dictionary<string, ErlHeader> Headers = new Dictionary<string, ErlHeader>();

        public ErlModule Module(string path)
        {
            return Modules.GetOrAdd(path, () => new ErlModule(path));
        }

        public ErlHeader Header(string path)
        {
            return Headers.GetOrAdd(path, () => new ErlHeader(path));
        }
    }
}
