using Igor.CSharp.Model;
using Igor.Text;

namespace Igor.CSharp
{
    public static class CsVersion
    {
        public static readonly System.Version Version80 = new System.Version(8, 0);
        public static readonly System.Version Version73 = new System.Version(7, 3);
        public static readonly System.Version Version72 = new System.Version(7, 2);
        public static readonly System.Version Version71 = new System.Version(7, 1);
        public static readonly System.Version Version70 = new System.Version(7, 0);
        public static readonly System.Version Version60 = new System.Version(6, 0);
        public static readonly System.Version Version50 = new System.Version(5, 0);

        public static CsFrameworkVersion FrameworkVersion { get; set; } = CsFrameworkVersion.Default;

        public static bool UseTpl => Context.Instance.TargetVersion >= Version50 && Context.Instance.Attribute(CsAttributes.Tpl, true);

        public static bool SupportsOutVar => Context.Instance.TargetVersion >= Version70;

        public static bool SupportsSwitchExpression => Context.Instance.TargetVersion >= Version80;

        public static bool SupportsPropertyDefaults => Context.Instance.TargetVersion >= Version60;

        public static bool NullableReferenceTypes => Context.Instance.TargetVersion >= Version80 && Context.Instance.Attribute(CsAttributes.Nullable, false);

        public static bool SupportsArrayEmpty => FrameworkVersion.Match(CsFrameworkVersion.NetStandard.Version13, CsFrameworkVersion.NetCore.Version10, CsFrameworkVersion.NetFramework.Version46);

        public static void UsingTasks(CsFile file)
        {
            if (UseTpl)
            {
                file.Use("System.Threading.Tasks");
            }
        }

        public static string TaskClass => UseTpl ? "Task" : "Igor.Services.Rpc";
        public static string TaskException => UseTpl ? "Exception.InnerException" : "Exception";

        public static string NameOf(string name)
        {
            if (Context.Instance.TargetVersion >= Version60)
                return $"nameof({name})";
            else
                return name.Quoted();
        }

        public static void ReadOnlyProperty(this CsClass @class, string propName, string propDef, string expression)
        {
            if (Context.Instance.TargetVersion >= Version60)
                @class.Property(propName, $"{propDef} => {expression};");
            else
                @class.Property(propName, $@"{propDef}
{{
    get {{ return {expression}; }}
}}");
        }

        public static string Default(string expr)
        {
            if (Context.Instance.TargetVersion >= Version71)
                return "default";
            else
                return $"default({expr})";
        }

        public static string IsNull(string expr)
        {
            if (Context.Instance.TargetVersion >= Version70)
                return $"{expr} is null";
            else
                return $"ReferenceEquals({expr}, null)";
        }

        public static string EmptyArray(string type)
        {
            if (SupportsArrayEmpty)
                return $"System.Array.Empty<{type}>()";
            else
                return $"new {type}[0]";
        }
    }
}