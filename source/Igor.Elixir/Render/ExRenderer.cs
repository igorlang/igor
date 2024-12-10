using System;
using System.Collections.Generic;
using System.Text;
using Igor.Text;

namespace Igor.Elixir.Render
{
    public class ExRenderer : Renderer
    {
        protected ExRenderer()
        {
            Tab = "  ";
            RemoveDoubleSpaces = false;
            EndWithEmptyLine = true;
        }

        protected override EmptyLineMode GetEmptyLineModeBetweenLines(string prev, string next)
        {
            if (prev.StartsWith("defmodule"))
                return EmptyLineMode.Enforce;
            if (prev.EndsWith("end"))
                return EmptyLineMode.Allow;
            else
                return EmptyLineMode.Keep;
        }

        public void WriteDoc(string doc)
        {
            if (doc == null)
                return;
            Line(@"@doc """"""");
            Comment(doc, "");
            Line(@"""""""");
        }

        public static Renderer Create() => new ExRenderer();
    }
}
