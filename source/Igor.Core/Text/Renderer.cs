using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igor.Text
{
    public class Renderer
    {
        public enum EmptyLineMode
        {
            Keep,
            Allow,
            Suggest,
            Disable,
            Enforce,
            Forbid,
        }

        public string Tab { get; set; }
        public bool RemoveDoubleSpaces { get; set; }
        public bool EndWithEmptyLine { get; set; }

        private int indent;
        private string indentString = "";
        private int emptyLines;
        private EmptyLineMode currentEmptyLineMode = EmptyLineMode.Forbid;
        private string lastLine = "";
        private string currentLine = "";

        private readonly StringBuilder builder = new StringBuilder();

        public Renderer()
        {
            Tab = "    ";
            RemoveDoubleSpaces = false;
        }

        public void Indent(int t = 1)
        {
            MaybeFinishCurrentLine();
            indent += t;
            indentString = Enumerable.Repeat(Tab, indent).JoinStrings();
        }

        public void Outdent(int t = 1)
        {
            MaybeFinishCurrentLine();
            indent -= t;
            indentString = Enumerable.Repeat(Tab, indent).JoinStrings();
        }

        private void PushEmptyLineMode(EmptyLineMode emptyLineMode)
        {
            currentEmptyLineMode = (EmptyLineMode)Math.Max((int)currentEmptyLineMode, (int)emptyLineMode);
        }

        public void EmptyLine()
        {
            MaybeFinishCurrentLine();
            PushEmptyLineMode(EmptyLineMode.Enforce);
        }

        public void Line(string line)
        {
            MaybeFinishCurrentLine();
            if (string.IsNullOrWhiteSpace(line))
                emptyLines++;
            else
                AppendNonEmptyLine(line);
        }

        public void Append(string str)
        {
            currentLine += str;
            if (currentLine.EndsWith("\n"))
                MaybeFinishCurrentLine();
        }

        public void NewLine()
        {
            MaybeFinishCurrentLine();
        }

        public void Format(string format, params object[] args)
        {
            AppendBlock(string.Format(format, args));
        }

        public void Block(string block)
        {
            AppendBlock(block);
        }

        private void AppendNonEmptyLine(string line)
        {
            var line1 = line.TrimEnd();
            var trim = line1.TrimStart();
            PushEmptyLineMode(GetEmptyLineModeBetweenLines(lastLine, trim));
            lastLine = trim;
            FlushEmptyLines();
            builder.Append(indentString);
            if (RemoveDoubleSpaces)
                builder.AppendLine(StringHelper.RemoveDoubleSpaces(line1));
            else
                builder.AppendLine(line1);
        }

        protected virtual EmptyLineMode GetEmptyLineModeBetweenLines(string prev, string next) => EmptyLineMode.Keep;

        private void FlushEmptyLines()
        {
            bool shouldHaveEmptyLine;
            switch (currentEmptyLineMode)
            {
                case EmptyLineMode.Forbid:
                    shouldHaveEmptyLine = false;
                    break;
                case EmptyLineMode.Enforce:
                    shouldHaveEmptyLine = true;
                    break;
                case EmptyLineMode.Disable:
                    shouldHaveEmptyLine = false;
                    break;
                case EmptyLineMode.Suggest:
                    shouldHaveEmptyLine = true;
                    break;
                default:
                    shouldHaveEmptyLine = emptyLines > 0;
                    break;
            }
            if (shouldHaveEmptyLine)
                builder.AppendLine();
            emptyLines = 0;
            currentEmptyLineMode = EmptyLineMode.Keep;
        }

        private void MaybeFinishCurrentLine()
        {
            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                AppendNonEmptyLine(currentLine);
            }
            currentLine = "";
        }

        private void AppendBlock(string block, bool emptyLineDelimiter = false)
        {
            MaybeFinishCurrentLine();
            if (emptyLineDelimiter)
                EmptyLine();
            foreach (var line in block.Split('\n'))
                Line(line.TrimEnd('\r'));
            if (emptyLineDelimiter)
                EmptyLine();
        }

        public void Comment(string block, string commentPrefix, int maxWidth = 160)
        {
            if (block == null)
                return;
            var w = maxWidth - commentPrefix.Length;
            foreach (var line in block.Split('\n'))
            {
                var l = line.TrimEnd();
                int i = 0;
                while (l.Length - i > w)
                {
                    var ii = l.LastIndexOf(' ', i + w, w);
                    if (ii < 0)
                        break;
                    Line(commentPrefix + l.Substring(i, ii - i).Trim());
                    i = ii;
                }
                Line(commentPrefix + l.Substring(i).Trim());
            }
        }

        public void Blocks(IEnumerable<string> blocks, bool emptyLineDelimiter = false, string delimiter = null, string lastLinePostfix = null)
        {
            string prevBlock = null;
            foreach (var block in blocks)
            {
                if (prevBlock != null)
                    AppendBlock(prevBlock + delimiter, emptyLineDelimiter);
                prevBlock = block;
            }
            if (prevBlock != null)
                AppendBlock(prevBlock + lastLinePostfix, emptyLineDelimiter);
        }

        public void Blocks<T>(IEnumerable<T> blocks, Func<T, string> format, bool emptyLineDelimiter = false, string delimiter = null, string lastLinePostfix = null)
        {
            var source = blocks.Select(format);
            string prevBlock = null;
            foreach (var block in source)
            {
                if (prevBlock != null)
                    AppendBlock(prevBlock + delimiter, emptyLineDelimiter);
                prevBlock = block;
            }
            if (prevBlock != null)
                AppendBlock(prevBlock + lastLinePostfix, emptyLineDelimiter);
        }

        public void ForEach<T>(IEnumerable<T> blocks, Action<T> writer, bool emptyLineDelimiter = false)
        {
            foreach (var block in blocks)
            {
                if (emptyLineDelimiter)
                    EmptyLine();
                writer(block);
                if (emptyLineDelimiter)
                    EmptyLine();
            }
        }

        public void ForEach<T>(IEnumerable<T> blocks, Action<T, bool> writer, bool emptyLineDelimiter = false)
        {
            var blocks2 = blocks.ToList();
            for (int i = 0; i < blocks2.Count; i++)
            {
                bool isLast = i == blocks2.Count - 1;
                if (emptyLineDelimiter)
                    EmptyLine();
                writer(blocks2[i], isLast);
                if (emptyLineDelimiter)
                    EmptyLine();
            }
        }

        public void Table(IEnumerable<IReadOnlyList<string>> rows, bool emptyLineDelimiter = false, string rowDelimiter = null, string lastLinePostfix = null, int? delimiterPosition = null)
        {
            var rowsList = rows.ToList();
            var colCount = rowsList.Max(r => r.Count);
            var widths = new int[colCount];
            for (int i = 0; i < colCount; i++)
            {
                widths[i] = rowsList.Max(r => r.Count > i ? r[i].Length : 0);
                if (delimiterPosition == i && rowDelimiter != null)
                    widths[i] += rowDelimiter.Length;
            }
            for (int n = 0; n < rowsList.Count; n++)
            {
                var row = rowsList[n];
                if (emptyLineDelimiter)
                    EmptyLine();
                var sb = new StringBuilder();
                for (int i = 0; i < row.Count; i++)
                {
                    var item = row[i];
                    sb.Append(item);
                    var appendSpace = widths[i] + 1 - item.Length;
                    if (rowDelimiter != null && delimiterPosition.HasValue && delimiterPosition == i && n < rowsList.Count - 1)
                    {
                        sb.Append(rowDelimiter);
                        appendSpace -= rowDelimiter.Length;
                    }

                    if (i < row.Count - 1)
                        sb.Append(' ', appendSpace);
                }
                if (rowDelimiter != null && n < rowsList.Count - 1 && !delimiterPosition.HasValue)
                    sb.Append(rowDelimiter);
                if (lastLinePostfix != null && n == rowsList.Count - 1)
                    sb.Append(lastLinePostfix);
                Line(sb.ToString());
                if (emptyLineDelimiter)
                    EmptyLine();
            }
        }

        public void Table<T>(IEnumerable<T> rows, Func<T, IReadOnlyList<string>> rowSelector, bool emptyLineDelimiter = false, string rowDelimiter = null, string lastLinePostfix = null, Action<T> beforeRow = null)
        {
            var rowsList = rows.Select(row => (row, rowSelector(row))).ToList();
            if (!rowsList.Any())
                return;
            var colCount = rowsList.Max(r => r.Item2.Count);
            var widths = new int[colCount];
            for (int i = 0; i < colCount; i++)
            {
                widths[i] = rowsList.Max(r => r.Item2.Count > i ? r.Item2[i].Length : 0);
            }
            for (int n = 0; n < rowsList.Count; n++)
            {
                var (row, rowItems) = rowsList[n];
                if (emptyLineDelimiter)
                    EmptyLine();
                beforeRow?.Invoke(row);
                var sb = new StringBuilder();
                for (int i = 0; i < rowItems.Count; i++)
                {
                    var item = rowItems[i];
                    sb.Append(item);
                    if (i < rowItems.Count - 1)
                        sb.Append(' ', widths[i] + 1 - item.Length);
                }
                if (rowDelimiter != null && n < rowsList.Count - 1)
                    sb.Append(rowDelimiter);
                if (lastLinePostfix != null && n == rowsList.Count - 1)
                    sb.Append(lastLinePostfix);
                Line(sb.ToString());
                if (emptyLineDelimiter)
                    EmptyLine();
            }
        }

        public string Build()
        {
            MaybeFinishCurrentLine();
            if (EndWithEmptyLine)
                builder.AppendLine();
            return builder.ToString();
        }

        public void Reset()
        {
            builder.Clear();
        }

        public static Renderer operator +(Renderer renderer, string block)
        {
            if (block == null)
                renderer.EmptyLine();
            else
                renderer.Block(block);
            return renderer;
        }

        public static Renderer operator *(Renderer renderer, string value)
        {
            renderer.Append(value);
            return renderer;
        }

        public static Renderer operator ++(Renderer renderer)
        {
            renderer.Indent();
            return renderer;
        }

        public static Renderer operator --(Renderer renderer)
        {
            renderer.Outdent();
            return renderer;
        }
    }
}
