using System.Reflection;

namespace Igor
{
    /// <summary>
    /// Igor Version
    /// </summary>
    public static class Version
    {
        internal static System.Version AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>
        /// Igor version string with revision number
        /// </summary>
        public static string VersionString => $"igorc {AssemblyVersion}";

        /// <summary>
        /// Igor version string without revision number. This version string should be included into generated file header
        /// instead of the full VersionString, to ensure that generated file content doesn't change between revisions.
        /// </summary>
        public static string HeaderVersionString => $"igorc {AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
    }
}
