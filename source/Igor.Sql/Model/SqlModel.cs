using System.Collections.Generic;

namespace Igor.Sql.Model
{
    public class SqlModel
    {
        internal SqlModel()
        {
        }

        internal readonly List<SqlTable> Tables = new List<SqlTable>();
        internal readonly List<SqlEnum> Enums = new List<SqlEnum>();

        public SqlTable Table(string name)
        {
            return Tables.GetOrAdd(name, t => t.Name, () => new SqlTable(name));
        }

        public SqlEnum Enum(string name)
        {
            return Enums.GetOrAdd(name, t => t.Name, () => new SqlEnum(name));
        }
    }
}
