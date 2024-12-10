using Igor.Compiler;
using Igor.Text;
using System;

namespace Igor.Parser
{
    public class ParserException : Exception
    {
        public Location Location { get; }
        public ProblemCode ProblemCode { get; }

        public ParserException(Location location, string message, ProblemCode problemCode) : base(message)
        {
            Location = location;
            ProblemCode = problemCode;
        }
    }

    public class ParserNonRecoverableException : Exception
    {
        public ParserNonRecoverableException() : base("Parser failed to recover") { }
    }

    public readonly struct Token
    {
        public int Position { get; }
        public string Text { get; }
        public Term Term { get; }
        public bool IsValid { get; }
        public bool IsNone => Term == null;

        public Token(int position, string text, Term term, bool isValid = true)
        {
            Position = position;
            Text = text;
            Term = term;
            IsValid = isValid;
        }

        public static readonly Token None = new Token(-1, null, null, false);
    }

    public class IgorScanner
    {
        public string Source { get; }
        private readonly string filename;
        public CompilerOutput Output { get; }
        private readonly LineIndex lineIndex;

        public int CurrentPosition { get; set; }

        public bool AutoSkipWhitespaceAndComments { get; set; }

        public IgorScanner(string source, string filename, CompilerOutput output)
        {
            this.Source = source;
            this.filename = filename;
            this.Output = output;
            this.lineIndex = new LineIndex(source);
        }

        public Location GetCurrentLocation()
        {
            return GetLocation(CurrentPosition);
        }

        public Location GetLocation(int position)
        {
            lineIndex.GetLineAndColumn(position, out var line, out var column);
            return new Location(filename, line, column);
        }

        public Location GetLocation(Token token) => GetLocation(token.Position);

        public Location GetEofLocation()
        {
            return GetLocation(Source.Length);
        }

        public bool Test(Term term)
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();
            return term.Test(this);
        }

        public bool TestAny(params Term[] terms)
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();

            foreach (var term in terms)
            {
                if (term.Test(this))
                    return true;
            }

            return false;
        }

        public bool TryMatch(Term term)
        {
            return TryMatch(term, out var _);
        }

        public bool TryMatch(Term term, out Token token)
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();

            return term.TryMatch(this, out token);
        }

        public Token TryMatchAny(params Term[] terms)
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();

            foreach (var term in terms)
            {
                if (term.TryMatch(this, out var token))
                    return token;
            }
            return Token.None;
        }

        public Token Match(Term term, string expected = null)
        {
            if (TryMatch(term, out var token))
                return token;
            var location = GetCurrentLocation();
            expected = expected ?? term.ExpectedName;
            var message = IsEof() ? $"Unexpected end of file. Expected: {expected}" : $"Expected: {expected}";
            throw new ParserException(location, message, ProblemCode.SyntaxError);
        }

        public Token MatchAny(params Term[] terms)
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();

            foreach (var term in terms)
            {
                if (term.TryMatch(this, out var token))
                    return token;
            }

            throw new ParserException(GetCurrentLocation(), $"Expected: {terms.JoinStrings(" or ", term => term.ToString())}", ProblemCode.SyntaxError);
        }

        public bool SkipWhitespaceAndComments()
        {
            bool result = false;
            while (SkipWhitespace() || SkipLineComment() || SkipBlockComment())
                result = true;
            return result;
        }

        public bool SkipWhitespace()
        {
            bool result = false;
            while (CurrentPosition < Source.Length && char.IsWhiteSpace(Source, CurrentPosition))
            {
                CurrentPosition++;
                result = true;
            }
            return result;
        }

        private bool SkipLineComment()
        {
            if (CurrentPosition + 1 < Source.Length && Source[CurrentPosition] == '/' && Source[CurrentPosition + 1] == '/')
            {
                CurrentPosition += 2;
                var newLine = Source.IndexOf('\n', CurrentPosition);
                CurrentPosition = newLine < 0 ? Source.Length : newLine + 1;
                return true;
            }

            return false;
        }

        private bool SkipBlockComment()
        {
            if (CurrentPosition + 1 < Source.Length && Source[CurrentPosition] == '/' && Source[CurrentPosition + 1] == '*')
            {
                CurrentPosition += 2;
                var end = Source.IndexOf("*/", CurrentPosition, StringComparison.Ordinal);
                CurrentPosition = end < 0 ? Source.Length : end + 2;
                return true;
            }

            return false;
        }

        public bool IsEof()
        {
            return CurrentPosition >= Source.Length;
        }

        public bool TestEof()
        {
            if (AutoSkipWhitespaceAndComments)
                SkipWhitespaceAndComments();
            return IsEof();
        }

        public bool TestString(string value)
        {
            if (CurrentPosition + value.Length > Source.Length)
                return false;
            return string.CompareOrdinal(Source, CurrentPosition, value, 0, value.Length) == 0;
        }

        public bool TestChar(char character)
        {
            if (IsEof())
                return false;
            return character == Source[CurrentPosition];
        }

        public bool TestChar(SetOfChars setOfChars)
        {
            if (IsEof())
                return false;
            return setOfChars.Test(Source[CurrentPosition]);
        }

        public void Recover(Term consume, Term preview)
        {
            while (!IsEof())
            {
                if (consume != null && TryMatch(consume))
                    return;
                if (preview != null && Test(preview))
                    return;

                if (!SkipToken())
                    break;
            }

            throw new ParserNonRecoverableException();
        }

        protected void ReportException(ParserException exception)
        {
            Output.Error(exception.Location, exception.Message, exception.ProblemCode);
        }

        protected virtual bool SkipToken()
        {
            CurrentPosition++;
            return true;
        }
    }
}
