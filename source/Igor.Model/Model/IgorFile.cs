using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Model
{
    public class IgorFile
    {
        public IReadOnlyList<IgorModule> Modules => modules;
        public IReadOnlyList<string> Usings => usings;

        public string FileName { get; }
        public string Comment { get; set; }

        private List<IgorModule> modules = new List<IgorModule>();
        private List<string> usings = new List<string>();

        public IgorModule Module(string name) => modules.GetOrAdd(name, m => m.Name, () => new IgorModule(name));

        public IgorFile(string fileName)
        {
            FileName = fileName;
        }
    }
}
