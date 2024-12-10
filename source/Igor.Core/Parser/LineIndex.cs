using System.Collections.Generic;

namespace Igor.Parser
{
    public class LineIndex
    {
        private readonly List<int> lines = new List<int>();

        public LineIndex(string source)
        {
            int index = 0;
            do
            {
                lines.Add(index);
                index = source.IndexOf('\n', index);
                if (index < 0)
                    break;
                index++;
            } while (index < source.Length);
        }

        public int GetLine(int position)
        {
            int first = 0;
            int last = lines.Count - 1;
            int mid;
            do
            {
                mid = first + (last - first) / 2;
                if (position >= lines[mid] && (mid + 1 == lines.Count || position < lines[mid + 1]))
                    return mid + 1;
                if (position > lines[mid])
                    first = mid + 1;
                else
                    last = mid - 1;
            } while (first <= last);

            return mid + 1;
        }

        public void GetLineAndColumn(int position, out int line, out int column)
        {
            line = GetLine(position);
            column = position - lines[line - 1] + 1;
        }
    }
}
