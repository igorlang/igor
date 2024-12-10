using System;

namespace Igor.TypeScript
{
    public class TsModule : IEquatable<TsModule>
    {
        public string Name { get; }
        public string ImportPath { get; }

        public TsModule(string name, string importPath)
        {
            this.Name = name;
            this.ImportPath = importPath;
        }

        public bool Equals(TsModule other)
        {
            if (other == null)
                return false;
            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is TsModule module)
                return Equals(module);
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
