using System.Collections.Generic;

namespace Igor.Python.Model
{
    public class PythonEnumField
    {
        public string Name { get; }
        public long Value { get; set; }

        public PythonEnumField(string name)
        {
            Name = name;
        }
    }

    public class PythonEnum
    {
        public string Name { get; }
        public string Namespace { get; set; }

        internal List<PythonEnumField> Fields { get; } = new List<PythonEnumField>();

        internal PythonEnum(string name)
        {
            this.Name = name;
        }

        public PythonEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new PythonEnumField(name));
        }
    }
}
