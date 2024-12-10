using Igor.Python.AST;
using Igor.Python.Json;
using Igor.Python.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Python
{
    internal class PythonJsonGenerator : IPythonGenerator
    {
        public void Generate(PythonModel model, Module mod)
        {
            var py = model.FileOf(mod);

            foreach (var type in mod.Types)
            {
                if (type.pyJsonGenerateSerializer)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumSerializer(py, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenRecordSerializer(py, recordForm);
                            break;

                        case VariantForm variantForm:
                            GenVariantSerializer(py, variantForm);
                            break;

                        case DefineForm defineForm:
                            break;
                    }
                }
            }
        }

        private void GenEnumSerializer(PythonFile py, EnumForm enumForm)
        {
            /*
            var from_json = $"{enumForm.pyName.Format(Notation.LowerUnderscore)}_from_json";
            var to_json = $"{enumForm.pyName.Format(Notation.LowerUnderscore)}_to_json";
            py.Declare($@"local {from_json} = {{ {enumForm.Fields.JoinStrings(", ", f => $@"[""{f.jsonKey}""] = {enumForm.pyName}.{f.pyName}")} }}");
            py.Declare($@"local {to_json} = {{ {enumForm.Fields.JoinStrings(", ", f => $@"[{enumForm.pyName}.{f.pyName}] = ""{f.jsonKey}""")} }}");

            py.Declare($@"
function {enumForm.pyName}.from_json(json)
    local result = {from_json}[json]
    assert(result)
    return result
end
");
            py.Declare($@"
function {enumForm.pyName}.to_json(value)
    local result = {to_json}[value]
    assert(result)
    return result
end
");*/
        }

        private void GenRecordSerializer(PythonFile py, StructForm structForm)
        {
            var cl = py.Class(structForm.pyName);

            cl.Function($@"
@classmethod
def from_json(cls, json):
    result = {structForm.pyName}()
{structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinLines(f => $"    result.{f.pyFieldName} = {JsonSerialization.Parse(f.pyJsonTag, $"json['{f.jsonKey}']")}")}
    return result
");

            cl.Function($@"
@classmethod
def to_json(cls, obj):
    return igor.json_object({{
{structForm.jsonSerializedFields.JoinStrings(",\n", f => $"        ['{f.jsonKey}'] = {JsonSerialization.Pack(f.pyJsonTag, $"self.{f.pyFieldName}")}")}
    }})
");
        }

        private void GenVariantSerializer(PythonFile py, VariantForm structForm)
        {
            /*
            var cl = py.Class(structForm.pyName);
            var tag = structForm.TagField;

            string ParseVariantClause(RecordForm r)
            {
                return $@"if {tag.pyName} == {r.TagField.pyDefault} then
    return {r.pyName}._from_json(json)
end";
            }

            cl.Function($@"
function {structForm.pyName}.from_json(json)
    local {tag.pyName} = {JsonSerialization.Parse(tag.pyJsonTag, $"json['{tag.jsonKey}']")}
{structForm.Records.JoinLines(ParseVariantClause).Indent(4)}
    ferror(""Unknown {structForm.pyName}.{tag.pyName}: %s"", {tag.pyName})
end
");
            string PackVariantClause(RecordForm r)
            {
                return $@"if {tag.pyName} == {r.TagField.pyDefault} then
    return {r.pyName}.to_json(value)
end";
            }

            cl.Function($@"
function {structForm.pyName}.to_json(value)
    local {tag.pyName} = value{tag.pyIndexer}
{structForm.Records.JoinLines(PackVariantClause).Indent(4)}
    ferror(""Unknown {structForm.pyName}.{tag.pyName}: %s"", {tag.pyName})
end
");*/
        }
    }
}
