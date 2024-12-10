using Igor.Sql.AST;
using Igor.Sql.Model;
using System.Linq;

namespace Igor.Sql.Generators
{
    public class SqlTableGenerator : ISqlGenerator
    {
        public void Generate(SqlModel model, Module mod)
        {
            foreach (var enumForm in mod.Enums)
            {
                if (enumForm.sqlEnabled)
                    GenerateEnum(model, enumForm);
            }
            foreach (var table in mod.Tables)
            {
                if (table.sqlEnabled)
                    GenerateTable(model, table);
            }
        }

        public void GenerateTable(SqlModel model, TableForm table)
        {
            var t = model.Table(table.sqlName);
            t.Comment = table.Annotation;
            foreach (var field in table.Fields)
            {
                var f = t.Field(field.sqlName);
                f.Comment = field.Annotation;
                f.Type = field.sqlType;
                f.NotNull = !(field.Type is BuiltInType.Optional) && !field.sqlPrimaryKey;
                f.Unique = field.sqlUnique;
                f.NotEmpty = field.Attribute(CoreAttributes.NotEmpty, false);
                if (field.sqlDefault != null)
                    f.Default = field.sqlDefault;
                else if (field.DefaultValue != null)
                    f.Default = field.sqlType.FormatValue(field.DefaultValue);
                else if (field.Type is BuiltInType.Optional)
                    f.Default = "NULL";
                if (field.ForeignKey != null)
                    f.ForeignKey = new SqlForeignKey(field.ForeignKey.Table.sqlName, field.ForeignKey.sqlName);
            }
            var primaryKeyFields = table.Fields.Where(f => f.sqlPrimaryKey).Select(f => f.sqlName).ToList();
            if (primaryKeyFields.Any())
                t.PrimaryKey = new SqlPrimaryKey(primaryKeyFields);
        }

        public void GenerateEnum(SqlModel model, EnumForm enumForm)
        {
            var e = model.Enum(enumForm.sqlName);
            foreach (var field in enumForm.Fields)
            {
                e.Field(field.sqlName);
            }
        }
    }
}
