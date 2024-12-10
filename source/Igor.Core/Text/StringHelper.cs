using System;

namespace Igor.Text
{
    /// <summary>
    /// String processing static routines and extension methods
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Replace any space sequence with a single space, except the starting indentation sequence
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveDoubleSpaces(string str)
        {
            var trim = str.TrimStart(' ');
            var indent = str.Length - trim.Length;
            var stop = false;
            while (!stop)
            {
                var repl = trim.Replace("  ", " ");
                stop = repl == trim;
                trim = repl;
            }
            return trim.Indent(indent);
        }

        /// <summary>
        /// Finds the longest common prefix of two strings.
        /// </summary>
        /// <param name="str1">First string</param>
        /// <param name="str2">Second string</param>
        /// <param name="prefix">Returns the common prefix of <paramref name="str1"/> and <paramref name="str2"/></param>
        /// <param name="tail1">Returns the remaining part of <paramref name="str1"/></param>
        /// <param name="tail2">Returns the remaining part of <paramref name="str2"/></param>
        public static void LongestCommonPrefix(string str1, string str2, out string prefix, out string tail1, out string tail2)
        {
            if (string.IsNullOrEmpty(str1))
            {
                prefix = string.Empty;
                tail1 = string.Empty;
                tail2 = str2 ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(str2))
            {
                prefix = string.Empty;
                tail1 = str1;
                tail2 = string.Empty;
            }
            else if (str1 == str2)
            {
                prefix = str1;
                tail1 = string.Empty;
                tail2 = string.Empty;
            }
            else
            {
                var i = 0;
                while (i < str1.Length && i < str2.Length && str1[i] == str2[i])
                    i++;
                prefix = str1.Substring(0, i);
                tail1 = str1.Substring(i);
                tail2 = str2.Substring(i);
            }
        }

        /// <summary>
        /// Finds the longest common prefix of two strings with respect to the separator character.
        /// </summary>
        /// <example>
        /// <code>
        /// LongestCommonPrefix("System.Collections.Generic", "System.Collections.Immutable", '.', out var prefix, out var tail1, out var tail2);
        /// </code>
        /// would lead to <paramref name="prefix"/> == "System.Collections", <paramref name="tail1"/> == "Generic", <paramref name="tail2"/> == "Immutable"
        /// </example>
        /// <param name="str1">First string</param>
        /// <param name="str2">Second string</param>
        /// <param name="sep">Separator character</param>
        /// <param name="prefix">Returns the common prefix of <paramref name="str1"/> and <paramref name="str2"/></param>
        /// <param name="tail1">Returns the remaining part of <paramref name="str1"/></param>
        /// <param name="tail2">Returns the remaining part of <paramref name="str2"/></param>
        public static void LongestCommonPrefix(string str1, string str2, char sep, out string prefix, out string tail1, out string tail2)
        {
            if (string.IsNullOrEmpty(str1))
            {
                prefix = string.Empty;
                tail1 = string.Empty;
                tail2 = str2 ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(str2))
            {
                prefix = string.Empty;
                tail1 = str1;
                tail2 = string.Empty;
            }
            else if (str1 == str2)
            {
                prefix = str1;
                tail1 = string.Empty;
                tail2 = string.Empty;
            }
            else
            {
                int i = 0;
                int lastSepIndex = -1;
                while (i < str1.Length && i < str2.Length && str1[i] == str2[i])
                {
                    if (str1[i] == sep)
                        lastSepIndex = i;
                    i++;
                }
                if (i == str1.Length && str2[i] == sep)
                {
                    prefix = str1;
                    tail1 = string.Empty;
                    tail2 = str2.Substring(i + 1);
                }
                else if (i == str2.Length && str1[i] == sep)
                {
                    prefix = str2;
                    tail1 = str1.Substring(i + 1);
                    tail2 = string.Empty;
                }
                else if (lastSepIndex < 0)
                {
                    prefix = string.Empty;
                    tail1 = str1;
                    tail2 = str2;
                }
                else
                {
                    prefix = str1.Substring(0, lastSepIndex);
                    tail1 = str1.Substring(lastSepIndex + 1);
                    tail2 = str2.Substring(lastSepIndex + 1);
                }
            }
        }

        /// <summary>
        /// Finds the longest common prefix of two strings with respect to the separator character.
        /// </summary>
        /// <example>
        /// <code>
        /// LongestCommonPrefix("System.Collections.Generic", "System.Collections.Immutable", '.', out var prefix, out var tail1, out var tail2);
        /// </code>
        /// would lead to <paramref name="prefix"/> == "System.Collections", <paramref name="tail1"/> == "Generic", <paramref name="tail2"/> == "Immutable"
        /// </example>
        /// <param name="str1">First string</param>
        /// <param name="str2">Second string</param>
        /// <param name="sep">Separator string</param>
        /// <param name="prefix">Returns the common prefix of <paramref name="str1"/> and <paramref name="str2"/></param>
        /// <param name="tail1">Returns the remaining part of <paramref name="str1"/></param>
        /// <param name="tail2">Returns the remaining part of <paramref name="str2"/></param>
        public static void LongestCommonPrefix(string str1, string str2, string sep, out string prefix, out string tail1, out string tail2)
        {
            if (string.IsNullOrEmpty(sep))
                throw new ArgumentException("Separator string must not be empty", nameof(sep));

            bool IsSeparator(string str, int i)
            {
                for (int j = 0; j < sep.Length; j++)
                {
                    if (i + j >= str.Length)
                        return false;
                    if (str[i + j] != sep[j])
                        return false;
                }
                return true;
            }

            if (string.IsNullOrEmpty(str1))
            {
                prefix = string.Empty;
                tail1 = string.Empty;
                tail2 = str2 ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(str2))
            {
                prefix = string.Empty;
                tail1 = str1;
                tail2 = string.Empty;
            }
            else if (str1 == str2)
            {
                prefix = str1;
                tail1 = string.Empty;
                tail2 = string.Empty;
            }
            else
            {
                int i = 0;
                int lastSepIndex = -1;
                while (i < str1.Length && i < str2.Length && str1[i] == str2[i])
                {
                    if (IsSeparator(str1, i))
                        lastSepIndex = i;
                    i++;
                }
                if (i == str1.Length && IsSeparator(str2, i))
                {
                    prefix = str1;
                    tail1 = string.Empty;
                    tail2 = str2.Substring(i + 1);
                }
                else if (i == str2.Length && IsSeparator(str2, i))
                {
                    prefix = str2;
                    tail1 = str1.Substring(i + 1);
                    tail2 = string.Empty;
                }
                else if (lastSepIndex < 0)
                {
                    prefix = string.Empty;
                    tail1 = str1;
                    tail2 = str2;
                }
                else
                {
                    prefix = str1.Substring(0, lastSepIndex);
                    tail1 = str1.Substring(lastSepIndex + sep.Length);
                    tail2 = str2.Substring(lastSepIndex + sep.Length);
                }
            }
        }

        public static string RelativeName(string name, string relativeToNs, char separator)
        {
            LongestCommonPrefix(name, relativeToNs, separator, out var prefix, out var part1, out var part2);
            return part1;
        }

        public static string RelativeName(string name, string relativeToNs, string separator)
        {
            LongestCommonPrefix(name, relativeToNs, separator, out var prefix, out var part1, out var part2);
            return part1;
        }

        /// <summary>
        /// Put string into quotes
        /// </summary>
        /// <param name="value">Source string</param>
        /// <param name="startQuote">Start quote string</param>
        /// <param name="endQuote">End quote string (same as start quote if null)</param>
        /// <returns>Quoted string</returns>
        public static string Quoted(this string value, string startQuote = null, string endQuote = null)
        {
            var q1 = startQuote ?? "\"";
            var q2 = endQuote ?? q1;
            return $"{q1}{value}{q2}";
        }

        /// <summary>
        /// Remove prefix from string, if present, or the original string otherwise.
        /// </summary>
        /// <param name="value">Source string</param>
        /// <param name="prefix">Prefix to remove</param>
        /// <param name="comparisonType">Comparison type</param>
        /// <returns>String without prefix</returns>
        public static string RemovePrefix(this string value, string prefix, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (value.StartsWith(prefix, comparisonType))
                return value.Substring(prefix.Length);
            else
                return value;
        }

        /// <summary>
        /// Remove suffix from string, if present, or the original string otherwise.
        /// </summary>
        /// <param name="value">Source string</param>
        /// <param name="suffix">Suffix to remove</param>
        /// <param name="comparisonType">Comparison type</param>
        /// <returns>String without suffix</returns>
        public static string RemoveSuffix(this string value, string suffix, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (value.EndsWith(suffix, comparisonType))
                return value.Substring(0, value.Length - suffix.Length);
            else
                return value;
        }
    }
}
