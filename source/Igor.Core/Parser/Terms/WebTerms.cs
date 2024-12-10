using System.Text;

namespace Igor.Parser
{
    public class UriPart : Term
    {
        private readonly string name;
        private readonly char startChar;
        private readonly SetOfChars allowedChars = new SetOfChars(char.IsLetterOrDigit) { '-', '.', '_', '!', '$', '\'', '(', ')', '*', '+', ',', ';', ':', '@', '[', ']' };

        public UriPart(string name, char startChar, SetOfChars allowedChars = null)
        {
            this.name = name;
            this.startChar = startChar;
            if (allowedChars != null)
                this.allowedChars = allowedChars;
        }

        public override bool Test(IgorScanner scanner)
        {
            return scanner.TestChar(startChar);
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            if (scanner.TestChar(startChar))
            {
                var position = scanner.CurrentPosition;
                scanner.CurrentPosition++;
                var sb = new StringBuilder();

                while (scanner.TestChar(allowedChars))
                {
                    var c = scanner.Source[scanner.CurrentPosition];
                    sb.Append(c);
                    scanner.CurrentPosition++;
                }

                token = new Token(position, sb.ToString(), this);
                return true;
            }

            token = Token.None;
            return false;
        }

        public override string ToString() => name;

        public static readonly UriPart PathSegment = new UriPart("URI path segment", '/');
        public static readonly UriPart PathQuery = new UriPart("URI query", '?');
        public static readonly UriPart PathQueryPart = new UriPart("URI query", '&');
        public static readonly UriPart PathQueryValue = new UriPart("URI query value", '=');
        public static readonly UriPart WebHeaderName = new UriPart("HTTP header", '~', new SetOfChars(char.IsLetterOrDigit) { '-', '.', '_', '!', '$', '\'', '(', ')', '*', '+', ',', ';', '@', '[', ']' });
    }

    public class ReasonPhraseTerm : Term
    {
        private readonly SetOfChars terminators = new SetOfChars() { ':', ',', ';' };

        public override bool Test(IgorScanner scanner)
        {
            return !scanner.IsEof() && !scanner.TestChar(terminators);
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            if (scanner.IsEof() || scanner.TestChar(terminators))
            {
                token = Token.None;
                return false;
            }

            var position = scanner.CurrentPosition;
            while (!scanner.IsEof() && !scanner.TestChar(terminators))
                scanner.CurrentPosition++;
            token = new Token(position, scanner.Source.Substring(position, scanner.CurrentPosition - position), this);
            return true;
        }

        public override string ToString() => "Reason phrase";

        public static readonly ReasonPhraseTerm Default = new ReasonPhraseTerm();
    }
}
