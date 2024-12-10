using Igor.Text;
using System;
using System.Collections.Generic;

namespace Igor.UE4.Model
{
    public class UeFile
    {
        public string Name { get; private set; }
        public string FileName { get; private set; }

        internal readonly List<string> Includes = new List<string>();

        internal virtual bool IsEmpty => false;

        internal UeFile(string name)
        {
            this.Name = name;
            this.FileName = name;
        }

        public void Include(string file)
        {
            file = file.RemovePrefix("Public/", StringComparison.OrdinalIgnoreCase);
            file = file.RemovePrefix(@"Public\", StringComparison.OrdinalIgnoreCase);
            file = file.RemovePrefix("Private/", StringComparison.OrdinalIgnoreCase);
            file = file.RemovePrefix(@"Private\", StringComparison.OrdinalIgnoreCase);
            if (!Includes.Contains(file))
                Includes.Add(file);
        }
    }
}
