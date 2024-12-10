using Igor.CSharp.AST;

namespace Igor.CSharp.Model
{
    public static class CsModelHelper
    {
        public static CsFile FileOf(this CsModel model, Module astModule)
        {
            return model.File(astModule.csFileName);
        }

        public static CsFile FileOf(this CsModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }

        public static CsNamespace NamespaceOf(this CsModel model, Form astForm)
        {
            return model.FileOf(astForm.Module).Namespace(astForm.csNamespace);
        }

        public static CsClass TypeOf(this CsModel model, StructForm astStruct)
        {
            return model.NamespaceOf(astStruct).Class(astStruct.csName);
        }

        public static CsEnum TypeOf(this CsModel model, EnumForm astEnum)
        {
            return model.NamespaceOf(astEnum).Enum(astEnum.csName);
        }

        public static CsEnumField DeclOf(this CsModel model, EnumField astEnumField)
        {
            return TypeOf(model, astEnumField.Enum).Field(astEnumField.csName);
        }

        public static CsProperty DeclOf(this CsModel model, RecordField astRecordField)
        {
            return TypeOf(model, astRecordField.Struct).Property(astRecordField.csName);
        }
    }
}
