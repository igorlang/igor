using System.Collections.Generic;

namespace Igor.Python.Model
{
    public class PythonClass
    {
        public string Name { get; }
        public string BaseClass { get; set; }

        internal PythonClass(string name)
        {
            this.Name = name;
        }

        internal List<string> Properties { get; } = new List<string>();
        internal List<string> Functions { get; } = new List<string>();

        public void Property(string prop)
        {
            Properties.Add(prop);
        }

        public void Function(string fun)
        {
            Functions.Add(fun);
        }
    }
}
