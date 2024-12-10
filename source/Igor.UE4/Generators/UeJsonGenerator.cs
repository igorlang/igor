using Igor.Text;
using Igor.UE4.AST;
using Igor.UE4.Model;
using System.Linq;

namespace Igor.UE4
{
    internal class UeJsonGenerator : IUeGenerator
    {
        public void Generate(UeModel model, Module mod)
        {
            var cppFile = model.CppFile(mod.ueCppFile);
            var hFile = model.HFile(mod.ueHFile);

            foreach (var type in mod.Types)
            {
                if (type.ueEnabled && type.jsonEnabled && !type.ueJsonCustomSerializer)
                {
                    hFile.Include("Json.h");
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumHeader(enumForm, hFile);
                            if (enumForm.jsonNumber)
                                GenEnumNumberCode(enumForm, cppFile);
                            else
                                GenEnumStringCode(enumForm, cppFile);
                            break;

                        case VariantForm variantForm:
                            GenRecordFuns(variantForm, hFile, cppFile);
                            break;

                        case RecordForm recordForm:
                            GenRecordFuns(recordForm, hFile, cppFile);
                            break;
                    }
                }
            }
        }

        public void GenEnumHeader(EnumForm enumForm, UeHFile h)
        {
            var ns = h.Namespace("Igor");
            if (!enumForm.jsonNumber)
                ns.Function($"bool IgorReadJsonKey(const FString& String, {enumForm.ueType.QualifiedName}& Result);", enumForm);
            ns.Function($"bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {enumForm.ueType.QualifiedName}& Result);", enumForm);
            if (!enumForm.jsonNumber)
                ns.Function($"FString IgorWriteJsonKey(const {enumForm.ueType.QualifiedName}& Value);", enumForm);
            ns.Function($"TSharedRef<FJsonValue> IgorWriteJson(const {enumForm.ueType.QualifiedName}& Value);", enumForm);
        }

        public void GenEnumStringCode(EnumForm enumForm, UeCppFile cpp)
        {
            string fieldToString(EnumField field)
            {
                return
$@"case {field.ueRelativeQualifiedName(null)}:
    return TEXT(""{field.jsonKey}"");";
            }
            string fieldFromString(EnumField field)
            {
                return $@"if (String == TEXT(""{ field.jsonKey}""))
{{
    Result = {field.ueRelativeQualifiedName(null)};
    return true;
}}
";
            }

            var igorNs = cpp.Namespace("Igor");
            igorNs.Function(
$@"bool IgorReadJsonKey(const FString& String, {enumForm.ueType.QualifiedName}& Result)
{{
{enumForm.Fields.JoinLines(fieldFromString)}
    return false;
}}", enumForm);

            igorNs.Function(
$@"bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {enumForm.ueType.QualifiedName}& Result)
{{
    return IgorReadJsonKey(Json->AsString(), Result);
}}", enumForm);
            igorNs.Function(
$@"FString IgorWriteJsonKey(const {enumForm.ueType.QualifiedConstName}& Value)
{{
    switch (Value)
    {{
{enumForm.Fields.JoinLines(fieldToString)}
        default:
            verifyf(false, TEXT(""Unknown {enumForm.ueType.QualifiedName} value %d""), (int)Value);
            return FString();
    }}
}}", enumForm);

            igorNs.Function(
$@"TSharedRef<FJsonValue> IgorWriteJson(const {enumForm.ueType.QualifiedName}& Value)
{{
    return MakeShareable(new FJsonValueString(IgorWriteJsonKey(Value)));
}}", enumForm);
        }

        public void GenEnumNumberCode(EnumForm enumForm, UeCppFile cpp)
        {
            var igorNs = cpp.Namespace("Igor");
            igorNs.Function(
$@"bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {enumForm.ueType.QualifiedName}& Result)
{{
    Result = ({enumForm.ueType.QualifiedName})({enumForm.ueIntType})Json->AsNumber();
    return true;
}}", enumForm);

            igorNs.Function(
$@"TSharedRef<FJsonValue> IgorWriteJson(const {enumForm.ueType.QualifiedName}& Value)
{{
    return MakeShareable(new FJsonValueNumber((double)({enumForm.ueIntType})Value));
}}", enumForm);
        }

        public void GenRecordFuns(StructForm structForm, UeHFile h, UeCppFile cpp)
        {
            var hNs = h.Namespace("Igor");
            UeNamespace implNs;
            string templatePrefix = "";

            if (structForm.IsGeneric)
            {
                h.Include($"{structForm.ueIgorPath}IgorJson.h");
                implNs = hNs;
                templatePrefix = $"template<{structForm.Args.JoinStrings(", ", arg => $"typename {arg.ueName}")}>\n";
            }
            else
            {
                hNs.Function($@"TSharedRef<FJsonValue> IgorWriteJson(const {structForm.ueType.QualifiedName}& Value);", structForm);
                hNs.Function($@"bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {structForm.ueType.QualifiedName}& Result);", structForm);
                cpp.Include($"{structForm.ueIgorPath}IgorJson.h");
                var cppNs = cpp.Namespace("Igor");
                implNs = cppNs;
            }

            var accesssor = structForm.uePtr ? "->" : ".";
            var verify = structForm.uePtr ? $@"    verifyf(Value.IsValid(), TEXT(""Value of {structForm.ueName} is not set""));" : "";

            if (structForm is VariantForm vf)
            {
                string categoryCase(RecordForm descendant) =>
$@"case {descendant.TagField.ueValue(null)}:
    return IgorWriteJson(StaticCastSharedPtr<const {descendant.ueType.QualifiedBaseName}>(Value));";

                implNs.Function(
$@"TSharedRef<FJsonValue> IgorWriteJson(const {structForm.ueType.QualifiedConstName}& Value)
{{
    switch (Value->{structForm.TagField.ueTagGetter}())
    {{
{vf.Records.JoinLines(categoryCase)}
        default:
            checkNoEntry();
            return MakeShareable(new FJsonValueNull());
    }}
}}", structForm);
            }
            else
            {
                string WriteJsonField(RecordField f)
                {
                    if (structForm.jsonNulls == false && f.IsOptional)
                        return $@"if (Value{accesssor}{f.ueWriteValue}.IsSet())
{{
    IgorWriteJsonField(JsonObject, TEXT(""{f.jsonKey}""), Value{accesssor}{f.ueWriteValue});
}}";
                    else
                        return $@"IgorWriteJsonField(JsonObject, TEXT(""{f.jsonKey}""), Value{accesssor}{f.ueWriteValue});";
                }

                implNs.Function(
$@"{templatePrefix}TSharedRef<FJsonValue> IgorWriteJson(const {structForm.ueType.QualifiedConstName}& Value)
{{
{verify}
    TSharedRef<FJsonObject> JsonObject = MakeShared<FJsonObject>();
{structForm.Fields.Where(f => !f.jsonIgnore).JoinLines(WriteJsonField)}
    return MakeShareable(new FJsonValueObject(JsonObject));
}}", structForm);
            }

            if (structForm.uePtr)
            {
                if (!structForm.IsGeneric)
                    hNs.Function($@"{templatePrefix}TSharedRef<FJsonValue> IgorWriteJson(const {structForm.ueType.QualifiedConstName}& Value);", structForm);
                implNs.Function(
    $@"{templatePrefix}TSharedRef<FJsonValue> IgorWriteJson(const {structForm.ueType.QualifiedName}& Value)
{{
    {structForm.ueType.QualifiedConstName} ConstValue = StaticCastSharedPtr<const {structForm.ueType.QualifiedBaseName}>(Value);
    return IgorWriteJson(ConstValue);
}}", structForm);
            }

            if (structForm is VariantForm v)
            {
                string categoryCase(RecordForm descendant) =>
$@"case {descendant.TagField.ueValue(null)}:
{{
    {descendant.ueType.QualifiedName} {descendant.ueVarName} = MakeShared<{descendant.ueType.QualifiedBaseName}>();
    bool bSuccess = IgorReadJson(Json, {descendant.ueVarName});
    Result = {descendant.ueVarName};
    return bSuccess;
}}
break;";

                implNs.Function(
$@"bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {structForm.ueType.QualifiedName}& Result)
{{
    if (Json->Type != EJson::Object)
    {{
        return false;
    }}
    const TSharedPtr<FJsonObject>& JsonObject = Json->AsObject();
    {structForm.TagField.ueType.QualifiedName} {structForm.TagField.ueName};
    if (!IgorReadJsonField(JsonObject, TEXT(""{structForm.TagField.jsonKey}""), {structForm.TagField.ueName}))
    {{
        return false;
    }}
    switch ({structForm.TagField.ueName})
    {{
{v.Records.JoinLines(categoryCase)}
        default:
            return false;
    }}
}}", structForm);
            }
            else
            {
                var create = structForm.uePtr ? $@"    Result = MakeShared<{structForm.ueType.QualifiedBaseName}>();" : "";
                implNs.Function(
$@"{templatePrefix}bool IgorReadJson(const TSharedPtr<FJsonValue>& Json, {structForm.ueType.QualifiedName}& Result)
{{
{create}
    if (Json->Type != EJson::Object)
    {{
        return false;
    }}
    const TSharedPtr<FJsonObject>& JsonObject = Json->AsObject();
    bool bSuccess = true;
{structForm.Fields.Where(f => !f.jsonIgnore && !f.IsTag).JoinLines(f => $@"bSuccess = IgorReadJsonField(JsonObject, TEXT(""{f.jsonKey}""), Result{accesssor}{f.ueName}) && bSuccess;")}
    return bSuccess;
}}", structForm);
            }
        }
    }
}
