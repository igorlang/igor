using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Model
{
    public class IgorEnumField : IgorDeclaration
    {
        public long? Value { get; set; }

        public IgorEnumField(string name) : base(name)
        {
        }
    }

    public class IgorEnum : IgorDeclaration
    {
        public IReadOnlyList<IgorEnumField> Fields => fields;

        private readonly List<IgorEnumField> fields = new List<IgorEnumField>();
        public IgorEnum(string name) : base(name)
        {
        }

        public IgorEnumField Field(string name) => fields.GetOrAdd(name, f => f.Name, () => new IgorEnumField(name));
    }
}
