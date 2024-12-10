using System.Collections.Generic;

namespace Igor.Lua.Model
{
    public class LuaFile
    {
        public string FileName { get; private set; }

        internal bool IsEmpty => Enums.Count == 0 && Classes.Count == 0;

        internal LuaFile(string name)
        {
            this.FileName = name;
        }

        internal List<string> Requires { get; } = new List<string>();
        internal List<string> Namespaces { get; } = new List<string>();
        internal List<LuaEnum> Enums { get; } = new List<LuaEnum>();
        internal List<LuaClass> Classes { get; } = new List<LuaClass>();
        internal List<string> Declarations { get; } = new List<string>();

        public LuaEnum Enum(string name)
        {
            return Enums.GetOrAdd(name, f => f.Name, () => new LuaEnum(name));
        }

        public LuaClass Class(string name)
        {
            return Classes.GetOrAdd(name, f => f.Name, () => new LuaClass(name));
        }

        public void Namespace(string ns)
        {
            if (!Namespaces.Contains(ns))
            {
                Namespaces.Add(ns);
            }
        }

        public void Require(string script)
        {
            if (!Requires.Contains(script))
            {
                Requires.Add(script);
            }
        }

        public void Declare(string block)
        {
            Declarations.Add(block);
        }
    }
}
