using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.TypeScript
{
    public static class TsCodeUtils
    {
        public static string MakeArray(IEnumerable<string> values)
        {
            var list = values.ToList();
            if (list.Count == 0)
                return "[]";
            else
                return $"[{values.JoinStrings(", ")}]";
        }

        public static string As(string value, string type) => $"{value} as {type}";
    }
}
