using Igor.Text;

namespace Igor.Erlang.Render
{
    public class ErlRenderer : Renderer
    {
        public ErlRenderer()
        {
            Tab = "    ";
            RemoveDoubleSpaces = true;
            EndWithEmptyLine = true;
        }

        protected override EmptyLineMode GetEmptyLineModeBetweenLines(string prev, string next)
        {
            if (prev.EndsWith("."))
                return EmptyLineMode.Allow;
            else if (prev.StartsWith("%"))
                return EmptyLineMode.Allow;
            else
                return EmptyLineMode.Disable;
        }
    }
}
