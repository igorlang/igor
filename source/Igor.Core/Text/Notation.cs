using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Igor.Text
{
    public enum Notation
    {
        /// <summary>
        /// No translation, keep identifier as is
        /// </summary>
        [IgorEnumValue("none")]
        None,

        /// <summary>
        /// Lowercase all letters, e.g. 'getobjectid'
        /// </summary>
        [IgorEnumValue("lower")]
        Lower,

        /// <summary>
        /// Uppercase all letters, e.g. 'GETOBJECTID'
        /// </summary>
        [IgorEnumValue("upper")]
        Upper,

        /// <summary>
        /// Camel case notation, starts with the lower case, e.g. 'getObjectId' or 'getObjectID' depending on initialism treatment
        /// </summary>
        [IgorEnumValue("lower_camel")]
        LowerCamel,

        /// <summary>
        /// Camel case notation, starts with the upper case (Pascal notation), e.g. 'GetObjectId' or 'GetObjectID' depending on initialism treatment
        /// </summary>
        [IgorEnumValue("upper_camel")]
        UpperCamel,

        /// <summary>
        /// Lowercase all letters, split words with '_', e.g. 'get_object_id'
        /// </summary>
        [IgorEnumValue("lower_underscore")]
        LowerUnderscore,

        /// <summary>
        /// Uppercase all letters, split words with '_', e.g. 'GET_OBJECT_ID'
        /// </summary>
        [IgorEnumValue("upper_underscore")]
        UpperUnderscore,

        /// <summary>
        /// Lowercase all letters, split words with '-', e.g. 'get-object-id'
        /// </summary>
        [IgorEnumValue("lower_hyphen")]
        LowerHyphen,

        /// <summary>
        /// Uppercase all letters, split words with '-', e.g. 'GET-OBJECT-ID'
        /// </summary>
        [IgorEnumValue("upper_hyphen")]
        UpperHyphen,

        /// <summary>
        /// Space separated capitalized words, e.g. 'Get Object Id'
        /// </summary>
        [IgorEnumValue("Title")]
        Title,

        /// <summary>
        /// Lowercase first letters of each word, e.g. "goi"
        /// </summary>
        [IgorEnumValue("lower_initialism")]
        LowerInitialism,

        /// <summary>
        /// Lowercase first letter of the last word, e.g "i"
        /// </summary>
        [IgorEnumValue("first_letter_last_word")]
        FirstLetterLastWord,
    }

    /// <summary>
    /// Static routines and extension methods for Notation name translation.
    /// </summary>
    /// <seealso cref="Notation"/>
    public static class NotationHelper
    {
        private static readonly HashSet<string> CommonInitialisms = new HashSet<string> {
            "acl", "api", "ascii", "cpu", "css", "db", "dns", "eof", "guid", "html", "http", "https", "id",
            "ip", "json", "lhs", "qps", "ram", "rhs", "rpc","sla", "smtp", "sql", "ssh", "tcp", "tls",
            "ttl", "udp", "ui", "uid", "uuid", "uri", "url", "utf8", "vm", "xml","xmpp", "xsrf", "xss"
        };

        private enum Case
        {
            None,
            Upper,
            Lower,
            Any,
        };

        /// <summary>
        /// Is the word a known initialism?
        /// </summary>
        /// <param name="word">Word</param>
        /// <returns>True if the word is an initialism, false otherwise</returns>
        public static bool IsInitialism(this string word) => CommonInitialisms.Contains(word);

        public static void RegisterInitialisms(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                CommonInitialisms.Add(word);
            }
        }

        private static string ToUpperCamel(string[] words, bool upperInitialisms)
        {
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if (upperInitialisms && word.IsInitialism())
                    sb.Append(CultureInfo.InvariantCulture.TextInfo.ToUpper(word));
                else
                    sb.Append(Capitalize(word));
            }
            return sb.ToString();
        }

        private static string ToLowerCamel(string[] words, bool upperInitialisms)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (i == 0)
                    sb.Append(word);
                else if (upperInitialisms && word.IsInitialism())
                    sb.Append(CultureInfo.InvariantCulture.TextInfo.ToUpper(word));
                else
                    sb.Append(Capitalize(words[i]));
            }
            return sb.ToString();
        }

        private static string ToTitle(string[] words, bool upperInitialisms)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (i > 0)
                    sb.Append(' ');
                if (upperInitialisms && word.IsInitialism())
                    sb.Append(CultureInfo.InvariantCulture.TextInfo.ToUpper(word));
                else
                    sb.Append(Capitalize(word));
            }
            return sb.ToString();
        }

        private static string ToUpperUnderscore(string[] words)
        {
            return words.JoinStrings("_", CultureInfo.InvariantCulture.TextInfo.ToUpper);
        }

        private static string ToLowerUnderscore(string[] words)
        {
            return words.JoinStrings("_");
        }

        private static string ToUpperHyphen(string[] words)
        {
            return words.JoinStrings("-", CultureInfo.InvariantCulture.TextInfo.ToUpper);
        }

        private static string ToLowerHyphen(string[] words)
        {
            return words.JoinStrings("-");
        }

        private static string[] Split(string str)
        {
            return str.Split('_').SelectMany(SplitCase).ToArray();
        }

        private static string ToFirstLetterLastWord(string str)
        {
            if (str == "") {
                return "";
            }
            var words = SplitCase(str);
            return words[words.Count-1].Substring(0, 1);
        }

        private static List<string> SplitCase(string str)
        {
            List<string> result = new List<string>();
            string word = "";
            Case last = Case.None;
            foreach (var s in Reverse(str))
            {
                var current = GetCase(s);
                var chs = s.ToString();
                switch (last)
                {
                    case var _ when last == current:
                        word += chs;
                        break;
                    case Case.Any:
                        last = current;
                        word += chs;
                        break;
                    case Case.None:
                        word += chs;
                        last = current;
                        break;
                    case Case.Upper:
                        result.Add(Reverse(word).ToLowerInvariant());
                        word = chs;
                        last = Case.Lower;
                        break;
                    case Case.Lower:
                        result.Add(Reverse(word + chs).ToLowerInvariant());
                        word = "";
                        last = Case.None;
                        break;
                }
            }
            if (word != "")
                result.Add(Reverse(word).ToLowerInvariant());
            result.Reverse();
            return result;
        }

        private static string Reverse(string str)
        {
            var arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        private static Case GetCase(char ch)
        {
            if (char.ToUpperInvariant(ch) == char.ToLowerInvariant(ch))
                return Case.Any;
            else if (char.IsLower(ch))
                return Case.Lower;
            else
                return Case.Upper;
        }

        private static string Capitalize(string str)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);
        }

        private static string DontStartWithDigit(this string str)
        {
            if (str.Length > 0 && char.IsDigit(str[0]))
                return "_" + str;
            else
                return str;
        }

        /// <summary>
        /// Extension method to translate identifier to a given Notation
        /// </summary>
        /// <example>
        /// <code>
        /// "get_object_id".Format(Notation.LowerCamel, true)
        /// </code>
        /// returns "getObjectID"
        /// </example>
        /// <param name="str">Source identifier</param>
        /// <param name="notation">Notation used to translate identifier</param>
        /// <param name="upperInitialisms">Whether to uppercase initialisms in camel notations</param>
        /// <returns>Translated identifier</returns>
        /// <seealso cref="Notation"/>
        public static string Format(this string str, Notation notation, bool upperInitialisms = false)
        {
            switch (notation)
            {
                case Notation.Lower: return str.ToLowerInvariant().DontStartWithDigit();
                case Notation.Upper: return str.ToUpperInvariant().DontStartWithDigit();
                case Notation.LowerCamel: return ToLowerCamel(Split(str), upperInitialisms).DontStartWithDigit();
                case Notation.UpperCamel: return ToUpperCamel(Split(str), upperInitialisms).DontStartWithDigit();
                case Notation.LowerUnderscore: return ToLowerUnderscore(Split(str)).DontStartWithDigit();
                case Notation.UpperUnderscore: return ToUpperUnderscore(Split(str)).DontStartWithDigit();
                case Notation.LowerHyphen: return ToLowerHyphen(Split(str)).DontStartWithDigit();
                case Notation.UpperHyphen: return ToUpperHyphen(Split(str)).DontStartWithDigit();
                case Notation.Title: return ToTitle(Split(str), upperInitialisms).DontStartWithDigit();
                case Notation.LowerInitialism: return Split(str).JoinStrings(s => s.Substring(0, 1)).DontStartWithDigit();
                case Notation.FirstLetterLastWord: return ToFirstLetterLastWord(str).DontStartWithDigit();
                default: return str.DontStartWithDigit();
            }
        }
    }
}
