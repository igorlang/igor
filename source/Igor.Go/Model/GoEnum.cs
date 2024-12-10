using System.Collections.Generic;

namespace Igor.Go.Model
{
    public class GoEnumField
    {
        public string Name { get; }
        public string Value { get; set; }
        public string Comment { get; set; }

        public GoEnumField(string name)
        {
            Name = name;
        }
    }

    public class GoEnum : GoTypeDeclaration
    {
        public string BaseType { get; set; }

        internal List<GoEnumField> Fields { get; } = new List<GoEnumField>();

        internal GoEnum(string name) : base(name)
        {
        }

        public GoEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new GoEnumField(name));
        }
    }
}
