using Igor.Compiler;
using Igor.Text;
using System;
using System.Text;

namespace Igor.Parser
{
    public class LineAnnotationTerm : Term
    {
        public override bool Test(IgorScanner scanner)
        {
            return scanner.TestChar('#');
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            if (scanner.TestChar('#'))
            {
                var sb = new StringBuilder();
                var position = scanner.CurrentPosition;
                do
                {
                    scanner.CurrentPosition++;
                    var startPosition = scanner.CurrentPosition;
                    var newLine = scanner.Source.IndexOf('\n', scanner.CurrentPosition);
                    scanner.CurrentPosition = newLine < 0 ? scanner.Source.Length : newLine + 1;
                    if (sb.Length > 0)
                        sb.AppendLine();
                    sb.Append(scanner.Source.Substring(startPosition, scanner.CurrentPosition - 1 - startPosition).Trim());
                    scanner.SkipWhitespaceAndComments();
                } while (scanner.TestChar('#'));

                token = new Token(position, sb.ToString(), this);
                return true;
            }

            token = Token.None;
            return false;
        }

        public static readonly LineAnnotationTerm Default = new LineAnnotationTerm();
    }

    public class BlockAnnotationTerm : Term
    {
        public override bool Test(IgorScanner scanner)
        {
            return scanner.TestString("<#");
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            if (scanner.TestString("<#"))
            {
                var position = scanner.CurrentPosition;
                scanner.CurrentPosition += 2;
                var startPosition = scanner.CurrentPosition;
                var endIndex = scanner.Source.IndexOf("#>", scanner.CurrentPosition, StringComparison.Ordinal);
                string text;
                if (endIndex < 0)
                {
                    scanner.Output.Error(scanner.GetEofLocation(), "Unexpected end of file. Expected '#>'", ProblemCode.SyntaxError);
                    text = scanner.Source.Substring(startPosition);
                    scanner.CurrentPosition = scanner.Source.Length;
                }
                else
                {
                    text = scanner.Source.Substring(startPosition, endIndex - startPosition);
                    scanner.CurrentPosition = endIndex + 2;
                }

                text = TextHelper.Lines(text).JoinStrings(" ", line => line.Trim()).Trim();
                token = new Token(position, text, this);
                return true;
            }

            token = Token.None;
            return false;
        }

        public static readonly BlockAnnotationTerm Default = new BlockAnnotationTerm();
    }
}
