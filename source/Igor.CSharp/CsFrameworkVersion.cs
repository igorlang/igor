using Igor.Text;
using System;
using System.Linq;

namespace Igor.CSharp
{
    public enum CsFrameworkType
    {
        netstandard,
        netcoreapp,
        net,
    }

    public class CsFrameworkVersion
    {
        public static class NetStandard
        {
            public static readonly System.Version Version21 = new System.Version(2, 1);
            public static readonly System.Version Version20 = new System.Version(2, 0);
            public static readonly System.Version Version16 = new System.Version(1, 6);
            public static readonly System.Version Version15 = new System.Version(1, 5);
            public static readonly System.Version Version14 = new System.Version(1, 4);
            public static readonly System.Version Version13 = new System.Version(1, 3);
            public static readonly System.Version Version12 = new System.Version(1, 2);
            public static readonly System.Version Version11 = new System.Version(1, 1);
            public static readonly System.Version Version10 = new System.Version(1, 0);
        }

        public static class NetCore
        {
            public static readonly System.Version Version31 = new System.Version(3, 1);
            public static readonly System.Version Version30 = new System.Version(3, 1);
            public static readonly System.Version Version22 = new System.Version(2, 2);
            public static readonly System.Version Version21 = new System.Version(2, 1);
            public static readonly System.Version Version20 = new System.Version(2, 0);
            public static readonly System.Version Version11 = new System.Version(1, 1);
            public static readonly System.Version Version10 = new System.Version(1, 0);
        }

        public static class NetFramework
        {
            public static readonly System.Version Version48 = new System.Version(4, 8);
            public static readonly System.Version Version472 = new System.Version(4, 7, 2);
            public static readonly System.Version Version471 = new System.Version(4, 7, 1);
            public static readonly System.Version Version47 = new System.Version(4, 7);
            public static readonly System.Version Version462 = new System.Version(4, 6, 2);
            public static readonly System.Version Version461 = new System.Version(4, 6, 1);
            public static readonly System.Version Version46 = new System.Version(4, 6);
            public static readonly System.Version Version452 = new System.Version(4, 5, 2);
            public static readonly System.Version Version451 = new System.Version(4, 5, 1);
            public static readonly System.Version Version45 = new System.Version(4, 5);
            public static readonly System.Version Version40 = new System.Version(4, 0);
        }

        public CsFrameworkType Type { get; }
        public System.Version Version { get; }

        public static readonly CsFrameworkVersion Default = new CsFrameworkVersion(CsFrameworkType.netstandard, NetStandard.Version16);

        public CsFrameworkVersion(CsFrameworkType type, System.Version version)
        {
            Type = type;
            Version = version;
        }

        public bool Match(System.Version netstandard, System.Version core, System.Version net)
        {
            switch (Type)
            {
                case CsFrameworkType.netstandard: return netstandard != null && Version >= netstandard;
                case CsFrameworkType.netcoreapp: return core != null && Version >= core;
                case CsFrameworkType.net: return net != null && Version >= net;
                default: throw new ArgumentException();
            }
        }

        public static bool TryParse(string value, out CsFrameworkVersion framework)
        {
            framework = null;
            if (value.StartsWith("netstandard"))
            {
                if (System.Version.TryParse(value.RemovePrefix("netstandard"), out var version))
                    framework = new CsFrameworkVersion(CsFrameworkType.netstandard, version);
            }
            else if (value.StartsWith("netcoreapp"))
            {
                if (System.Version.TryParse(value.RemovePrefix("netstandard"), out var version))
                    framework = new CsFrameworkVersion(CsFrameworkType.netcoreapp, version);
            }
            else if (value.StartsWith("net"))
            {
                var versionString = value.RemovePrefix("net");
                if (versionString.Length >= 2 && versionString.Length <= 3 && versionString.All(char.IsDigit))
                {
                    var major = int.Parse(versionString[0].ToString());
                    var minor = int.Parse(versionString[1].ToString());
                    var build = versionString.Length == 3 ? int.Parse(versionString[2].ToString()) : 0;
                    framework = new CsFrameworkVersion(CsFrameworkType.net, new System.Version(major, minor, build));
                }
            }
            return framework != null;
        }
    }
}
