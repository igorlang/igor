using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Environment;

namespace Igor.Text
{
    /// <summary>
    /// Text (string containing new line characters) processing static functions and extension methods.
    /// </summary>
    public static class TextHelper
    {
        private static string IndentLine(string str, int i)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            else
                return new string(' ', i) + str;
        }

        /// <summary>
        /// Indent text with a given number of spaces
        /// </summary>
        /// <param name="str">Source text</param>
        /// <param name="i">Number of spaces</param>
        /// <returns>Indented text</returns>
        public static string Indent(this string str, int i)
        {
            var lines = str.Split('\n');
            return lines.JoinLines(s => IndentLine(s, i));
        }

        /// <summary>
        /// Comment text
        /// </summary>
        /// <param name="str">Source text</param>
        /// <param name="prefix">Comment prefix</param>
        /// <returns>Commented text</returns>
        public static string Comment(this string str, string prefix)
        {
            var lines = str.Split('\n');
            return lines.JoinLines(s => prefix + s);
        }

        /// <summary>
        /// Replace all line breaks with the platform-specific new line characters (e.g. "\r\n")
        /// </summary>
        /// <param name="str">Source text</param>
        /// <returns>Text with valid new line characters</returns>
        public static string FixLineBreaks(this string str)
        {
            return string.Join(NewLine, Lines(str));
        }

        /// <summary>
        /// Split text to lines (without trailing space and newline characters)
        /// </summary>
        /// <param name="str">Source text</param>
        /// <returns>Array of lines</returns>
        public static string[] Lines(string str)
        {
            return str.Split('\n').Select(s => s.TrimEnd('\r')).ToArray();
        }

        public static string JoinLines<T>(this IEnumerable<T> values, Func<T, string> format, bool trailingNewLine = false)
        {
            return values.Select(format).JoinLines(trailingNewLine);
        }

        public static string JoinLines(this IEnumerable<string> strings, bool trailingNewLine = false)
        {
            if (trailingNewLine)
                return string.Join("", strings.Select(s => s + "\n"));
            else
                return string.Join("\n", strings);
        }

        /// <summary>
        /// Join a sequence of values using string formatter function and a separator
        /// </summary>
        /// <param name="values">Source values</param>
        /// <param name="format">Value to string formatter</param>
        /// <param name="sep">Separator string</param>
        /// <returns>Combined string</returns>
        public static string JoinStrings<T>(this IEnumerable<T> values, string sep, Func<T, string> format)
        {
            return string.Join(sep, values.Select(format));
        }

        /// <summary>
        /// Join a sequence of values using string formatter function 
        /// </summary>
        /// <param name="values">Source values</param>
        /// <param name="format">Value to string formatter</param>
        /// <returns>Combined string</returns>
        public static string JoinStrings<T>(this IEnumerable<T> values, Func<T, string> format)
        {
            return string.Join("", values.Select(format));
        }

        /// <summary>
        /// Join a sequence of strings using a separator
        /// </summary>
        /// <param name="strings">Source strings</param>
        /// <param name="sep">Separator string</param>
        /// <returns>Combined string</returns>
        public static string JoinStrings(this IEnumerable<string> strings, string sep = "")
        {
            return string.Join(sep, strings);
        }

        /// <summary>
        /// Ensures text ends with platform-specific NewLine symbol
        /// </summary>
        /// <param name="source">Source text</param>
        /// <returns>Text ending with NewLine</returns>
        public static string FixEndLine(string source)
        {
            return source.TrimEnd('\r', '\n', '\t', ' ') + NewLine;
        }

        /// <summary>
        /// Remove double empty lines. Ensure that that there are no adjacent empty lines.
        /// Lines are empty if they contain nothing but whitespaces.
        /// </summary>
        /// <param name="source">Source text</param>
        /// <returns>Text with double empty lines removed</returns>
        public static string RemoveDoubleEmptyLines(string source)
        {
            var sb = new StringBuilder();
            bool prevEmpty = true;
            foreach (var line in Lines(source))
            {
                var isEmpty = line.Trim() == string.Empty;
                if (!(isEmpty && prevEmpty))
                    sb.AppendLine(line);

                prevEmpty = isEmpty;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Conditionally remove empty lines.
        /// Empty lines are removed if the previous line satisfies <paramref name="removeBefore"/> predicate
        /// or the next line satisfies <paramref name="removeAfter"/> predicate.
        /// </summary>
        /// <param name="source">Source text</param>
        /// <param name="removeAfter">Predicate that prescripts to remove the next line</param>
        /// <param name="removeBefore">Predicate that prescripts to remove the previous line</param>
        /// <returns>Text with empty lines removed</returns>
        public static string RemoveEmptyLines(string source, Predicate<string> removeAfter, Predicate<string> removeBefore)
        {
            var sb = new StringBuilder();
            var lines = Lines(source);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                bool appendLine = true;
                if (i != 0 && i != lines.Length - 1 && line.Trim() == "")
                {
                    appendLine = !removeAfter(lines[i - 1]) && !removeBefore(lines[i + 1]);
                }

                if (appendLine)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        public static string Tabulize(string source, Predicate<string> indentCond, Predicate<string> outdentCond, string tab)
        {
            var indentStrings = new List<string>();
            string IndentString(int i)
            {
                if (i >= indentStrings.Count)
                {
                    for (var j = indentStrings.Count; j <= i; j++)
                        indentStrings.Add(string.Concat(Enumerable.Repeat(tab, j)));
                }
                return indentStrings[i];
            }

            var sb = new StringBuilder();
            int indent = 0;
            bool first = true;

            foreach (var line in Lines(source))
            {
                if (!first)
                    sb.AppendLine();
                first = false;

                var trimmed = line.Trim();

                if (outdentCond(trimmed) && indent > 0)
                    indent--;

                if (trimmed != string.Empty)
                    sb.Append(IndentString(indent)).Append(trimmed);

                if (indentCond(trimmed))
                    indent++;
            }

            return sb.ToString();
        }

        public static string Reindent(this string source, string oldIndent, string newIndent)
        {
            var sb = new StringBuilder();
            foreach (var line in Lines(source))
            {
                var i = 0;
                while (line.Length >= oldIndent.Length * (i + 1) && line.Substring(i, oldIndent.Length) == oldIndent)
                {
                    i += oldIndent.Length;
                    sb.Append(newIndent);
                }
                sb.AppendLine(line.Substring(i));
            }
            return sb.ToString();
        }
    }
}
