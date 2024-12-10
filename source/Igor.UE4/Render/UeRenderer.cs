using Igor.Text;

namespace Igor.UE4.Render
{
    public class UeRenderer : Renderer
    {
        public UeRenderer()
        {
            Tab = "    ";
            RemoveDoubleSpaces = true;
        }

        protected override EmptyLineMode GetEmptyLineModeBetweenLines(string prev, string next)
        {
            if (prev == "{")
                return EmptyLineMode.Forbid;
            else if (next.EndsWith("public:") || next.EndsWith("protected:") || next.EndsWith("private:"))
                return EmptyLineMode.Suggest;
            else if (prev.EndsWith(":"))
                return EmptyLineMode.Forbid;
            else if (prev.StartsWith("[") && prev.EndsWith("]"))
                return EmptyLineMode.Forbid;
            else if (next == "else")
                return EmptyLineMode.Forbid;
            else if (next == "}" || next == "};")
                return EmptyLineMode.Forbid;
            else
                return EmptyLineMode.Keep;
        }
    }
}
