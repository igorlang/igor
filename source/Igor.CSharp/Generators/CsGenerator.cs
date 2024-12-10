using Igor.CSharp.AST;
using Igor.CSharp.Model;

namespace Igor.CSharp
{
    public interface ICsGenerator
    {
        void Generate(CsModel model, Module mod);
    }
}
