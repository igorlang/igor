using System;
using System.Collections.Generic;

namespace Igor.Compiler
{
    public class CompilerMessage : IEquatable<CompilerMessage>
    {
        public CompilerMessageType Type { get; }
        public Location Location { get; }
        public string Text { get; }
        public string Code { get; }

        public CompilerMessage(CompilerMessageType type, Location location, string text, string code)
        {
            Type = type;
            Location = location;
            Text = text;
            Code = code;
        }

        public bool Equals(CompilerMessage other)
        {
            if (other == null)
                return false;
            return Type == other.Type && Location == other.Location && Text == other.Text && Code == other.Code;
        }

        public override bool Equals(object obj) => Equals(obj as CompilerMessage);

        public override int GetHashCode() => Text.GetHashCode();
    }

    public class TestCompilerOutput : CompilerOutput
    {
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();

        public IReadOnlyList<CompilerMessage> Messages => messages;

        public override void Log(string text)
        {
        }

        protected override void WriteMessage(CompilerMessageType type, Location location, string text, string code)
        {
            var message = new CompilerMessage(type, location, text, code);
            messages.Add(message);
        }
    }
}
