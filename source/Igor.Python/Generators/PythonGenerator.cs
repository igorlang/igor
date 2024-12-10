using Igor.Python.AST;
using Igor.Python.Model;

namespace Igor.Python
{
    public interface IPythonGenerator
    {
        void Generate(PythonModel model, Module mod);
    }
}
