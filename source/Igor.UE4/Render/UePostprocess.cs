using Igor.Text;

namespace Igor.UE4.Render
{
    public static class UePostprocess
    {
        public static string Postprocess(string text)
        {
            bool indentCond(string txt)
            {
                return txt.EndsWith(":") || txt.EndsWith("{");
            }

            bool outdentCond(string txt)
            {
                var endsWithBracket = txt.EndsWith("}") || txt.EndsWith("};");
                return txt.EndsWith(":") || (endsWithBracket && !txt.Contains("{"));
            }

            var tabbedText = TextHelper.Tabulize(text, indentCond, outdentCond, "\t");
            return TextHelper.FixEndLine(TextHelper.RemoveDoubleEmptyLines(tabbedText));
        }
    }
}
