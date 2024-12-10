using System.Collections.Generic;

namespace Igor.Sql.Model
{
    public class SqlForeignKey
    {
        public string Table { get; }
        public string Field { get; }

        public SqlForeignKey(string table, string field)
        {
            Table = table;
            Field = field;
        }
    }

    public class SqlPrimaryKey
    {
        public IReadOnlyList<string> Fields { get; }

        public SqlPrimaryKey(IReadOnlyList<string> fields)
        {
            Fields = fields;
        }
    }

    public class SqlIndex
    {
        public string Name { get; }

        public IReadOnlyList<string> Fields { get; set; }

        public bool Unique { get; set; }

        public SqlIndex(string name)
        {
            Name = name;
        }
    }

    public class SqlTableField
    {
        public string Name { get; }
        public string Comment { get; set; }

        public SqlTableField(string name)
        {
            Name = name;
        }

        public SqlType Type { get; set; }
        public bool NotNull { get; set; }
        public bool Unique { get; set; }
        public bool NotEmpty { get; set; }
        public string Default { get; set; }
        public SqlForeignKey ForeignKey { get; set; }
    }

    public class SqlTable
    {
        public string Name { get; }
        public string Comment { get; set; }
        public SqlPrimaryKey PrimaryKey { get; set; }
        public List<SqlIndex> Indexes { get; } = new List<SqlIndex>();

        public SqlTable(string name)
        {
            Name = name;
        }

        internal readonly List<SqlTableField> Fields = new List<SqlTableField>();

        public SqlTableField Field(string name)
        {
            return Fields.GetOrAdd(name, t => t.Name, () => new SqlTableField(name));
        }

        public SqlIndex Index(string name)
        {
            return Indexes.GetOrAdd(name, t => t.Name, () => new SqlIndex(name));
        }
    }
}
