using Igor.Sql.Model;
using Igor.Text;

namespace Igor.Sql.Render
{
    public abstract class SqlRenderer : Renderer
    {
        public abstract void WriteSql(SqlModel model);

        public static string Render<T>(SqlModel model) where T : SqlRenderer, new()
        {
            var renderer = new T();
            renderer.WriteSql(model);
            return renderer.Build();
        }
    }
}
