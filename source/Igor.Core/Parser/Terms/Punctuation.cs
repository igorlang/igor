using Igor.Text;

namespace Igor.Parser
{
    public class PunctuationTerm : Term
    {
        public string Value { get; }

        public override bool Test(IgorScanner scanner)
        {
            return scanner.TestString(Value);
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            var tokenPosition = scanner.CurrentPosition;
            if (scanner.TestString(Value))
            {
                scanner.CurrentPosition = scanner.CurrentPosition + Value.Length;
                token = new Token(tokenPosition, Value, this);
                return true;
            }
            token = Token.None;
            return false;
        }

        public PunctuationTerm(string value)
        {
            Value = value;
        }

        public override string ToString() => Value.Quoted("'");
    }

    public static class Punctuation
    {
        public static readonly PunctuationTerm LeftBracket = new PunctuationTerm("[");
        public static readonly PunctuationTerm RightBracket = new PunctuationTerm("]");
        public static readonly PunctuationTerm LeftParen = new PunctuationTerm("(");
        public static readonly PunctuationTerm RightParen = new PunctuationTerm(")");
        public static readonly PunctuationTerm LeftBrace = new PunctuationTerm("{");
        public static readonly PunctuationTerm RightBrace = new PunctuationTerm("}");
        public static readonly PunctuationTerm Less = new PunctuationTerm("<");
        public static readonly PunctuationTerm Greater = new PunctuationTerm(">");
        public static readonly PunctuationTerm Semi = new PunctuationTerm(";");
        public static readonly PunctuationTerm Comma = new PunctuationTerm(",");
        public static readonly PunctuationTerm Colon = new PunctuationTerm(":");
        public static readonly PunctuationTerm Assign = new PunctuationTerm("=");
        public static readonly PunctuationTerm Asterisk = new PunctuationTerm("*");
        public static readonly PunctuationTerm Dot = new PunctuationTerm(".");
        public static readonly PunctuationTerm Question = new PunctuationTerm("?");
        public static readonly PunctuationTerm Arrow = new PunctuationTerm("=>");
        public static readonly PunctuationTerm To = new PunctuationTerm("->");
        public static readonly PunctuationTerm Slash = new PunctuationTerm("/");
        public static readonly PunctuationTerm And = new PunctuationTerm("&");
        public static readonly PunctuationTerm Or = new PunctuationTerm("|");
    }
}
