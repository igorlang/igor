using Igor.Lua.AST;
using Igor.Lua.Json;
using Igor.Lua.Model;
using Igor.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Igor.Lua
{
    internal class LuaJsonGenerator : ILuaGenerator
    {
        public void Generate(LuaModel model, Module mod)
        {
            var lua = model.FileOf(mod);

            foreach (var type in mod.Types)
            {
                if (type.luaJsonGenerateSerializer)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            if (enumForm.luaEnumAlias != null)
                                GenEnumAliasSerializer(lua, enumForm);
                            else
                                GenEnumSerializer(lua, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenRecordSerializer(lua, recordForm);
                            break;

                        case VariantForm variantForm:
                            GenVariantSerializer(lua, variantForm);
                            break;

                        case DefineForm defineForm:
                            if (defineForm.luaEnumAlias != null)
                                GenEnumAliasSerializer(lua, defineForm);
                            break;
                    }
                }
            }
        }

        private void GenEnumSerializer(LuaFile lua, EnumForm enumForm)
        {
            var from_json = $"{enumForm.luaName.Format(Notation.LowerUnderscore)}_from_json";
            var to_json = $"{enumForm.luaName.Format(Notation.LowerUnderscore)}_to_json";
            lua.Declare($@"local {from_json} = {{ {enumForm.Fields.JoinStrings(", ", f => $@"[""{f.jsonKey}""] = {enumForm.luaName}.{f.luaName}")} }}");
            lua.Declare($@"local {to_json} = {{ {enumForm.Fields.JoinStrings(", ", f => $@"[{enumForm.luaName}.{f.luaName}] = ""{f.jsonKey}""")} }}");

            lua.Declare($@"
function {enumForm.luaName}.from_json(json)
    local result = {from_json}[json]
    assert(result)
    return result
end
");
            lua.Declare($@"
function {enumForm.luaName}.to_json(value)
    local result = {to_json}[value]
    assert(result)
    return result
end
");
        }

        private void GenEnumAliasSerializer(LuaFile lua, TypeForm enumForm)
        {
            var ns = string.IsNullOrEmpty(enumForm.luaNamespace) ? "" : $"{enumForm.luaNamespace}.";
            lua.Declare($"{ns}{enumForm.luaName} = igor.enum(igor.reversed_enum_table({enumForm.luaEnumAlias}), {enumForm.luaEnumAlias})");
        }

        private void GenRecordSerializer(LuaFile lua, StructForm structForm)
        {
            var cl = lua.Class(structForm.luaName);

            var fromJson = structForm.Ancestor == null ? "from_json" : "_from_json";

            string DeserializeField(RecordField f)
            {
                if (f.HasDefault)
                    return $"json['{f.jsonKey}'] ~= nil and {JsonSerialization.Parse(f.luaJsonTag, $"json['{f.jsonKey}']")} or {f.luaDefault}";
                else
                    return JsonSerialization.Parse(f.luaJsonTag, $"json['{f.jsonKey}']");
            }

            if (structForm.luaRecordStyle == RecordStyle.Class)
            {
                cl.Function($@"
function {structForm.luaName}.{fromJson}(json)
    local result = {structForm.luaName}:new()
{structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinLines(f => $"    result{f.luaIndexer} = {DeserializeField(f)}")}
    return result
end
");

                cl.Function($@"
function {structForm.luaName}:to_json()
{structForm.jsonSerializedFields.Where(f => !f.IsOptional && !f.IsTag).JoinLines(f => $"    assert(self{f.luaIndexer} ~= nil)")}
    return igor.json_object({{
{structForm.jsonSerializedFields.JoinStrings(",\n", f => $"        ['{f.jsonKey}'] = {JsonSerialization.Pack(f.luaJsonTag, $"self{f.luaIndexer}")}")}
    }})
end
");
            }
            else
            {
                var r = new Renderer();
                cl.Function($@"
function {structForm.luaName}.{fromJson}(json)
    return {{
{structForm.jsonSerializedFields.Where(f => !f.IsTag).JoinStrings(",\n", f => $"        {f.luaConstructionKey} = {DeserializeField(f)}")}
    }}
end
");

                cl.Function($@"
function {structForm.luaName}.to_json(data)
{structForm.jsonSerializedFields.Where(f => !f.IsOptional && !f.IsTag).JoinLines(f => $"    assert(data{f.luaIndexer} ~= nil)")}
    return igor.json_object({{
{structForm.jsonSerializedFields.JoinStrings(",\n", f => $"        ['{f.jsonKey}'] = {JsonSerialization.Pack(f.luaJsonTag, $"data{f.luaIndexer}")}")}
    }})
end
");
            }
        }

        private void GenVariantSerializer(LuaFile lua, VariantForm structForm)
        {
            var cl = lua.Class(structForm.luaName);
            var tag = structForm.TagField;

            string ParseVariantClause(RecordForm r)
            {
                return $@"if {tag.luaName} == {r.TagField.luaDefault} then
    return {r.luaName}._from_json(json)
end";
            }

            if (structForm.luaVariantSerializerLookup)
            {
                cl.Function($@"
function {structForm.luaName}.from_json(json)
    local {tag.luaName} = {JsonSerialization.Parse(tag.luaJsonTag, $"json['{tag.jsonKey}']")}
    local from_json = {structForm.luaJsonDeserializerLookupTable}[{tag.luaName}]
    if not from_json then
        ferror(""Unknown {structForm.luaName}.{tag.luaName}: %s"", {tag.luaName})
        return
    end
    return from_json(json)
end
");

                lua.Declare($@"
{structForm.luaJsonDeserializerLookupTable} = {{
{structForm.Records.OrderBy(rec => ((Value.Enum)rec.TagField.Default).Field.Value).JoinStrings(",\n", rec => $"    [{rec.TagField.luaDefaultRelative(lua.FileName)}] = {rec.luaName}._from_json")}
}}
");
            }
            else
            {
                cl.Function($@"
function {structForm.luaName}.from_json(json)
    local {tag.luaName} = {JsonSerialization.Parse(tag.luaJsonTag, $"json['{tag.jsonKey}']")}
{structForm.Records.JoinLines(ParseVariantClause).Indent(4)}
    ferror(""Unknown {structForm.luaName}.{tag.luaName}: %s"", {tag.luaName})
end
");
            }

            string PackVariantClause(RecordForm r)
            {
                return $@"if {tag.luaName} == {r.TagField.luaDefault} then
    return {r.luaName}.to_json(value)
end";
            }

            if (structForm.luaVariantSerializerLookup)
            {
                cl.Function($@"
function {structForm.luaName}.to_json(value)
    local {tag.luaName} = value{tag.luaIndexer}
    local to_json = {structForm.luaJsonSerializerLookupTable}[{tag.luaName}]
    if not to_json then
        ferror(""Unknown {structForm.luaName}.{tag.luaName}: %s"", {tag.luaName})
        return
    end
    return to_json(value)
end
");

                lua.Declare($@"
{structForm.luaJsonSerializerLookupTable} = {{
{structForm.Records.OrderBy(rec => ((Value.Enum)rec.TagField.Default).Field.Value).JoinStrings(",\n", rec => $"    [{rec.TagField.luaDefaultRelative(lua.FileName)}] = {rec.luaName}.to_json")}
}}
");
            }
            else
            {
                cl.Function($@"
function {structForm.luaName}.to_json(value)
    local {tag.luaName} = value{tag.luaIndexer}
{structForm.Records.JoinLines(PackVariantClause).Indent(4)}
    ferror(""Unknown {structForm.luaName}.{tag.luaName}: %s"", {tag.luaName})
end
");
            }
        }
    }
}
