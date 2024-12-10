using System;
using System.Text;

namespace Igor.Compiler
{
    /// <summary>
    /// Compiler output implementation for system console
    /// </summary>
    public class ConsoleCompilerOutput : CompilerOutput
    {
        public override void Log(string text) => Console.Error.WriteLine(text);

        protected override void WriteMessage(CompilerMessageType type, Location location, string text, string code)
        {
            var oldColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = GetMessageColor(type);
                var message = FormatMessage(type, location, text, code);
                Console.Error.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        private string FormatMessage(CompilerMessageType type, Location location, string text, string code)
        {
            var sb = new StringBuilder();
            if (location.FileName != null)
            {
                sb.Append(location);
                sb.Append(": ");
            }

            sb.Append(GetMessagePrefix(type));
            sb.Append(" ");
            sb.Append(code);
            sb.Append(": ");
            sb.Append(text);
            return sb.ToString();
        }

        private static ConsoleColor GetMessageColor(CompilerMessageType messageType)
        {
            switch (messageType)
            {
                case CompilerMessageType.Error:
                    return ConsoleColor.Red;
                case CompilerMessageType.Warning:
                    return ConsoleColor.Yellow;
                case CompilerMessageType.Hint:
                    return ConsoleColor.Gray;
                default:
                    throw new ArgumentException($"Unknown message type {messageType}", nameof(messageType));
            }
        }

        private static string GetMessagePrefix(CompilerMessageType messageType)
        {
            switch (messageType)
            {
                case CompilerMessageType.Error:
                    return "error";
                case CompilerMessageType.Warning: return "warning";
                case CompilerMessageType.Hint: return "hint";
                default:
                    throw new ArgumentException($"Unknown message type {messageType}", nameof(messageType));
            }
        }
    }
}
