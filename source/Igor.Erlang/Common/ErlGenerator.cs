using Igor.Erlang.AST;
using Igor.Erlang.Model;

namespace Igor.Erlang
{
    public interface IErlangGenerator
    {
        void Generate(ErlModel model, Module mod);
    }
}
