using Igor.Text;
using Igor.TypeScript.AST;
using Igor.TypeScript.Model;
using System;
using System.Linq;

namespace Igor.TypeScript
{
    internal class TsTypeGenerator : ITsGenerator
    {
        public void Generate(TsModel model, Module mod)
        {
            model.FileOf(mod).Import("import * as Igor from './igor';");
            foreach (var type in mod.Types)
            {
                if (type.tsGenerateDeclaration)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenerateEnum(model, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenerateClass(model, recordForm);
                            break;

                        case VariantForm variantForm:
                            GenerateClass(model, variantForm);
                            break;

                        case InterfaceForm interfaceForm:
                            GenerateIntf(model, interfaceForm);
                            break;
                    }
                }
            }
        }

        private void GenerateEnum(TsModel model, EnumForm enumForm)
        {
            var e = model.TypeOf(enumForm);
            e.Annotation = enumForm.Annotation;
            foreach (var field in enumForm.Fields)
            {
                var fieldModel = e.Field(field.tsName);
                fieldModel.Value = field.Value;
                fieldModel.Annotation = field.Annotation;
            }
        }

        private void GenerateIntf(TsModel model, InterfaceForm interfaceForm)
        {
            var c = model.TypeOf<TsInterface>(interfaceForm);
            c.Annotation = interfaceForm.Annotation;
            foreach (var i in interfaceForm.Interfaces)
            {
                var intfType = Helper.TargetInterface(i);
                c.Interface(intfType.relativeName(interfaceForm.tsNamespace));
                model.FileOf(interfaceForm).ImportType(intfType);
            }

            if (interfaceForm.IsGeneric)
                c.GenericArgs = interfaceForm.tsArgs.Select(a => a.relativeName(interfaceForm.tsNamespace)).ToList();

            foreach (var f in interfaceForm.Fields.Where(f => f.IsLocal))
            {
                var maybeOpt = f.tsType.isOptional ? "?" : "";
                c.Property($"{f.tsName}{maybeOpt}: {f.tsTypeName};");
            }
        }

        public void GenerateClass(TsModel model, StructForm structForm)
        {
            var file = model.FileOf(structForm);
            foreach (var f in structForm.Fields)
            {
                file.ImportType(f.tsType);
            }

            var c = model.TypeOf<TsClass>(structForm);
            c.Abstract = structForm is VariantForm;
            c.Annotation = structForm.Annotation;

            if (structForm.IsException)
                c.BaseClass = "Error";

            if (structForm.Ancestor != null)
                c.BaseClass = structForm.Ancestor.tsName;

            foreach (var i in structForm.Interfaces)
            {
                var intfType = Helper.TargetInterface(i);
                c.Interface(intfType.relativeName(structForm.tsNamespace));
                file.ImportType(intfType);
            }

            if (structForm.IsGeneric)
                c.GenericArgs = structForm.tsArgs.Select(a => a.relativeName(structForm.tsNamespace)).ToList();

            var errorMessageField = structForm.IsException ? structForm.Fields.FirstOrDefault(f => f.tsErrorMessage) : null;

            if (structForm.tsSetupCtor || structForm.IsException)
            {
                string ConstructorArgument(RecordField field)
                {
                    var modifier = (field.tsParameter && field != errorMessageField) ? field.tsModifier ?? "public " : "";
                    return $"{modifier}{field.tsVarName}: {field.tsTypeName}";
                }

                var r = new Renderer();
                var ctorFields = structForm.tsSetupCtor ? structForm.Fields : Array.Empty<RecordField>();
                r += $"constructor({ctorFields.JoinStrings(", ", ConstructorArgument)}) {{";
                r++;
                if (structForm.IsException)
                {
                    string errorMessage = null;
                    if (errorMessageField == null)
                        errorMessage = structForm.tsName.Quoted("'");
                    else if (structForm.tsSetupCtor)
                        errorMessage = errorMessageField.tsName;
                    r += $"super({errorMessage});";
                    if (structForm.Ancestor == null)
                        r += "Object.setPrototypeOf(this, new.target.prototype);";
                }
                foreach (var field in ctorFields)
                {
                    if (!field.tsParameter && field != errorMessageField)
                        r += $"this.{field.tsName} = {field.tsVarName};";
                }
                r--;
                r += "}";
                c.Constructor(r.Build());
            }

            foreach (var f in structForm.Fields.Where(f => !f.tsParameter && f != errorMessageField))
            {
                var maybeOverride = TsVersion.SupportsOverride && (f.IsInherited || f.tsErrorMessage) ? "override " : "";
                var maybeOpt = f.tsType.isOptional ? "?" : "";
                var maybeAssert = TsVersion.SupportsDefiniteAssignmentAssertions && !f.tsType.isOptional && !f.HasDefault ? "!" : "";
                var maybeDefault = (f.HasDefault || f.IsOptional) && !structForm.IsPatch ? $" = {f.tsDefault}" : "";
                c.Property($"{f.tsModifier}{maybeOverride}{f.tsName}{maybeOpt}{maybeAssert}: {f.tsTypeName}{maybeDefault};");
            }
        }
    }
}
