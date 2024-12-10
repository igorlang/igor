using System.Collections.Generic;

namespace Igor.Lua.Model
{
    public class LuaClass
    {
        public string Name { get; }
        public string Super { get; set; }
        public string Namespace { get; set; }
        public RecordStyle Style { get; set; } = RecordStyle.Class;

        internal LuaClass(string name)
        {
            this.Name = name;
        }

        internal List<string> Functions { get; } = new List<string>();

        public void Function(string fun)
        {
            Functions.Add(fun);
        }
    }
}
