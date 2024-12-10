using Igor.Text;
using System;

namespace Igor.Sql.AST
{
    public partial class Statement
    {
        public Notation sqlNotation => Attribute(SqlAttributes.SqlNotation, Notation.None);
        public string sqlName => Attribute(SqlAttributes.SqlName, Name.Format(sqlNotation));
        public bool sqlEnabled => Attribute(CoreAttributes.Enabled, false);
    }

    public partial class TypeForm
    {
        public virtual SqlType sqlType(TableField field) => throw new NotSupportedException();
    }

    public partial class DefineForm
    {
        public override SqlType sqlType(TableField field) => Helper.GetSqlType(field, Type);
    }

    public partial class EnumForm
    {
        public override SqlType sqlType(TableField field) => new SqlTypeEnum(this);
    }

    public partial class TableField
    {
        public bool sqlAutoIncrement => Attribute(SqlAttributes.SqlAutoIncrement, false);
        public bool sqlPrimaryKey => Attribute(SqlAttributes.SqlPrimaryKey, false);
        public bool sqlUnique => Attribute(SqlAttributes.SqlUnique, false);
        public SqlType sqlType => Helper.GetSqlType(this, Type);
        public string sqlDefault => Attribute(SqlAttributes.SqlDefault);
    }
}
