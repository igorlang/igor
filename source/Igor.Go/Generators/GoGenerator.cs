using Igor.Go.AST;
using Igor.Go.Model;

namespace Igor.Go
{
    public interface IGoGenerator
    {
        void Generate(GoModel model, Module mod);
    }
}
