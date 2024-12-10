using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;
using System.Linq;

namespace Igor.CSharp
{
    internal class CsBinaryGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var type in mod.Types)
            {
                if (type.binaryEnabled && type.csEnabled)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            if (enumForm.csBinaryGenerateSerializer)
                                GenerateEnum(model, enumForm);
                            break;
                        case RecordForm recordForm:
                            if (recordForm.csBinaryGenerateSerializer)
                                GenerateStruct(model, recordForm);
                            break;
                        case VariantForm variantForm:
                            if (variantForm.csBinaryGenerateSerializer)
                            {
                                GenerateStruct(model, variantForm);
                                GenerateVariant(model, variantForm);
                            }
                            break;
                    }
                }
            }
        }

        public void GenerateEnum(CsModel model, EnumForm enumForm)
        {
            var file = model.FileOf(enumForm);
            file.UseAlias("IgorSerializer", "Igor.Serialization.IgorSerializer");
            file.Use("System.IO");
            var csTypeName = enumForm.csType.relativeName(enumForm.csBinaryNamespace);
            var c = file.Namespace(enumForm.csBinaryNamespace).Class(enumForm.csBinaryGeneratedSerializerClass);
            c.Sealed = true;
            c.Interface($"Igor.Serialization.IBinarySerializer<{csTypeName}>");

            var intSerializer = Helper.PrimitiveSerializer(Primitive.FromInteger(enumForm.IntType));

            c.Method(
$@"public void Serialize(BinaryWriter writer, {csTypeName} value)
{{
    {intSerializer}.Serialize(writer, ({enumForm.csIntType})value);
}}
");

            c.Method(
$@"public {csTypeName} Deserialize(BinaryReader reader)
{{
    return ({csTypeName}){intSerializer}.Deserialize(reader);
}}
");

            c.Property("Instance",
$@"public static readonly {enumForm.csBinaryGeneratedSerializerClass} Instance = new {enumForm.csBinaryGeneratedSerializerClass}();
");
        }

        public void GenerateStruct(CsModel model, StructForm structForm)
        {
            var file = model.FileOf(structForm);
            file.UseAlias("IgorSerializer", "Igor.Serialization.IgorSerializer");
            file.Use("System.IO");
            var c = file.Namespace(structForm.csBinaryNamespace).Class(structForm.csBinaryGeneratedSerializerClass);
            c.Sealed = true;
            var typeName = structForm.csType.relativeName(structForm.csBinaryNamespace);
            if (structForm.IsGeneric)
                c.GenericArgs = structForm.Args.Select(arg => arg.csName).ToList();
            c.Interface($"Igor.Serialization.IBinarySerializer<{typeName}>");

            if (!(structForm is VariantForm))
            {
                if (structForm.binaryHeader && structForm.csBinaryBitFields.Any())
                {
                    var bitFieldsArgs = structForm.csBinaryBitFields.JoinStrings(", ", field => "value." + field.csNotNull);
                    c.Method(
$@"public void Serialize(BinaryWriter writer, {typeName} value)
{{
{structForm.csRequire("value")}
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryRequire("value"))}
    var bits = new Igor.Serialization.BitField({bitFieldsArgs});
    bits.Write(writer);
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryWrite)}
}}
");
                }
                else
                {
                    c.Method(
$@"public void Serialize(BinaryWriter writer, {typeName} value)
{{
{structForm.csRequire("value")}
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryRequire("value"))}
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryWriteNoHeader)}
}}
");
                }

                var refKeyword = structForm.csReference ? "" : "ref";

                c.Method(
$@"public {typeName} Deserialize(BinaryReader reader)
{{
    var result = new {typeName}();
    Deserialize(reader, {refKeyword} result);
    return result;
}}
");

                if (structForm.binaryHeader && structForm.csBinaryBitFields.Any())
                    c.Method(
$@"public void Deserialize(BinaryReader reader, {refKeyword} {typeName} value)
{{
{structForm.csRequire("value")}
    var bits = new Igor.Serialization.BitField({structForm.csBinaryBitFields.Count()});
    bits.Read(reader);
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryRead)}
}}
");
                else
                    c.Method(
$@"public void Deserialize(BinaryReader reader, {refKeyword} {typeName} value)
{{
{structForm.csRequire("value")}
{structForm.binarySerializedFields.JoinLines(field => field.csBinaryReadNoHeader)}
}}
");
            }

            if (structForm.IsGeneric)
            {
                foreach (var arg in structForm.Args)
                {
                    c.Property(arg.csVarName, $"{arg.csBinarySerializerType} {arg.csVarName};");
                }

                var ctorArgs = structForm.Args.JoinStrings(", ", arg => $"{arg.csBinarySerializerType} {arg.csVarName}");
                var ctorInitArgs = structForm.Args.JoinLines(arg => $"    this.{arg.csVarName} = {arg.csVarName};");

                c.Constructor(
$@"public {structForm.csBinaryGeneratedSerializerClass}({ctorArgs})
{{
{ctorInitArgs}
}}
");
            }
            else
            {
                c.Property("Instance", $"public static readonly {structForm.csBinaryGeneratedSerializerClass} Instance = new {structForm.csBinaryGeneratedSerializerClass}();");
            }
        }

        public void GenerateVariant(CsModel model, VariantForm variantForm)
        {
            var c = model.FileOf(variantForm).Namespace(variantForm.csBinaryNamespace).Class(variantForm.csBinaryGeneratedSerializerClass);
            var csTypeName = variantForm.csType.relativeName(variantForm.csBinaryNamespace);

            string csIgorDeserializeDescendant(RecordForm desc) =>
$@"        case {desc.TagField.csDefault}:
            return {desc.csBinarySerializerInstance(variantForm.csBinaryNamespace)}.Deserialize(reader);
";

            string csIgorSerializeDescendant(RecordForm desc) =>
$@"        case {desc.TagField.csDefault}:
            {desc.csBinarySerializerInstance(variantForm.csBinaryNamespace)}.Serialize(writer, ({desc.csType.relativeName(variantForm.csBinaryNamespace)})value);
            break;
";

            c.Method(
$@"public void Serialize(BinaryWriter writer, {csTypeName} value)
{{
{variantForm.csRequire("value")}
    {variantForm.TagField.csBinarySerializer}.Serialize(writer, value.{variantForm.TagField.csName});
    switch (value.{variantForm.TagField.csName})
    {{
{variantForm.Records.JoinStrings(csIgorSerializeDescendant)}
        default:
            throw new System.ArgumentException(""Invalid variant tag"");
    }}
}}
");

            c.Method(
$@"public {csTypeName} Deserialize(BinaryReader reader)
{{
    {variantForm.TagField.csTypeName} {variantForm.TagField.csVarName} = {variantForm.TagField.csBinarySerializer}.Deserialize(reader);
    switch ({variantForm.TagField.csVarName})
    {{
{variantForm.Records.JoinStrings(csIgorDeserializeDescendant)}
        default:
            throw new System.ArgumentException(""Invalid variant tag"");
    }}
}}
");
        }
    }
}
