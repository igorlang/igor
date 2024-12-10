using System;
using System.Globalization;

namespace Igor.Parser
{
    [Flags]
    public enum NumberTermFlags
    {
        AllowMinus,
        AllowHex,
    }

    public class NumberTerm : Term
    {
        private readonly NumberTermFlags flags;

        public NumberTerm(NumberTermFlags flags)
        {
            this.flags = flags;
        }

        public override bool Test(IgorScanner scanner)
        {
            if (scanner.IsEof())
                return false;
            if (char.IsDigit(scanner.Source, scanner.CurrentPosition))
                return true;
            if (flags.HasFlag(NumberTermFlags.AllowMinus) && scanner.TestChar('-'))
                return scanner.CurrentPosition + 1 < scanner.Source.Length && char.IsDigit(scanner.Source, scanner.CurrentPosition + 1);
            return false;
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            token = Token.None;
            var wordPosition = scanner.CurrentPosition;
            var nextPosition = wordPosition;
            if (flags.HasFlag(NumberTermFlags.AllowMinus) && scanner.TestChar('-'))
                nextPosition++;
            if (flags.HasFlag(NumberTermFlags.AllowHex) && scanner.TestString("0x"))
                nextPosition += 2;
            if (nextPosition >= scanner.Source.Length || !char.IsDigit(scanner.Source, nextPosition))
                return false;
            while (nextPosition < scanner.Source.Length && char.IsLetterOrDigit(scanner.Source, nextPosition) || scanner.Source[nextPosition] == '.')
                nextPosition++;
            if (nextPosition > wordPosition)
            {
                var text = scanner.Source.Substring(wordPosition, nextPosition - wordPosition);
                scanner.CurrentPosition = nextPosition;
                token = new Token(wordPosition, text, this);
                return true;
            }

            return false;
        }

        public static bool TryParseIntegral(Token token, out long value, out NumberBase numberBase)
        {
            numberBase = NumberBase.Decimal;
            if (token.Text.StartsWith("0x") && long.TryParse(token.Text.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                numberBase = NumberBase.Hex;
                return true;
            }
            else if (long.TryParse(token.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;
            else
                return false;
        }

        public static readonly NumberTerm Default = new NumberTerm(NumberTermFlags.AllowMinus | NumberTermFlags.AllowHex);
    }
}
