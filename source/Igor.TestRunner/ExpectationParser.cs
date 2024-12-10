using ParsecSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Igor.Text;
using static ParsecSharp.Parser;
using static ParsecSharp.Text;

namespace Igor.TestRunner
{
    public class ExpectationParserException : Exception
    {
        public string ExpectationString { get; }
        public int Line { get; }

        public ExpectationParserException(int line, string expectationString)
            : base($"Failed to parse expectation: {expectationString}")
        {
            ExpectationString = expectationString;
            Line = line;
        }
    }

    public class ExpectationParser
    {
        private Parser<char, char> UnescapedChar()
            => Any().Except(Char('"'), Char('\\'), Satisfy(x => x <= 0x1F));

        private Parser<char, char> EscapedChar()
            => Char('\\').Right(
                Choice(
                    Char('"'),
                    Char('\\'),
                    Char('/'),
                    Char('b').Map(_ => '\b'),
                    Char('f').Map(_ => '\f'),
                    Char('n').Map(_ => '\n'),
                    Char('r').Map(_ => '\r'),
                    Char('t').Map(_ => '\t'),
                    Char('u').Right(HexDigit().Repeat(4).AsString())
                        .Map(hex => (char)int.Parse(hex, NumberStyles.HexNumber))));

        private Parser<char, string> Identifier() => (Letter().Or(Char('_'))).Append(Many(LetterOrDigit().Or(Char('_')))).AsString();

        private Parser<char, string> QuotedString()
            => Many(UnescapedChar() | EscapedChar()).Between(Char('"')).AsString();

        private Parser<char, Expression> StringExpression()
            => QuotedString().Map(s => (Expression)new StringExpression(s));

        private Parser<char, Expression> BoolExpression()
            => (String("false").Map(_ => false) | String("true").Map(_ => true)).Map(b => (Expression)new BoolExpression(b));

        private Parser<char, int> Sign() => Optional(Char('-').Map(_ => -1), 1);

        private Parser<char, int> Int() => Char('0').Map(_ => 0) | OneOf("123456789").Append(Many(DecDigit())).ToInt();

        private Parser<char, double> Frac()
            => Char('.').Right(Many1(DecDigit())).AsString().Map(x => double.Parse("0." + x, CultureInfo.InvariantCulture));

        private Parser<char, Expression> FloatExpression()
            => from sign in Sign()
               from integer in Int()
               from frac in Frac()
               select (Expression)new FloatExpression(sign * (integer + frac));

        private Parser<char, Expression> IntegerExpression()
             => from sign in Sign()
                from integer in Int()
                select (Expression)new IntegerExpression(sign * integer);

        private Parser<char, Expression> NameExpression()
            => Char('@').Right(Identifier()).Map(v => (Expression)new NameExpression(lineIndex, v));

        private Parser<char, Accessor> PropertyAccessor() =>
            from _1 in Char('.')
            from prop in Identifier()
            select (Accessor)new PropertyAccessor(prop);

        private Parser<char, Accessor> IndexAccessor() =>
            from _1 in Char('[')
            from i in Int()
            from _2 in Char(']')
            select (Accessor)new IndexAccessor(i);

        private Parser<char, Accessor> Accessor() => PropertyAccessor() | IndexAccessor();

        private Parser<char, Expression> AccessExpression() =>
            from name in NameExpression()
            from accessors in Many(Accessor())
            select accessors.Aggregate(name, (acc, accessor) => new AccessExpression(acc, accessor));

        private Parser<char, Expression> Expression() => BoolExpression() | FloatExpression() | IntegerExpression() | StringExpression() | AccessExpression() | NameExpression();

        private Parser<char, Expectation> ErrorExpectation() =>
            from _1 in String("Expected")
            from _2 in WhiteSpace().Ignore()
            from _3 in String("error")
            from _4 in WhiteSpace().Ignore()
            from error in QuotedString()
            select (Expectation)new ErrorExpectation(lineIndex, error);

        private Parser<char, Expectation> MatchExpectation() =>
            from _1 in String("Expect")
            from _2 in WhiteSpace().Ignore()
            from _3 in String("that")
            from _4 in WhiteSpace().Ignore()
            from left in Expression()
            from _6 in WhiteSpace().Ignore()
            from _7 in Char('=')
            from _8 in WhiteSpace().Ignore()
            from right in Expression()
            select (Expectation)new MatchExpectation(left, right);

        private Parser<char, Expectation> Expectation()
            => ErrorExpectation() | MatchExpectation();

        public Result<char, Expectation> ParseExpectation(string source, int lineIndex)
        {
            this.lineIndex = lineIndex;
            return Expectation().End().Parse(source);
        }

        private int lineIndex;

        public List<Expectation> Parse(string source)
        {
            var result = new List<Expectation>();
            var lines = TextHelper.Lines(source);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.Contains('$'))
                    continue;
                foreach (var exp in line.Split('$').Skip(1).Select(s => s.Trim()))
                {
                    ParseExpectation(exp, i + 1).CaseOf(
                        fail => throw new ExpectationParserException(lineIndex, exp),
                        success => result.Add(success.Value));
                }
            }
            return result;
        }
    }
}
