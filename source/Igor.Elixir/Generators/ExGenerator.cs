using Igor.Elixir.AST;
using Igor.Elixir.Model;

namespace Igor.Elixir
{
    public interface IElixirGenerator
    {
        void Generate(ExModel model, Module mod);
    }
}
