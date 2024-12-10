using Igor.Sql.AST;
using Igor.Sql.Model;

namespace Igor.Sql
{
    public interface ISqlGenerator
    {
        void Generate(SqlModel model, Module mod);
    }
}
