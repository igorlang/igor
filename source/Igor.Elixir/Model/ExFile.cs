using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igor.Elixir.Model
{
    public class ExFile
    {
        public string FileName { get; }

        public ExFile(string filename)
        {
            FileName = filename;
        }

        public bool IsEmpty => !Modules.Any();

        internal readonly Dictionary<string, ExModule> Modules = new Dictionary<string, ExModule>();

        public ExModule Module(string name)
        {
            return Modules.GetOrAdd(name, () => new ExModule(name));
        }
    }
}
