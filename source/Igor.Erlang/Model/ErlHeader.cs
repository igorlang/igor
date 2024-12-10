using System.Collections.Generic;

namespace Igor.Erlang.Model
{
    public class ErlRecordField
    {
        public string Name { get; }
        public string Default { get; set; }
        public string TypeSpec { get; set; }
        public string Comment { get; set; }

        public ErlRecordField(string name)
        {
            Name = name;
        }
    }

    public class ErlRecord
    {
        public string Name { get; }
        public string Comment { get; set; }
        internal readonly List<ErlRecordField> Fields = new List<ErlRecordField>();

        public ErlRecord(string name)
        {
            Name = name;
        }

        public ErlRecordField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new ErlRecordField(name));
        }
    }

    public class ErlHeader
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }

        internal readonly List<ErlRecord> Records = new List<ErlRecord>();

        internal readonly List<string> Defines = new List<string>();

        internal bool IsEmpty => Records.Count == 0 && Defines.Count == 0;

        internal ErlHeader(string name)
        {
            this.Name = name;
            var includePath = Context.Instance.Attribute(ErlAttributes.IncludePath, "include");
            this.FileName = $"{includePath}/{name}";
        }

        public ErlRecord Record(string name)
        {
            return Records.GetOrAdd(name, r => r.Name, () => new ErlRecord(name));
        }

        public void Define(string declaration)
        {
            Defines.Add(declaration);
        }

        public void Define(string name, string value)
        {
            Define($"-define({name}, {value}).");
        }
    }
}
