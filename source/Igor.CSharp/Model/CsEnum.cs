using System.Collections.Generic;

namespace Igor.CSharp.Model
{
    public class CsEnumField : CsAttributeHost
    {
        public string Name { get; }
        public long Value { get; set; }

        public CsEnumField(string name)
        {
            Name = name;
        }
    }

    public class CsEnum : CsAttributeHost
    {
        public string Name { get; }
        public string IntType { get; set; }

        internal List<CsEnumField> Fields { get; } = new List<Model.CsEnumField>();

        internal CsEnum(string name)
        {
            this.Name = name;
        }

        public CsEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new CsEnumField(name));
        }
    }
}
