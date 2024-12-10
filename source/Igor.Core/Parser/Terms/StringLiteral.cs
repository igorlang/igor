using Igor.Compiler;
using System.Text;

namespace Igor.Parser
{
    public class StringLiteralTerm : Term
    {
        public override bool Test(IgorScanner scanner)
        {
            return scanner.TestChar('"');
        }

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            void ReportUnterminatedString()
            {
                scanner.Output.Error(scanner.GetCurrentLocation(), "Unterminated quoted string", ProblemCode.UnterminatedString);
            }

            if (scanner.TestChar('"'))
            {
                var position = scanner.CurrentPosition;
                scanner.CurrentPosition++;
                var sb = new StringBuilder();

                while (!scanner.IsEof())
                {
                    var c = scanner.Source[scanner.CurrentPosition];
                    switch (c)
                    {
                        case '\\':
                            scanner.CurrentPosition++;
                            if (scanner.IsEof())
                            {
                                ReportUnterminatedString();
                                token = new Token(position, sb.ToString(), this);
                                return true;
                            }
                            else
                            {
                                var c1 = scanner.Source[scanner.CurrentPosition];
                                scanner.CurrentPosition++;
                                switch (c1)
                                {
                                    case '"':
                                        sb.Append('"');
                                        break;
                                    case '\'':
                                        sb.Append('\'');
                                        break;
                                    case '\\':
                                        sb.Append('\\');
                                        break;
                                    case '0':
                                        sb.Append('\0');
                                        break;
                                    case 'a':
                                        sb.Append('\a');
                                        break;
                                    case 'b':
                                        sb.Append('\b');
                                        break;
                                    case 'f':
                                        sb.Append('\f');
                                        break;
                                    case 'n':
                                        sb.Append('\n');
                                        break;
                                    case 'r':
                                        sb.Append('\r');
                                        break;
                                    case 't':
                                        sb.Append('\t');
                                        break;
                                    case 'v':
                                        sb.Append('\v');
                                        break;
                                    case '\n':
                                        ReportUnterminatedString();
                                        token = new Token(position, sb.ToString(), this);
                                        return true;
                                    default:
                                        scanner.Output.Warning(scanner.GetLocation(scanner.CurrentPosition - 1), "Unrecognized escape sequence", ProblemCode.UnrecognizedEscapeSequence);
                                        sb.Append(c);
                                        break;
                                }
                            }
                            break;

                        case '\n':
                            ReportUnterminatedString();
                            scanner.CurrentPosition++;
                            token = new Token(position, sb.ToString(), this);
                            return true;

                        case '"':
                            token = new Token(position, sb.ToString(), this);
                            scanner.CurrentPosition++;
                            return true;

                        default:
                            sb.Append(c);
                            scanner.CurrentPosition++;
                            break;
                    }
                }
                ReportUnterminatedString();
                token = new Token(position, sb.ToString(), this);
                return true;
            }

            token = Token.None;
            return false;
        }

        public static readonly StringLiteralTerm Default = new StringLiteralTerm();
    }
}
