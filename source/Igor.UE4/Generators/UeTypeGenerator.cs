using System;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;
using Igor.Text;

namespace Igor.UE4
{
    internal class UeTypeGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            var cppFile = model.CppFile(mod.ueCppFile);
            var hFile = model.HFile(mod.ueHFile);

            IncludeHeaders(mod, hFile, cppFile);

            foreach (var form in mod.Types)
            {
                if (form.ueGenerated)
                {
                    switch (form)
                    {
                        case EnumForm enumForm:
                            GenerateEnum(enumForm, hFile);
                            break;

                        case VariantForm variantForm:
                            GenerateStruct(variantForm, hFile);
                            break;

                        case RecordForm recordForm:
                            GenerateStruct(recordForm, hFile);
                            break;

                        case InterfaceForm interfaceForm:
                            GenerateStruct(interfaceForm, hFile);
                            break;

                        case DefineForm defineForm:
                            GenerateTypedef(defineForm, hFile);
                            break;
                    }
                }
            }
        }

        public void IncludeHeaders(Module mod, UeHFile hFile, UeCppFile cppFile)
        {
            cppFile.Include(mod.ueHFile);
            foreach (var incl in mod.ListAttribute(UeAttributes.CppInclude))
                cppFile.Include(incl);
            foreach (var incl in mod.ListAttribute(UeAttributes.HInclude))
                hFile.Include(incl);
        }

        public void GenerateEnum(EnumForm e, UeHFile h)
        {
            var enumModel = h.Namespace(e.ueNamespace).Enum(e.ueName);
            enumModel.UEnum = e.ueUEnum;
            enumModel.IntType = e.ueIntType;
            enumModel.Comment = e.Annotation;
            if (e.ueUEnum)
                h.Include("UObject/ObjectMacros.h");
            if (e.ueBlueprintType)
                enumModel.Specifier("BlueprintType");
            foreach (var field in e.Fields)
            {
                var f = enumModel.Field(field.ueName);
                foreach (var meta in field.ueMeta)
                {
                    if (meta.Value.IsBool && meta.Value.AsBool)
                        f.Meta[meta.Key] = null;
                    else if (meta.Value.IsBool)
                        f.Meta[meta.Key] = "false";
                    else if (meta.Value.IsString)
                        f.Meta[meta.Key] = meta.Value.AsString;
                }
                f.Value = field.Value;
                f.Comment = field.Annotation;
            }
        }

        public void GenerateStruct(StructForm structForm, UeHFile h)
        {
            var structModel = h.Namespace(structForm.ueNamespace).Struct(structForm.ueName);
            structModel.Comment = structForm.Annotation;
            structModel.ApiMacro = structForm.ueApiMacro;
            if (structForm.ueBlueprintType)
                structModel.Specifier("BlueprintType");
            if (structForm.ueUStruct)
            {
                structModel.UStruct = true;
            }
            else if (structForm.ueUClass)
            {
                structModel.UClass = true;
            }
            if (structForm.IsGeneric)
            {
                structModel.GenericArguments = structForm.Args.Select(arg => arg.ueName).ToArray();
            }
            foreach (var field in structForm.Fields.Where(f => !f.IsInherited && !f.IsTag && !f.ueIgnore))
            {
                if (structForm is InterfaceForm)
                {
                    if (field.IsLocal)
                        structModel.Function($"virtual const {field.ueType.RelativeName(structForm.ueNamespace)}& Get{field.ueName.Format(Notation.UpperCamel)}() const = 0;", AccessModifier.Public);
                    continue;
                }

                if (field.InterfaceDeclarations.Any(intfDecl => intfDecl.Struct is InterfaceForm intf && intf.ueEnabled))
                {
                    structModel.Function($"virtual const {field.ueType.RelativeName(structForm.ueNamespace)}& Get{field.ueName.Format(Notation.UpperCamel)}() const override {{ return {field.ueName}; }};", AccessModifier.Public);
                }

                var prop = structModel.Field(field.ueName);
                prop.Type = field.ueType.RelativeName(structForm.ueNamespace);
                prop.Value = field.ueValue(structForm.ueNamespace);
                prop.UProperty = field.ueUProperty;
                prop.Comment = field.Annotation;
                foreach (var incl in field.ueType.HIncludes)
                    if (incl != h.FileName)
                        h.Include(incl);
                foreach (var usedType in field.ueType.UsedTypes)
                {
                    if (usedType is UeUserType userType && userType.Form is StructForm usedStruct && usedStruct.ueGenerated && usedStruct.Module.ueHFile == h.FileName && !h.Namespace(usedStruct.ueNamespace).IsDefined(usedStruct.ueName))
                    {
                        h.ForwardDeclaration(usedStruct.ueName, StructType.Struct, usedStruct.ueNamespace);
                    }
                }

                if (field.ueBlueprintReadOnly)
                    prop.Specifier("BlueprintReadOnly");
                if (field.ueBlueprintReadWrite)
                    prop.Specifier("BlueprintReadWrite");
                if (field.ueEditAnywhere)
                    prop.Specifier("EditAnywhere");
                if (field.ueEditDefaultsOnly)
                    prop.Specifier("EditDefaultsOnly");
                if (field.ueVisibleAnywhere)
                    prop.Specifier("VisibleAnywhere");
                if (field.ueVisibleDefaultsOnly)
                    prop.Specifier("VisibleDefaultsOnly");
                if (field.ueCategory != null)
                    prop.Specifier("Category", field.ueCategory);
            }

            if (structForm.Ancestor != null)
                structModel.BaseType(structForm.Ancestor.ueName);
            else if (structForm.ueBaseType != null)
                structModel.BaseType(structForm.ueBaseType);

            foreach (var intf in structForm.Interfaces)
            {
                switch (intf)
                {
                    case InterfaceForm intfForm:
                        if (intfForm.ueEnabled)
                            structModel.BaseType(intfForm.ueName);
                        break;

                    case GenericInterface genericIntf:
                        // TODO:
                        break;
                }
            }

            if (structForm.Ancestor != null && structForm is RecordForm)
            {
                var tag = structForm.TagField;
                var category =
        $@"virtual {tag.ueType.QualifiedName} {tag.ueTagGetter}() const override
{{
    return {tag.ueType.FormatValue(tag.Default, structForm.ueNamespace)};
}}";
                structModel.Function(category, AccessModifier.Public);
            }

            if (structForm is VariantForm && structForm.Ancestor == null)
            {
                var tag = structForm.TagField;
                var value = tag.Default != null ? tag.ueType.FormatValue(tag.Default, structForm.ueNamespace) : $"({tag.ueType.QualifiedName})0";
                var category =
            $@"virtual {tag.ueType.QualifiedName} {tag.ueTagGetter}() const
{{
    return {value};
}}";
                structModel.Function(category, AccessModifier.Public);
            }

            if (structForm.Ancestor == null && structForm.ueBaseType == null)
            {
                var destructor = $@"virtual ~{structForm.ueName}() = default;";

                structModel.Function(destructor, AccessModifier.Public);
            }
        }

        public void GenerateTypedef(DefineForm define, UeHFile hFile)
        {
            var typedef = hFile.Namespace(define.ueNamespace).Typedef(define.ueName, define.ueTargetType.QualifiedBaseName);
            typedef.Comment = define.Annotation;
        }
    }
}
