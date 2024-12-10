using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Binary
{
    public class ErlIgorWriter
    {
        private enum Kind
        {
            IoData,
            Binary,
        }

        private class StreamValue
        {
            public Kind kind { get; }
            public string text { get; }

            public StreamValue(Kind kind, string text)
            {
                this.kind = kind;
                this.text = text;
            }
        }

        private readonly List<StreamValue> values = new List<StreamValue>();

        public bool RequireIgorInclude()
        {
            return values.Any(v => v.kind == Kind.Binary);
        }

        public void WriteBinary(string text)
        {
            values.Add(new StreamValue(Kind.Binary, text));
        }

        public void WriteIoData(string text)
        {
            values.Add(new StreamValue(Kind.IoData, text));
        }

        public void Bit(string name)
        {
            WriteBinary($"?bit({name})");
        }

        public void PadBits(int bits)
        {
            if (bits > 0)
                WriteBinary($"0:{bits}");
        }

        public void Bool(string name)
        {
            WriteIoData($"if {name} =:= true -> 1; true -> 0 end");
        }

        public void Var(string name, SerializationTag tag)
        {
            switch (tag)
            {
                case SerializationTags.Primitive primitive:
                    switch (primitive.Type)
                    {
                        case PrimitiveType.Bool: Bool(name); break;
                        case PrimitiveType.SByte: WriteBinary($"?sbyte({name})"); break;
                        case PrimitiveType.Byte: WriteBinary($"?byte({name})"); break;
                        case PrimitiveType.Short: WriteBinary($"?short({name})"); break;
                        case PrimitiveType.UShort: WriteBinary($"?ushort({name})"); break;
                        case PrimitiveType.Int: WriteBinary($"?int({name})"); break;
                        case PrimitiveType.UInt: WriteBinary($"?uint({name})"); break;
                        case PrimitiveType.Long: WriteBinary($"?long({name})"); break;
                        case PrimitiveType.ULong: WriteBinary($"?ulong({name})"); break;
                        case PrimitiveType.Float: WriteBinary($"?float({name})"); break;
                        case PrimitiveType.Double: WriteBinary($"?double({name})"); break;
                        default: Pack(name, tag); break;
                    }
                    break;
                case SerializationTags.Custom custom:
                    WriteIoData($"{custom.PackFun}({name}{custom.Args.JoinStrings(arg => $", {arg.PackTag}")})");
                    break;
                default:
                    Pack(name, tag);
                    break;
            }
        }

        public void Pack(string name, SerializationTag tag)
        {
            WriteIoData($"igor_binary:pack_value({name}, {tag.PackTag})");
        }

        public void MaybePack(string name, SerializationTag tag)
        {
            WriteIoData($"igor_binary:maybe_pack_value({name}, {tag.PackTag})");
        }

        public void Render(Renderer r, string postfix = null)
        {
            r.Block(Render() + postfix);
        }

        public string Render()
        {
            var isBinary = values.All(v => v.kind == Kind.Binary);
            if (isBinary)
            {
                return $"<<{values.JoinStrings(", ", val => val.text)}>>";
            }
            else if (values.Count == 1)
            {
                return values[0].text;
            }
            else
            {
                var groups = values
                    .GroupAdjacentBy((v1, v2) => v1.kind == v2.kind && v1.kind == Kind.Binary)
                    .JoinStrings(",\n", g => g[0].kind == Kind.Binary ? $"<<{g.JoinStrings(", ", gg => gg.text)}>>" : g.First().text)
                    .Indent(4);
                return
$@"[
{groups}
]";
            }
        }

        public static string Render(params (string name, SerializationTag tag)[] vars)
        {
            return Render((IEnumerable<(string name, SerializationTag tag)>)vars);
        }

        public static string Render(IEnumerable<(string name, SerializationTag tag)> vars)
        {
            var stream = new ErlIgorWriter();
            foreach (var item in vars)
                stream.Var(item.name, item.tag);
            return stream.Render();
        }
    }
}
