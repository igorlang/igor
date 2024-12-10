using System.Collections.Generic;
using System.Linq;

namespace Igor.Erlang.Model
{
    public class ErlModule
    {
        private class TextGroup
        {
            public object Group { get; set; }
            public string Text { get; }

            public TextGroup(string text, object group)
            {
                this.Text = text;
                this.Group = group;
            }
        }

        public string Name { get; private set; }
        public string FileName { get; private set; }
        public string Comment { get; set; }

        private readonly List<string> includeLibs = new List<string>();
        private readonly List<string> includes = new List<string>();
        private readonly List<TextGroup> exports = new List<TextGroup>();
        private readonly List<TextGroup> exportTypes = new List<TextGroup>();
        private readonly List<string> callbacks = new List<string>();
        private readonly List<TextGroup> types = new List<TextGroup>();
        private readonly List<TextGroup> functions = new List<TextGroup>();
        private readonly List<string> defines = new List<string>();
        private readonly List<TextGroup> dialyzerOpts = new List<TextGroup>();
        private readonly List<string> behaviours = new List<string>();

        public bool IsEmpty => exports.Count == 0 && exportTypes.Count == 0 && callbacks.Count == 0;

        public IEnumerable<string> IncludeLibs => includeLibs;
        public IEnumerable<string> Includes => includes;
        public IEnumerable<string> Exports => Group(exports);
        public IEnumerable<string> ExportTypes => Group(exportTypes);
        public IEnumerable<string> Callbacks => callbacks;
        public IEnumerable<string> Types => Group(types);
        public IEnumerable<string> Functions => Group(functions);
        public IEnumerable<string> Defines => defines;
        public IEnumerable<string> DialyzerOpts => Group(dialyzerOpts);
        public IReadOnlyList<string> Behaviours => behaviours;

        internal ErlModule(string path)
        {
            this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            var srcPath = Context.Instance.Attribute(ErlAttributes.SrcPath, "src");
            this.FileName = $"{srcPath}/{path}.erl";
        }

        public void Include(string file)
        {
            if (!includes.Contains(file))
                includes.Add(file);
        }

        public void IncludeLib(string file)
        {
            if (!includeLibs.Contains(file))
                includeLibs.Add(file);
        }

        public void Export(string function, int arity, object group = null)
        {
            GetOrAdd(exports, $"{function}/{arity}", group);
        }

        public void ExportType(string type, int arity, object group = null)
        {
            GetOrAdd(exportTypes, $"{type}/{arity}", group);
        }

        public void Callback(string callback)
        {
            callbacks.Add(callback);
        }

        public void Type(string type, string desc, object group = null)
        {
            types.Add(new TextGroup($"-type {type}() :: {desc}.", group));
        }

        public void PolymorphicType(string type, string args, string desc, object group = null)
        {
            types.Add(new TextGroup($"-type {type}({args}) :: {desc}.", group));
        }

        public void Function(string func, object group = null)
        {
            functions.Add(new TextGroup(func, group));
        }

        public void Define(string macros)
        {
            defines.Add(macros);
        }

        public void Define(string name, string body)
        {
            Define($"-define({name}, {body}).");
        }

        public void DialyzerOpt(string opt, object group = null)
        {
            dialyzerOpts.Add(new TextGroup(opt, group));
        }

        public void Behaviour(string behaviour)
        {
            if (!behaviours.Contains(behaviour))
                behaviours.Add(behaviour);
        }

        private void GetOrAdd(List<TextGroup> textGroups, string text, object group)
        {
            var oldGroup = textGroups.Find(tg => tg.Text == text);
            if (oldGroup == null)
            {
                textGroups.Add(new TextGroup(text, group));
            }
            else if (oldGroup.Group == null)
            {
                oldGroup.Group = group;
            }
        }

        private IEnumerable<string> Group(List<TextGroup> textGroups) => textGroups.GroupBy(tg => tg.Group, tg => tg.Text).SelectMany(g => g);
    }
}
