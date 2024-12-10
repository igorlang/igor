using System.Collections.Generic;
using System.Linq;

namespace Igor.Go.Model
{
    public class GoImport
    {
        public string Path { get; }
        public string Identifier { get; set; }

        public GoImport(string path, string identifier = null)
        {
            Path = path;
            Identifier = identifier;
        }
    }

    public abstract class GoDeclaration
    {
        public object Group { get; set; }
        public string Comment { get; set; }
    }

    public class GoRawDeclaration : GoDeclaration
    {
        public string Text { get; }

        public GoRawDeclaration(string text, object group)
        {
            Text = text;
            Group = group;
        }
    }

    public abstract class GoTypeDeclaration : GoDeclaration
    {
        public string Name { get; }

        protected GoTypeDeclaration(string name) => Name = name;
    }

    public class GoTypeDefinition : GoTypeDeclaration
    {
        public string Type { get; set; }

        public GoTypeDefinition(string name) : base(name)
        {
        }

        public IDictionary<string, string> GenericArgs { get; set; }
    }

    public class GoFile
    {
        public string FileName { get; }
        public string PackagePath { get; }
        public string PackageName => PackagePath.Split('/').Last();

        internal bool IsEmpty => Declarations.Count == 0;

        internal GoFile(string filename, string package)
        {
            this.FileName = filename;
            this.PackagePath = package;
        }

        internal List<GoImport> Imports { get; } = new List<GoImport>();
        internal List<GoDeclaration> Declarations { get; } = new List<GoDeclaration>();
        public IEnumerable<GoEnum> Enums => Declarations.OfType<GoEnum>();
        public IEnumerable<GoStruct> Structs => Declarations.OfType<GoStruct>();
        public IEnumerable<GoInterface> Interfaces => Declarations.OfType<GoInterface>();
        public IEnumerable<GoTypeDefinition> TypeDefinitions => Declarations.OfType<GoTypeDefinition>();

        public GoEnum Enum(string name)
        {
            var result = Enums.FirstOrDefault(decl => decl.Name == name);
            if (result == null)
            {
                result = new GoEnum(name);
                Declarations.Add(result);
            }
            return result;
        }

        public GoStruct Struct(string name)
        {
            var result = Structs.FirstOrDefault(decl => decl.Name == name);
            if (result == null)
            {
                result = new GoStruct(name);
                Declarations.Add(result);
            }
            return result;
        }

        public GoInterface Interface(string name)
        {
            var result = Interfaces.FirstOrDefault(decl => decl.Name == name);
            if (result == null)
            {
                result = new GoInterface(name);
                Declarations.Add(result);
            }
            return result;
        }

        public GoTypeDefinition DefineType(string name)
        {
            var result = TypeDefinitions.FirstOrDefault(decl => decl.Name == name);
            if (result == null)
            {
                result = new GoTypeDefinition(name);
                Declarations.Add(result);
            }
            return result;
        }

        public void Import(string path, string identifier = null)
        {
            var import = Imports.GetOrAdd(path, imp => imp.Path, () => new GoImport(path));
            if (identifier != null)
                import.Identifier = identifier;
        }

        public void Declare(string block, object group)
        {
            Declarations.Add(new GoRawDeclaration(block, group));
        }
    }
}
