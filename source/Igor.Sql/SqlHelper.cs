using Igor.Sql.AST;
using System;

namespace Igor.Sql
{
    public static class Helper
    {
        public static SqlType GetSqlType(TableField field, IType type)
        {
            var alias = field?.Attribute(SqlAttributes.SqlAlias);
            if (alias != null)
            {
                return new SqlTypeAlias(alias);
            }

            switch (type)
            {
                case BuiltInType.String _:
                case BuiltInType.Atom _:
                    return new SqlTypeText();

                case BuiltInType.Bool _:
                    return new SqlTypeBool();

                case BuiltInType.Json _:
                    return new SqlTypeJson();

                case BuiltInType.Integer integerType:
                    return new SqlTypeInteger(integerType.Type, field.sqlAutoIncrement);

                case BuiltInType.Float floatType:
                    return new SqlTypeFloat(floatType.Type);

                case BuiltInType.Optional optionalType:
                    return GetSqlType(field, optionalType.ItemType);

                case TypeForm form:
                    return form.sqlType(field);

                default:
                    throw new NotImplementedException(type.GetType().ToString());
            }
        }
    }
}
