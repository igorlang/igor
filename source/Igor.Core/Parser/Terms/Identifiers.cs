using Igor.Compiler;
using System.Text.RegularExpressions;

namespace Igor.Parser
{
    public class IdentifierTerm : Term
    {
        public bool AllowInvalid { get; }
        public bool ReportInvalid { get; }
        public static readonly Regex IdentifierRegex = new Regex(@"^[a-zA-Z_]{1}[a-zA-Z0-9_]*$");

        public IdentifierTerm(bool allowInvalid, bool reportInvalid)
        {
            AllowInvalid = allowInvalid;
            ReportInvalid = reportInvalid;
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            var wordPosition = scanner.CurrentPosition;
            var nextPosition = wordPosition;
            while (nextPosition < scanner.Source.Length && IsValidChar(scanner.Source[nextPosition]))
                nextPosition++;
            if (nextPosition > wordPosition)
            {
                var text = scanner.Source.Substring(wordPosition, nextPosition - wordPosition);
                bool isValid = IsValidIdentifier(text);
                if (isValid || AllowInvalid)
                {
                    scanner.CurrentPosition = nextPosition;
                    token = new Token(wordPosition, text, this, isValid);
                    if (!isValid)
                        ReportInvalidIdentifier(scanner, token);
                    return true;
                }
            }

            token = Token.None;
            return false;
        }

        public override string ToString() => "identifier";

        public virtual bool IsValidChar(char character) => char.IsLetterOrDigit(character) || character == '_';

        public virtual void ReportInvalidIdentifier(IgorScanner scanner, in Token token)
        {
            var location = scanner.GetLocation(token.Position);
            scanner.Output.Error(location, $"'{token.Text}' is not a valid identifier", ProblemCode.InvalidIdentifier);
        }

        public virtual bool IsValidIdentifier(string text) => IdentifierRegex.IsMatch(text);

        public static readonly IdentifierTerm MaybeInvalidIdentifier = new IdentifierTerm(true, true);
        public static readonly IdentifierTerm SkipInvalidIdentifier = new IdentifierTerm(true, false);
    }

    public class AttributeTargetTerm : IdentifierTerm
    {
        public AttributeTargetTerm() : base(true, true)
        {
        }

        public override string ToString() => "attribute-target";

        public override void ReportInvalidIdentifier(IgorScanner scanner, in Token token)
        {
            var location = scanner.GetLocation(token.Position);
            scanner.Output.Error(location, $"'{token.Text}' is not a valid attribute target", ProblemCode.SyntaxError);
        }

        public static readonly AttributeTargetTerm Default = new AttributeTargetTerm();
    }

    public class AttributeNameTerm : IdentifierTerm
    {
        public static readonly Regex AttributeNameRegex = new Regex(@"^[a-zA-Z]{1}[a-zA-Z0-9_.]*$");

        public AttributeNameTerm() : base(true, true)
        {
        }

        public override string ToString() => "attribute-name";

        public override void ReportInvalidIdentifier(IgorScanner scanner, in Token token)
        {
            var location = scanner.GetLocation(token.Position);
            scanner.Output.Error(location, $"'{token.Text}' is not a valid attribute name", ProblemCode.SyntaxError);
        }

        public override bool IsValidChar(char character) => char.IsLetterOrDigit(character) || character == '_' || character == '.';

        public override bool IsValidIdentifier(string text) => AttributeNameRegex.IsMatch(text);

        public static readonly AttributeNameTerm Default = new AttributeNameTerm();
    }
}
