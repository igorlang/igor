using Igor.Sql.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Sql.Render
{
    public class SqlPostgresRenderer : SqlRenderer
    {
        public string WriteTableField(SqlTableField field, bool primaryKey)
        {
            var notNull = field.NotNull ? " NOT NULL" : "";
            var defPrimaryKey = primaryKey ? " PRIMARY KEY" : "";
            var unique = field.Unique ? " UNIQUE" : "";
            var notEmpty = field.NotEmpty ? $" CHECK (TRIM({field.Name}) <> '')" : "";
            var value = field.Default == null ? "" : $" DEFAULT {field.Default}";
            var foreignKey = field.ForeignKey != null ? $" REFERENCES {field.ForeignKey.Table}({field.ForeignKey.Field})" : "";
            return $"{field.Name} {field.Type}{unique}{defPrimaryKey}{notNull}{value}{notEmpty}{foreignKey}";
        }

        public void WriteTable(SqlTable table)
        {
            if (!string.IsNullOrEmpty(table.Comment))
                Comment(table.Comment, "-- ");
            Line($"CREATE TABLE {table.Name}");
            Line("(");
            Indent();
            var primaryKeySize = table.PrimaryKey?.Fields.Count ?? 0;
            var lastLinePostfix = primaryKeySize > 1 ? "," : "";
            Blocks(table.Fields, f => WriteTableField(f, primaryKeySize == 1 && table.PrimaryKey.Fields.Contains(f.Name)), delimiter: ",", lastLinePostfix: lastLinePostfix);
            if (primaryKeySize > 1)
            {
                Line($"PRIMARY KEY({table.PrimaryKey.Fields.JoinStrings(", ")})");
            }
            Outdent();
            Line(")");
            Line("WITHOUT OIDS;");
        }

        public void WriteEnum(SqlEnum sqlEnum)
        {
            Line($"CREATE TYPE {sqlEnum.Name} AS ENUM ({sqlEnum.Fields.JoinStrings(", ", f => f.Name.Quoted("'"))});");
        }

        public override void WriteSql(SqlModel model)
        {
            ForEach(model.Enums, WriteEnum, true);
            ForEach(model.Tables, WriteTable, true);
        }
    }
}
