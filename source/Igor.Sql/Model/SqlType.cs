using Igor.Sql.AST;
using Igor.Text;
using System;
using System.Globalization;

namespace Igor.Sql
{
    public abstract class SqlType
    {
        public abstract string FormatValue(Value value);
    }

    public class SqlTypeBool : SqlType
    {
        public override string ToString() => "boolean";

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.Bool b: return b.Value.ToString().ToUpper();
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeText : SqlType
    {
        public override string ToString() => "text";

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.String s: return s.Value.ToString().Quoted("'");
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeInteger : SqlType
    {
        public IntegerType IntegerType { get; }
        public bool AutoIncrement { get; }

        public SqlTypeInteger(IntegerType integerType, bool autoIncrement)
        {
            IntegerType = integerType;
            AutoIncrement = autoIncrement;
        }

        public override string ToString()
        {
            switch (IntegerType)
            {
                case IntegerType.ULong when AutoIncrement:
                case IntegerType.Long when AutoIncrement:
                    return "bigserial";
                case IntegerType.Byte when AutoIncrement:
                case IntegerType.SByte when AutoIncrement:
                case IntegerType.Short when AutoIncrement:
                case IntegerType.UShort when AutoIncrement:
                case IntegerType.Int when AutoIncrement:
                case IntegerType.UInt when AutoIncrement:
                    return "serial";
                case IntegerType.Byte:
                case IntegerType.SByte:
                case IntegerType.Short:
                    return "smallint";
                case IntegerType.UShort:
                case IntegerType.Int:
                    return "integer";
                case IntegerType.UInt:
                case IntegerType.ULong:
                case IntegerType.Long:
                default:
                    return "bigint";
            }
        }

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.Integer i: return i.Value.ToString(CultureInfo.InvariantCulture);
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeFloat : SqlType
    {
        public FloatType FloatType { get; }

        public SqlTypeFloat(FloatType floatType)
        {
            FloatType = floatType;
        }

        public override string ToString()
        {
            switch (FloatType)
            {
                case FloatType.Double:
                    return "double precision";
                default:
                    return "real";
            }
        }

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.Integer i: return i.Value.ToString(CultureInfo.InvariantCulture);
                case Value.Float f: return f.Value.ToString(CultureInfo.InvariantCulture);
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeEnum : SqlType
    {
        public EnumForm EnumForm { get; }

        public SqlTypeEnum(EnumForm enumForm)
        {
            EnumForm = enumForm;
        }

        public override string ToString() => EnumForm.sqlName;

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.Enum e: return e.Field.sqlName.Quoted("'");
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeJson : SqlType
    {
        public override string ToString() => "jsonb";

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                case Value.EmptyObject _: return "'{}'";
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }

    public class SqlTypeAlias : SqlType
    {
        public string Alias { get; }

        public SqlTypeAlias(string alias)
        {
            Alias = alias;
        }

        public override string ToString() => Alias;

        public override string FormatValue(Value value)
        {
            switch (value)
            {
                default: throw new NotSupportedException(value.ToString());
            }
        }
    }
}
