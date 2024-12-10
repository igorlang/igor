using System.Collections.Generic;

namespace Igor.Sql.Model
{
    public class SqlEnumField
    {
        public string Name { get; }

        public SqlEnumField(string name)
        {
            Name = name;
        }
    }

    public class SqlEnum
    {
        public string Name { get; }

        public SqlEnum(string name)
        {
            Name = name;
        }

        internal readonly List<SqlEnumField> Fields = new List<SqlEnumField>();

        public SqlEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, t => t.Name, () => new SqlEnumField(name));
        }
    }
}
