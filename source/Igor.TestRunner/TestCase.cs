using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Igor.TestRunner
{
    public class TestCase
    {
        public string Name { get; }
        public string Folder { get; }
        public IReadOnlyList<string> FileNames { get; }

        public TestCase(string name, string folder, IReadOnlyList<string> fileNames)
        {
            Name = name;
            Folder = folder;
            FileNames = fileNames;
        }
    }

    public static class TestCaseLoader
    {
        public static IReadOnlyList<TestCase> Load(string path)
        {
            var fileTests = Directory.EnumerateFiles(path, "*.igor", SearchOption.TopDirectoryOnly).Select(f => LoadFile(path, f));
            var folderTests = Directory.EnumerateDirectories(path).Select(LoadFolder);
            return fileTests.Concat(folderTests).ToList();
        }

        private static TestCase LoadFile(string rootPath, string fileName) =>
            new TestCase(Path.GetFileNameWithoutExtension(fileName), rootPath, new List<string> { Path.GetFileName(fileName) });

        private static TestCase LoadFolder(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.igor", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
            return new TestCase(Path.GetFileName(folder), folder, files);
        }
    }
}
