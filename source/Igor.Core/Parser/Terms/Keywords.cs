using Igor.Text;

namespace Igor.Parser
{
    public class KeywordTerm : Term
    {
        public string Name { get; }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            var tokenPosition = scanner.CurrentPosition;
            if (scanner.TestString(Name))
            {
                var nextPosition = scanner.CurrentPosition + Name.Length;
                if (nextPosition == scanner.Source.Length || !char.IsLetterOrDigit(scanner.Source, nextPosition))
                {
                    scanner.CurrentPosition = nextPosition;
                    token = new Token(tokenPosition, Name, this);
                    return true;
                }
            }

            token = Token.None;
            return false;
        }

        public KeywordTerm(string name)
        {
            Name = name;
        }

        public override string ToString() => Name.Quoted("'");
    }

    public static class Keywords
    {
        public static readonly KeywordTerm Using = new KeywordTerm("using");
        public static readonly KeywordTerm Module = new KeywordTerm("module");
        public static readonly KeywordTerm Record = new KeywordTerm("record");
        public static readonly KeywordTerm Exception = new KeywordTerm("exception");
        public static readonly KeywordTerm Variant = new KeywordTerm("variant");
        public static readonly KeywordTerm Interface = new KeywordTerm("interface");
        public static readonly KeywordTerm Enum = new KeywordTerm("enum");
        public static readonly KeywordTerm Define = new KeywordTerm("define");
        public static readonly KeywordTerm Union = new KeywordTerm("union");
        public static readonly KeywordTerm Table = new KeywordTerm("table");
        public static readonly KeywordTerm Service = new KeywordTerm("service");
        public static readonly KeywordTerm Webservice = new KeywordTerm("webservice");
        public static readonly KeywordTerm Tag = new KeywordTerm("tag");
        public static readonly KeywordTerm False = new KeywordTerm("false");
        public static readonly KeywordTerm True = new KeywordTerm("true");
        public static readonly KeywordTerm CToS = new KeywordTerm("c->s");
        public static readonly KeywordTerm SToC = new KeywordTerm("s->c");
        public static readonly KeywordTerm Returns = new KeywordTerm("returns");
        public static readonly KeywordTerm Throws = new KeywordTerm("throws");
        public static readonly KeywordTerm As = new KeywordTerm("as");
    }
}

