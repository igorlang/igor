using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp
{
    public static class CsSyntaxUtils
    {
        public static string IndexInitializer(IDictionary<string, string> pairs, string emptyPrefix = "", int indent = 0, bool quoteKeys = false)
        {
            if (!pairs.Any())
                return emptyPrefix;
            string MaybeQuote(string key) => quoteKeys ? key.Quoted() : key;
            if (Context.Instance.TargetVersion < CsVersion.Version60)
            {
                return Environment.NewLine + $@"{{
{pairs.JoinLines(pair => $@"    {{ {MaybeQuote(pair.Key)}, {pair.Value} }},")}
}}".Indent(indent);
            }
            else
            {
                return Environment.NewLine + $@"{{
{pairs.JoinStrings(",\n", pair => $@"    [{MaybeQuote(pair.Key)}] = {pair.Value}")}
}}".Indent(indent);
            }
        }

        public static string CollectionInitializer(IEnumerable<string> values, string emptyPrefix = "", int indent = 0)
        {
            if (!values.Any())
                return emptyPrefix;
            return Environment.NewLine + $@"{{
{values.JoinStrings(",\n", val => $@"    {val}")}
}}".Indent(indent);
        }

        public static string ReturnSwitch(string value, IEnumerable<(string, string)> cases, string defaultCase)
        {
            var r = new Renderer();
            if (CsVersion.SupportsSwitchExpression)
            {
                r += $"return {value} switch";
                r += "{";
                r++;
                foreach (var c in cases)
                    r += $"{c.Item1} => {c.Item2},";
                r += $"_ => {defaultCase}";
                r--;
                r += "};";
            }
            else
            {
                r += $"switch ({value})";
                r += "{";
                r++;
                foreach (var c in cases)
                    r += $"case {c.Item1}: return {c.Item2};";
                if (defaultCase.StartsWith("throw"))
                    r += $"default: {defaultCase};";
                else
                    r += $"default: return {defaultCase};";
                r--;
                r += "}";
            }
            return r.Build();
        }
    }
}
