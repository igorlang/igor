using Igor.Compiler;
using Igor.Text;
using System.IO;
using System.Text;

namespace Igor
{
    /// <summary>
    /// Generated target file
    /// </summary>
    public class TargetFile
    {
        /// <summary>
        /// File name, relative to output folder
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// File content
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Is the file considered to be empty (contains no meaningful declarations)?
        /// Empty files are not created in the file system.
        /// Empty files are deleted, if the related option is provided.
        /// </summary>
        public bool Empty { get; }

        public TargetFile(string name, string text, bool empty)
        {
            this.Name = name;
            this.Text = text;
            this.Empty = empty;
        }
    }
}
