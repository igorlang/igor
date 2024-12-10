using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igor.Erlang.Binary
{
    public class ErlIgorReader
    {
        private abstract class StreamValue
        {
            public class Inline : StreamValue
            {
                public string text { get; }

                public Inline(string text)
                {
                    this.text = text;
                }
            }

            public class Call : StreamValue
            {
                public Func<string, string> text { get; }
                public string var { get; }

                public Call(Func<string, string> text, string var)
                {
                    this.text = text;
                    this.var = var;
                }
            }
        }

        private readonly List<StreamValue> values = new List<StreamValue>();

        public bool RequireIgorInclude() => values.OfType<StreamValue.Inline>().Any();

        public void ReadInline(string text)
        {
            values.Add(new StreamValue.Inline(text));
        }

        public void ReadCall(Func<string, string> text, string var)
        {
            values.Add(new StreamValue.Call(text, var));
        }

        public void Bit(string name)
        {
            ReadInline($"Bit_{name}:1");
        }

        public void PadBits(int bits)
        {
            if (bits > 0)
                ReadInline($"0:{bits}");
        }

        public void Var(string name, SerializationTag tag)
        {
            switch (tag)
            {
                case SerializationTags.Primitive primitive:
                    switch (primitive.Type)
                    {
                        case PrimitiveType.SByte:
                            ReadInline($"?sbyte({name})");
                            break;
                        case PrimitiveType.Byte:
                            ReadInline($"?byte({name})");
                            break;
                        case PrimitiveType.Short:
                            ReadInline($"?short({name})");
                            break;
                        case PrimitiveType.UShort:
                            ReadInline($"?ushort({name})");
                            break;
                        case PrimitiveType.Int:
                            ReadInline($"?int({name})");
                            break;
                        case PrimitiveType.UInt:
                            ReadInline($"?uint({name})");
                            break;
                        case PrimitiveType.Long:
                            ReadInline($"?long({name})");
                            break;
                        case PrimitiveType.ULong:
                            ReadInline($"?ulong({name})");
                            break;
                        case PrimitiveType.Float:
                            ReadInline($"?float({name})");
                            break;
                        case PrimitiveType.Double:
                            ReadInline($"?double({name})");
                            break;
                        default:
                            ParseCall(name, tag);
                            break;
                    }
                    break;
                case SerializationTags.Custom custom:
                    ReadCall(b => $"{custom.ParseFun}({b}{custom.Args.JoinStrings(arg => $", {arg.ParseTag}")})", name);
                    break;
                default:
                    ParseCall(name, tag);
                    break;
            }
        }

        public void ParseCall(string name, SerializationTag tag)
        {
            ReadCall(binary => $"igor_binary:parse_value({binary}, {tag.ParseTag})", name);
        }

        public void MaybeParseCall(string name, SerializationTag tag)
        {
            ReadCall(binary => $"igor_binary:maybe_parse_value(Bit_{name}, {binary}, {tag.ParseTag})", name);
        }

        public void Render(Renderer r, string postfix = null)
        {
            r.Block(Render() + postfix);
        }

        public string Render()
        {
            if (values.Count == 0)
                return "Tail = Binary,";
            else if (values.All(v => v is StreamValue.Inline))
            {
                var inlines = values.Cast<StreamValue.Inline>();
                return $"<<{inlines.JoinStrings(", ", i => i.text)}, Tail/binary>> = Binary,";
            }
            else
            {
                var sb = new StringBuilder();
                var groups = values.GroupAdjacentBy((v1, v2) => v1 is StreamValue.Inline && v2 is StreamValue.Inline);
                var count = groups.Count();
                int i = 0;
                foreach (var g in groups)
                {
                    var binary = i == 0 ? "Binary" : $"Binary{i}";
                    var tail = i == count - 1 ? "Tail" : $"Binary{i + 1}";
                    if (g[0] is StreamValue.Inline)
                    {
                        var inlines = g.Cast<StreamValue.Inline>();
                        sb.AppendLine($"<<{inlines.JoinStrings(", ", inl => inl.text)}, {tail}/binary>> = {binary},");
                    }
                    else
                    {
                        var v = (StreamValue.Call)g[0];
                        sb.AppendLine($"{{{v.var}, {tail}}} = {v.text(binary)},");
                    }
                    i++;
                }
                return sb.ToString();
            }
        }

        public static string Render(params (string name, SerializationTag tag)[] vars)
        {
            return Render((IEnumerable<(string name, SerializationTag tag)>)vars);
        }

        public static string Render(IEnumerable<(string name, SerializationTag tag)> vars)
        {
            var stream = new ErlIgorReader();
            foreach (var item in vars)
                stream.Var(item.name, item.tag);
            return stream.Render();
        }
    }
}
