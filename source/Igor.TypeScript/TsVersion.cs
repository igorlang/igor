namespace Igor.TypeScript
{
    public static class TsVersion
    {
        public static System.Version Version => Context.Instance.TargetVersion;

        public static readonly System.Version Version27 = new System.Version(2, 7);
        public static readonly System.Version Version43 = new System.Version(4, 3);

        public static bool SupportsDefiniteAssignmentAssertions => Context.Instance.TargetVersion >= Version27;
        public static bool SupportsOverride => Context.Instance.TargetVersion >= Version43;
    }
}
