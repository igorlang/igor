using Igor.Compiler;
using System.Collections.Generic;

namespace Igor.Declarations
{
    /// <summary>
    /// IScope interface provides name resolution
    /// </summary>
    public interface IScope
    {
        bool Find(SymbolReference reference, IList<ISymbolDeclaration> declarations);
    }

    public class UnionScope : IScope
    {
        private readonly List<IScope> scopes = new List<IScope>();

        public UnionScope(params IScope[] scopes)
        {
            this.scopes.AddRange(scopes);
        }

        public void AddScope(IScope scope)
        {
            scopes.Add(scope);
        }

        public void AddScopes(IEnumerable<IScope> newScopes)
        {
            scopes.AddRange(newScopes);
        }

        public bool Find(SymbolReference reference, IList<ISymbolDeclaration> declarations)
        {
            bool result = false;
            foreach (var scope in scopes)
            {
                result = result || scope.Find(reference, declarations);
            }

            return result;
        }
    }

    public class FallbackScope : IScope
    {
        private readonly IScope defaultScope;
        private readonly IScope fallbackScope;

        public FallbackScope(IScope defaultScope, IScope fallbackScope)
        {
            this.defaultScope = defaultScope;
            this.fallbackScope = fallbackScope;
        }

        public bool Find(SymbolReference reference, IList<ISymbolDeclaration> declarations)
        {
            if (defaultScope.Find(reference, declarations))
                return true;
            return fallbackScope.Find(reference, declarations);
        }
    }

    /// <summary>
    /// Name table that can be used to register and search for names
    /// </summary>
    public class SymbolTable : IScope
    {
        private readonly Dictionary<string, ISymbolDeclaration> symbols = new Dictionary<string, ISymbolDeclaration>();

        public void Register(ISymbolDeclaration declaration, CompileContext context, string name = null)
        {
            if (name == null) name = declaration.Name.Name;
            if (symbols.ContainsKey(name))
            {
                context.Output.Error(declaration.Name.Location, $"Duplicate definition {name}", ProblemCode.DuplicateDefinition);
            }
            else
            {
                symbols.Add(name, declaration);
            }
        }

        public bool Find(SymbolReference reference, IList<ISymbolDeclaration> declarations)
        {
            if (symbols.TryGetValue(reference.Name, out var declaration))
            {
                declarations.Add(declaration);
                return true;
            }
            return false;
        }
    }
}

