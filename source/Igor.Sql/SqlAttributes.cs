using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Sql
{
    public static class SqlAttributes
    {
        public static readonly BoolAttributeDescriptor SqlAutoIncrement = new BoolAttributeDescriptor("sql.auto_increment", IgorAttributeTargets.TableField);
        public static readonly BoolAttributeDescriptor SqlPrimaryKey = new BoolAttributeDescriptor("sql.primary_key", IgorAttributeTargets.TableField);
        public static readonly BoolAttributeDescriptor SqlUnique = new BoolAttributeDescriptor("sql.unique", IgorAttributeTargets.TableField);
        public static readonly StringAttributeDescriptor SqlName = new StringAttributeDescriptor("sql.name", IgorAttributeTargets.Any);
        public static readonly StringAttributeDescriptor SqlAlias = new StringAttributeDescriptor("sql.alias", IgorAttributeTargets.Type, AttributeInheritance.Type);
        public static readonly StringAttributeDescriptor SqlDefault = new StringAttributeDescriptor("sql.default", IgorAttributeTargets.TableField);
        public static readonly EnumAttributeDescriptor<Notation> SqlNotation = new EnumAttributeDescriptor<Notation>("sql.notation", IgorAttributeTargets.Any, AttributeInheritance.Scope);

        public static IReadOnlyList<AttributeDescriptor> AllAttributes { get; }

        static SqlAttributes()
        {
            var props = typeof(SqlAttributes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            AllAttributes = props.Where(p => typeof(AttributeDescriptor).IsAssignableFrom(p.FieldType)).Select(p => (AttributeDescriptor)p.GetValue(null)).ToList();
        }
    }
}
