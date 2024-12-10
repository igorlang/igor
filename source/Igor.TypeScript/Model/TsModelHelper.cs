using Igor.TypeScript.AST;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// Utilities for obtaining TypeScript models for AST types
    /// </summary>
    public static class TsModelHelper
    {
        /// <summary>
        /// Get the TypeScript file model for the AST module
        /// </summary>
        /// <param name="model">TypeScript model</param>
        /// <param name="astModule">AST module</param>
        /// <returns>TypeScript file model</returns>
        public static TsFile FileOf(this TsModel model, Module astModule)
        {
            return model.File(astModule.tsName, astModule.tsFileName);
        }

        /// <summary>
        /// Get the TypeScript file model for the AST form (top-level module definition)
        /// </summary>
        /// <param name="model">TypeScript model</param>
        /// <param name="astForm">AST form</param>
        /// <returns>TypeScript file model</returns>
        public static TsFile FileOf(this TsModel model, Form astForm)
        {
            return model.FileOf(astForm.Module);
        }

        public static T TypeOf<T>(this TsModel model, StructForm astStruct) where T : TsDeclaration
        {
            if (astStruct is InterfaceForm)
                return model.FileOf(astStruct).Interface(astStruct.tsName) as T;
            else
                return model.FileOf(astStruct).Class(astStruct.tsName) as T;
        }

        public static TsEnum TypeOf(this TsModel model, EnumForm astEnum)
        {
            return model.FileOf(astEnum).Enum(astEnum.tsName);
        }

        public static void ImportModule(this TsFile file, TsModule mod)
        {
            if (mod.Name == file.Name)
                return;
            file.Import($"import * as {mod.Name} from '{mod.ImportPath}';");
        }

        public static void ImportType(this TsFile file, TsType type)
        {
            void ImportTypeNoNested()
            {
                var typeModule = type.module;
                if (typeModule == null)
                    return;
                ImportModule(file, typeModule);
            }

            ImportTypeNoNested();
            foreach (var nestedType in type.nestedTypes)
            {
                ImportType(file, nestedType);
            }
        }
    }
}
