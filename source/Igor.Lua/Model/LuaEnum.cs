using System.Collections.Generic;

namespace Igor.Lua.Model
{
    public class LuaEnumField
    {
        public string Name { get; }
        public long Value { get; set; }

        public LuaEnumField(string name)
        {
            Name = name;
        }
    }

    public class LuaEnum
    {
        public string Name { get; }
        public string Namespace { get; set; }
        public EnumStyle Style { get; set; } = EnumStyle.Table;

        internal List<LuaEnumField> Fields { get; } = new List<LuaEnumField>();

        internal LuaEnum(string name)
        {
            this.Name = name;
        }

        public LuaEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new LuaEnumField(name));
        }
    }
}
