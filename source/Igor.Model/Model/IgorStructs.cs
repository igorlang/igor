using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Model
{
    public class IgorStructField : IgorDeclaration
    {
        public string Type { get; set; }
        public string Default { get; set; }
        public bool IsTag { get; set; }

        public IgorStructField(string name) : base(name)
        {
        }
    }

    public enum IgorStructType
    {
        Record,
        Variant,
        Interface,
    }

    public class IgorStruct : IgorDeclaration
    {
        public IgorStructType StructType { get; set; }
        public string Ancestor { get; set; }
        public string TagValue { get; set; }
        public IReadOnlyList<IgorStructField> Fields => fields;

        private readonly List<IgorStructField> fields = new List<IgorStructField>();
        public IgorStruct(string name) : base(name)
        {
        }

        public IgorStructField Field(string name) => fields.GetOrAdd(name, f => f.Name, () => new IgorStructField(name));
    }
}
