using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4
{
    internal class UeBinaryGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            var cpp = model.CppFile(mod.ueCppFile);
            var h = model.HFile(mod.ueHFile);

            foreach (var type in mod.Types)
            {
                if (type.ueEnabled && type.binaryEnabled)
                {
                    h.ForwardDeclaration("FIgorBinaryReader", StructType.Class);
                    h.ForwardDeclaration("FIgorBinaryWriter", StructType.Class);

                    var igorPath = type.ueIgorPath;
                    cpp.Include($"{igorPath}IgorBinary.h");
                    cpp.Include($"{igorPath}IgorBitField.h");
                    cpp.Include($"{igorPath}IgorBinaryReader.h");
                    cpp.Include($"{igorPath}IgorBinaryWriter.h");

                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumHeader(enumForm, h);
                            GenEnumCode(enumForm, cpp);
                            break;

                        case VariantForm variantForm:
                            GenRecordFuns(variantForm, h, cpp);
                            GenRecordHeader(variantForm, h);
                            GenRecordCode(variantForm, cpp);
                            break;

                        case RecordForm recordForm:
                            GenRecordFuns(recordForm, h, cpp);
                            GenRecordHeader(recordForm, h);
                            GenRecordCode(recordForm, cpp);
                            break;
                    }
                }
            }
        }

        private void GenEnumCode(EnumForm enumForm, UeCppFile cpp)
        {
            var igorNs = cpp.Namespace("Igor");
            var igorWriteBinary =
$@"void IgorWriteBinary(FIgorWriter& Writer, const {enumForm.ueName}& Value)
{{
    IgorWriteBinary(Writer, ({enumForm.ueIntType})Value);
}}
";
            var igorReadBinary =
$@"void IgorReadBinary(FIgorReader& Reader, {enumForm.ueName}& Result)
{{
    {enumForm.ueIntType} Value;
    IgorReadBinary(Reader, Value);
    Result = ({enumForm.ueName})Value;
}}
";

            igorNs.Function(igorWriteBinary, enumForm);
            igorNs.Function(igorReadBinary, enumForm);
        }

        private void GenEnumHeader(EnumForm enumForm, UeHFile h)
        {
            var ns = h.Namespace("Igor");
            ns.Function($"void IgorWriteBinary(FIgorWriter& Writer, const {enumForm.ueName}& Value);", enumForm);
            ns.Function($"void IgorReadBinary(FIgorReader& Reader, {enumForm.ueName}& Result);", enumForm);
        }

        private void GenRecordFuns(StructForm s, UeHFile h, UeCppFile cpp)
        {
            var ns = h.Namespace("Igor");
            ns.Function($"void IgorWriteBinary(FIgorWriter& Writer, const {s.ueType.QualifiedName}& Value);", s);
            ns.Function($"void IgorReadBinary(FIgorReader& Reader, {s.ueType.QualifiedName}& Result);", s);

            var igorNs = cpp.Namespace("Igor");
            var igorWriteBinary =
                    s is VariantForm ?
$@"void IgorWriteBinary(FIgorWriter& Writer, const {s.ueType.QualifiedName}& Value)
{{
    verifyf(Value.IsValid(), TEXT(""Value of {s.ueName} is not set""));
    {s.ueName}::WriteVariantBinary(Writer, Value);
}}"
                    :
$@"void IgorWriteBinary(FIgorWriter& Writer, const {s.ueType.QualifiedName}& Value)
{{
    verifyf(Value.IsValid(), TEXT(""Value of {s.ueName} is not set""));
    Value->WriteBinary(Writer);
}}";
            var igorReadBinary =
                s is VariantForm ?
$@"void IgorReadBinary(FIgorReader& Reader, {s.ueType.QualifiedName}& Result)
{{
    Result = {s.ueName}::ReadVariantBinary(Reader);
}}"
            :
$@"void IgorReadBinary(FIgorReader& Reader, {s.ueType.QualifiedName}& Result)
{{
    Result = MakeShareable(new {s.ueName}());
    Result->ReadBinary(Reader);
}}";

            igorNs.Function(igorWriteBinary, s);
            igorNs.Function(igorReadBinary, s);
        }

        private void GenRecordHeader(StructForm s, UeHFile h)
        {
            var structModel = h.Namespace(s.ueNamespace).StructOrClass(s.ueName);

            var maybeOverride = s.Ancestor != null ? " override" : null;
            var maybeAbstract = s is VariantForm ? " { }" : null;
            var writeBinary = $"virtual void WriteBinary(FIgorBinaryWriter& Writer) const{maybeOverride}{maybeAbstract};";
            var readBinary = $"virtual void ReadBinary(FIgorBinaryReader& Reader){maybeOverride}{maybeAbstract};";
            structModel.Function(writeBinary, AccessModifier.Public);
            structModel.Function(readBinary, AccessModifier.Public);

            if (s is VariantForm)
            {
                var readVariantBinary = $"static TSharedPtr<{s.ueName}> ReadVariantBinary(FIgorBinaryReader& Reader);";
                var writeVariantBinary = $"static void WriteVariantBinary(FIgorBinaryWriter& Writer, const {s.ueType.QualifiedName}& Value);";
                structModel.Function(readVariantBinary, AccessModifier.Public);
                structModel.Function(writeVariantBinary, AccessModifier.Public);
            }
        }

        private void GenRecordCode(StructForm s, UeCppFile cpp)
        {
            var serializedFields = s.Fields.Where(f => !f.binaryIgnore && !f.IsTag);

            if (!(s is VariantForm))
            {
                string writeBinary;

                if (s.binaryHeader)
                {
                    string writeField(RecordField field)
                    {
                        if (field.IsOptional)
                            return
$@"if ({field.ueName}.IsSet())
{{
    Igor::IgorWriteBinary(Writer, {field.ueName}.GetValue());
}}";
                        else
                            return $@"Igor::IgorWriteBinary(Writer, {field.ueName});";
                    }

                    writeBinary = $@"void {s.ueName}::WriteBinary(FIgorBinaryWriter& Writer) const
{{
    bool HeaderFields[] = {{ {serializedFields.JoinStrings(", ", f => f.ueBinaryHeader)} }};
    Igor::IgorBitField Header(HeaderFields, {serializedFields.Count()});
    Header.Write(Writer);
{serializedFields.JoinLines(writeField)}
}}";
                }
                else
                {
                    writeBinary =
$@"void {s.ueName}::WriteBinary(FIgorBinaryWriter& Writer) const
{{
{serializedFields.JoinLines(f => $"Igor::IgorWriteBinary(Writer, {f.ueName});")}
}}
";
                }

                string readBinary;
                if (s.binaryHeader)
                {
                    var serializedFieldsIndexed = serializedFields.SelectWithIndex((f, i) => (f, i));
                    string readField(RecordField field, int index)
                    {
                        if (field.IsOptional)
                        {
                            var nonOpt = Helper.TargetType(field.NonOptType);
                            return $@"if (Header.Get({index}))
{{
    {nonOpt.QualifiedName} {field.ueName}Value;
    Igor::IgorReadBinary(Reader, {field.ueName}Value);
    {field.ueName} = MoveTemp({field.ueName}Value);
}}
else
{{
    {field.ueName}.Reset();
}}";
                        }
                        else
                        {
                            return
$@"if (Header.Get({index}))
{{
    Igor::IgorReadBinary(Reader, {field.ueName});
}}";
                        }
                    }

                    readBinary =
$@"void {s.ueName}::ReadBinary(FIgorBinaryReader& Reader)
{{
    Igor::IgorBitField Header({serializedFields.Count()});
    Header.Read(Reader);
{serializedFieldsIndexed.JoinLines(((RecordField f, int i) fi) => readField(fi.f, fi.i))}
}}
";
                }
                else
                {
                    readBinary =
$@"void {s.ueName}::ReadBinary(FIgorBinaryReader& Reader)
{{
{serializedFields.JoinLines(f => $"Igor::IgorReadBinary(Reader, {f.ueName});")}
}}
";
                }

                cpp.DefaultNamespace.Function(writeBinary, s);
                cpp.DefaultNamespace.Function(readBinary, s);
            }

            if (s is VariantForm v)
            {
                string CategoryCase(RecordForm descendant)
                {
                    return $@"case {descendant.TagField.ueValue(null)}:
    Result = MakeShareable(new {descendant.ueName}());
    break;";
                }
                var readVariantBinary =
$@"TSharedPtr<{s.ueName}> {s.ueName}::ReadVariantBinary(FIgorBinaryReader& Reader)
{{
    TSharedPtr<{s.ueName}> Result;

    {s.TagField.ueType.QualifiedName} {s.TagField.ueName};
    Igor::IgorReadBinary(Reader, {s.TagField.ueName});
    switch ({s.TagField.ueName})
    {{
{v.Records.JoinLines(CategoryCase)}
    default:
        verifyf(false, TEXT(""Unknown variant tag""));
        break;
    }}

    Result->ReadBinary(Reader);
    return Result;
}}
";
                var writeVariantBinary =
$@"void {s.ueName}::WriteVariantBinary(FIgorBinaryWriter& Writer, const {s.ueType.QualifiedName}& Value)
{{
    Igor::IgorWriteBinary(Writer, Value->{s.TagField.ueTagGetter}());
    Value->WriteBinary(Writer);
}}
";

                cpp.DefaultNamespace.Function(readVariantBinary, s);
                cpp.DefaultNamespace.Function(writeVariantBinary, s);
            }
        }
    }
}
