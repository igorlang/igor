using System.Collections.Generic;

namespace Igor.Python.Model
{
    public class PythonImport
    {
        public string Module { get; }
        internal List<string> ImportTypes { get; } = new List<string>();

        public PythonImport(string module)
        {
            Module = module;
        }

        public void Types(params string[] types)
        {
            foreach (var type in types)
            {
                if (!ImportTypes.Contains(type))
                    ImportTypes.Add(type);
            }
        }
    }

    public class PythonFile
    {
        public string FileName { get; private set; }

        internal bool IsEmpty => Enums.Count == 0 && Classes.Count == 0 && Declarations.Count == 0;

        internal PythonFile(string name)
        {
            this.FileName = name;
        }

        internal List<PythonImport> Imports { get; } = new List<PythonImport>();
        internal List<PythonEnum> Enums { get; } = new List<PythonEnum>();
        internal List<PythonClass> Classes { get; } = new List<PythonClass>();
        internal List<string> Declarations { get; } = new List<string>();

        public PythonEnum Enum(string name)
        {
            return Enums.GetOrAdd(name, f => f.Name, () => new PythonEnum(name));
        }

        public PythonClass Class(string name)
        {
            return Classes.GetOrAdd(name, f => f.Name, () => new PythonClass(name));
        }

        public PythonImport Import(string module)
        {
            return Imports.GetOrAdd(module, f => f.Module, () => new PythonImport(module));
        }

        public void Declare(string block)
        {
            Declarations.Add(block);
        }
    }
}
