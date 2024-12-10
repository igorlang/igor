using Igor.Text;

namespace Igor.UE4
{
    /// <summary>
    /// Routines for manipulating fully qualified C++ names
    /// </summary>
    public static class UeName
    {
        /// <summary>
        /// Combine fully qualified name from several parts, using :: as a separator
        /// </summary>
        /// <param name="parts">Name parts</param>
        /// <returns>Fully qualified name</returns>
        public static string Combine(params string[] parts)
        {
            string result = null;
            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    result = result == null ? part : $"{result}::{part}";
                }
            }
            return result;
        }

        public static string RelativeName(string name, string relativeToNs) => StringHelper.RelativeName(name, relativeToNs, "::");

        public static string RelativeName(string ns, string typeName, string relativeToNs)
        {
            return Combine(RelativeName(ns, relativeToNs), typeName);
        }

        internal static readonly string[] NamespaceSeparators = new[] { "::" };
    }
}
