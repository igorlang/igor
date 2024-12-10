using Igor.Go.AST;

namespace Igor.Go.Model
{
    public static class GoModelHelper
    {
        public static GoFile FileOf(this GoModel model, Module astModule)
        {
            return model.Package(astModule.goPackage).File(astModule.goFileName);
        }

        public static GoFile FileOf(this GoModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }

        public static GoStruct TypeOf(this GoModel model, StructForm astStruct)
        {
            return model.FileOf(astStruct).Struct(astStruct.goName);
        }

        public static GoEnum TypeOf(this GoModel model, EnumForm astEnum)
        {
            return model.FileOf(astEnum).Enum(astEnum.goName);
        }

        public static GoInterface TypeOf(this GoModel model, InterfaceForm astIntf)
        {
            return model.FileOf(astIntf).Interface(astIntf.goName);
        }

        public static GoTypeDefinition TypeOf(this GoModel model, DefineForm astDefine)
        {
            return model.FileOf(astDefine).DefineType(astDefine.goName);
        }

        public static GoProperty PropertyOf(this GoModel model, RecordField astField)
        {
            return model.TypeOf(astField.Struct).Property(astField.goName);
        }
    }
}
