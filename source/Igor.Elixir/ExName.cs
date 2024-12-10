using Igor.Text;

namespace Igor.Elixir
{
    public static class ExName
    {
        public static string Combine(params string[] parts)
        {
            string result = null;
            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    result = result == null ? part : $"{result}.{part}";
                }
            }
            return result;
        }

        public static string RelativeName(string name, string relativeToNs) => StringHelper.RelativeName(name, relativeToNs, '.');

        public static string RelativeName(string ns, string typeName, string relativeToNs)
        {
            return Combine(RelativeName(ns, relativeToNs), typeName);
        }
    }
}