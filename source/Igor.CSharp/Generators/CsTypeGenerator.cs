using Igor.CSharp.AST;
using Igor.CSharp.Model;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsTypeGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            var file = model.FileOf(mod);
            file.Use("System.Collections.Generic");

            foreach (var type in mod.Types)
            {
                if (type.csGenerateDeclaration)
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

        private void GenerateEnum(CsModel model, EnumForm enumForm)
        {
            var e = model.TypeOf(enumForm);
            e.Summary = enumForm.Annotation;
            e.AddAttributes(enumForm.ListAttribute(CsAttributes.Attribute));
            if (enumForm.csEnumBaseTypes)
                e.IntType = enumForm.csIntType;
            if (enumForm.Flags)
                e.AddAttribute("System.Flags");
            foreach (var field in enumForm.Fields)
            {
                var fieldModel = e.Field(field.csName);
                fieldModel.Summary = field.Annotation;
                fieldModel.Value = field.Value;
                fieldModel.AddAttributes(field.ListAttribute(CsAttributes.Attribute));
            }
        }

        private void GenerateIntf(CsModel model, InterfaceForm interfaceForm)
        {
            var c = model.TypeOf(interfaceForm);
            c.Summary = interfaceForm.Annotation;
            c.Kind = ClassKind.Interface;
            c.AddAttributes(interfaceForm.ListAttribute(CsAttributes.Attribute));
            foreach (var i in interfaceForm.Interfaces)
                c.Interface(Helper.TargetInterface(i).relativeName(interfaceForm.csNamespace));
            if (interfaceForm.IsGeneric)
                c.GenericArgs = interfaceForm.csArgs.Select(a => a.relativeName(interfaceForm.csNamespace)).ToList();
            foreach (var f in interfaceForm.Fields.Where(f => f.IsLocal))
                c.Property(f.csName, $"{f.csTypeName} {f.csName} {{ get; set; }}");
        }

        public void GenerateClass(CsModel model, StructForm structForm)
        {
            var c = model.TypeOf(structForm);
            c.Summary = structForm.Annotation;
            c.AddAttributes(structForm.ListAttribute(CsAttributes.Attribute));
            c.Partial |= structForm.csPartial;
            c.Sealed |= structForm.csSealed;
            c.Kind = structForm.csReference ? ClassKind.Class : ClassKind.Struct;
            c.Abstract = structForm is VariantForm;

            if (structForm.IsException)
                c.BaseClass = "Igor.Services.CustomRemoteException";

            if (structForm.Ancestor != null)
                c.BaseClass = structForm.Ancestor.csName;

            foreach (var i in structForm.Interfaces)
                c.Interface(Helper.TargetInterface(i).relativeName(structForm.csNamespace));

            if (structForm.IsGeneric)
                c.GenericArgs = structForm.csArgs.Select(a => a.relativeName(structForm.csNamespace)).ToList();

            var features = RecordFeatures.None;
            if (structForm.csDefaultCtor)
                features |= RecordFeatures.DefaultCtor;
            if (structForm.csSetupCtor)
                features |= RecordFeatures.SetupCtor;
            if (structForm.csGenerateEquals)
                features |= RecordFeatures.EqualsAndGetHashCode;
            if (structForm.csEquality)
                features |= RecordFeatures.Equality;
            if (structForm.Ancestor != null)
                features |= RecordFeatures.IsInherited;
            if (structForm is VariantForm)
                features |= RecordFeatures.Abstract;
            if (structForm.Ancestor != null && structForm.Ancestor.csGenerateEquals)
                features |= RecordFeatures.InheritedGetHashCode;
            if (structForm.csType.isReference)
                features |= RecordFeatures.Reference;
            if (structForm.csImmutable)
                features |= RecordFeatures.Immutable;

            c.Record(structForm.Fields.Select(f => f.csProperty).ToList(), structForm.csNamespace, features);
        }
    }
}
