using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Igor.Elixir.Model
{
    public class ExBlock
    {
        public string Text { get; }
        public string Annotation { get; set; }

        public ExBlock(string text)
        {
            Text = text;
        }
    }

    public class ExModule
    {
        public string Name { get; }
        public string Annotation { get; set; }

        internal List<string> Uses { get; } = new List<string>();
        internal List<string> Imports { get; } = new List<string>();
        internal List<string> Requires { get; } = new List<string>();
        internal List<ExBlock> Blocks { get; } = new List<ExBlock>();
        internal List<ExBlock> Callbacks { get; } = new List<ExBlock>();
        internal List<string> Behaviours { get; } = new List<string>();
        internal ExStruct Struct { get; private set; }

        internal ExModule(string name)
        {
            Name = name;
        }

        internal readonly Dictionary<string, ExModule> Modules = new Dictionary<string, ExModule>();

        public ExModule Module(string name)
        {
            return Modules.GetOrAdd(name, () => new ExModule(name));
        }

        public void Use(string module)
        {
            if (!Uses.Contains(module))
                Uses.Add(module);
        }

        public void Import(string module)
        {
            if (!Imports.Contains(module))
                Imports.Add(module);
        }

        public void Require(string module)
        {
            if (!Requires.Contains(module))
                Requires.Add(module);
        }

        public ExBlock Function(string text) => Block(text);

        public ExBlock Block(string text)
        {
            var result = new ExBlock(text);
            Blocks.Add(result);
            return result;
        }

        public ExStruct DefStruct()
        {
            Struct = Struct ?? new ExStruct();
            return Struct;
        }

        public ExStruct DefException()
        {
            Struct = Struct ?? new ExStruct();
            Struct.IsException = true;
            return Struct;
        }

        public ExBlock Callback(string spec)
        {
            var result = new ExBlock(spec);
            Callbacks.Add(result);
            return result;
        }

        public void Type(string name, string value)
        {
            Function($"@type {name} :: {value}");
        }

        public void Behaviour(string behaviour)
        {
            if (!Behaviours.Contains(behaviour))
                Behaviours.Add(behaviour);
        }
    }
}
