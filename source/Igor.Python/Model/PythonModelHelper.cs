using Igor.Python.AST;

namespace Igor.Python.Model
{
    public static class PythonModelHelper
    {
        public static PythonFile FileOf(this PythonModel model, Module astModule)
        {
            return model.File(astModule.pyFileName);
        }

        public static PythonFile FileOf(this PythonModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }

        public static PythonClass TypeOf(this PythonModel model, StructForm astStruct)
        {
            return model.FileOf(astStruct).Class(astStruct.pyName);
        }

        public static PythonEnum TypeOf(this PythonModel model, EnumForm astEnum)
        {
            return model.FileOf(astEnum).Enum(astEnum.pyName);
        }
    }
}
