using Igor.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Igor.Compiler
{
    internal static class FileSearch
    {
        private static bool FindFile(string fileName, IEnumerable<string> paths, out string file)
        {
            string GetFullPath(string path) => Path.GetFullPath(Path.Combine(path, fileName));
            file = paths.Select(GetFullPath).FirstOrDefault(File.Exists);
            return file != null;
        }

        private static bool IsMask(string filename)
        {
            return filename.Contains('*') || filename.Contains('?');
        }

        private static IEnumerable<string> ResolveMask(string mask, string path, IList<string> directoriesNotFound)
        {
            while (mask.StartsWith(".."))
            {
                mask = mask.RemovePrefix("..").RemovePrefix(Path.DirectorySeparatorChar.ToString()).RemovePrefix(Path.AltDirectorySeparatorChar.ToString());
                path = Path.Combine(path, "..");
            }

            try
            {
                if (Path.IsPathRooted(mask))
                    return new DirectoryInfo(Path.GetDirectoryName(mask)).GetFiles(Path.GetFileName(mask)).Select(fileInfo => fileInfo.FullName);
                else
                    return new DirectoryInfo(path).GetFiles(mask).Select(fileInfo => fileInfo.FullName);
            }
            catch (DirectoryNotFoundException exception)
            {
                directoriesNotFound.Add(exception.Message);
                return Array.Empty<string>();
            }
        }

        public static IEnumerable<string> FindFiles(string fileNameOrMask, IEnumerable<string> paths, IList<string> filesNotFound, IList<string> directoriesNotFound)
        {
            if (IsMask(fileNameOrMask))
                return paths.SelectMany(path => ResolveMask(fileNameOrMask, path, directoriesNotFound));
            if (FindFile(fileNameOrMask, paths, out var file))
                return file.Yield();
            filesNotFound.Add(fileNameOrMask);
            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> FileList(IEnumerable<string> files, IEnumerable<string> paths, IList<string> filesNotFound, IList<string> directoriesNotFound)
        {
            return files.SelectMany(file => FindFiles(file, paths, filesNotFound, directoriesNotFound));
        }
    }
}
