using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Elixir.Model
{
    public class ExStructField
    {
        public string Name { get; }
        public string Default { get; set; }
        public string TypeSpec { get; set; }
        public string Comment { get; set; }
        public bool Enforce { get; set; }

        public ExStructField(string name)
        {
            Name = name;
        }
    }

    public class ExStruct
    {

        internal readonly List<ExStructField> Fields = new List<ExStructField>();

        public bool IsException { get; set; }

        public ExStruct()
        {
        }

        public ExStructField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new ExStructField(name));
        }
    }

}
