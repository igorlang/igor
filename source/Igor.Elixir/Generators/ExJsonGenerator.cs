using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Igor.Elixir.AST;
using Igor.Elixir.Model;
using Igor.Elixir.Render;
using Igor.Text;

namespace Igor.Elixir.Generators
{
    class ExJsonGenerator : IElixirGenerator
    {
        public void Generate(ExModel model, Module mod)
        {
            var module = model.ModuleOf(mod);
            module.Annotation = mod.Annotation;

            foreach (var form in mod.Types)
            {
                if (form.exJsonIsSerializerGenerated)
                {
                    switch (form)
                    {
                        case EnumForm enumForm:
                            GenEnum(module, enumForm);
                            break;

                        case VariantForm variantForm:
                            GenVariant(module, variantForm);
                            break;

                        case RecordForm recordForm:
                            GenRecord(module, recordForm);
                            break;

                        case UnionForm unionForm:
                            GenUnion(module, unionForm);
                            break;
                    }
                }
            }
        }

        private void GenEnum(ExModule protocolModule, EnumForm enumForm)
        {
            var ex = protocolModule.Module(enumForm.exName);

            ex.Function(
$@"@spec from_json!(String.t()) :: {enumForm.exLocalType}
{enumForm.Fields.JoinLines(field => $@"def from_json!(""{field.jsonKey}""), do: {field.exName}")}");

            ex.Function(
$@"@spec to_json!({enumForm.exLocalType}) :: String.t()
{enumForm.Fields.JoinLines(field => $@"def to_json!({field.exName}), do: ""{field.jsonKey}""")}");
        }

        private string ParseField(RecordField f)
        {
            if (f.IsOptional || f.HasDefault)
                return $@"Igor.Json.parse_field(json, ""{f.jsonKey}"", {f.exJsonTag.Tag}, {f.exDefault})";
            else
                return $@"Igor.Json.parse_field(json, ""{f.jsonKey}"", {f.exJsonTag.Tag})";
        }

        private void GenRecord(ExModule protocolModule, StructForm structForm)
        {
            var ex = protocolModule.Module(structForm.exName);
            var structType = structForm.exStructType;

            var specArgs = structForm.IsGeneric ? $", {{{structForm.Args.JoinStrings(", ", arg => "Igor.Json.type()")}}}" : "";
            var commaArgs = structForm.IsGeneric ? $", {{{structForm.Args.JoinStrings(", ", arg => $"{arg.exTypeTagVarName}")}}}" : "";
            {
                var serFields = structForm.jsonSerializedFields.Where(f => !f.IsTag);

                var r = ExRenderer.Create();
                r += $@"@spec from_json!(Igor.Json.json(){specArgs}) :: {structForm.exLocalType}";
                if (serFields.Any())
                {
                    r += $"def from_json!(json{commaArgs}) do";
                    r++;
                    if (structForm.IsPatch)
                    {
                        r += "%{}";
                        foreach (var field in serFields)
                        {
                            r += $@"|> field_from_json(json, ""{field.jsonKey}"", {field.exJsonTag.Tag}, {field.exAtomName})";
                        }
                    }
                    else
                    {
                        r.Blocks(serFields, field => $"{field.exVarName} = {ParseField(field)}");
                        CodeUtils.ReturnStruct(r, structType, serFields, 3);
                    }
                    r--;
                    r += "end";
                }
                else
                {
                    r += $@"def from_json!(%{{}}{commaArgs}), do: {structType.Empty}";
                }
                ex.Function(r.Build());

                if (structForm.IsPatch && serFields.Any())
                {
                    ex.Function(
$@"defp field_from_json(map, json, json_key, type, map_key) do
  case Map.fetch(json, json_key) do
    {{:ok, value}} -> Map.put(map, map_key, Igor.Json.parse_value(value, type))
    :error -> map
  end
end
");
                }
            }

            {
                var serFields = structForm.jsonSerializedFields;

                var r = ExRenderer.Create();
                r += $@"@spec to_json!({structForm.exLocalType}{specArgs}) :: Igor.Json.json()";

                string PackField(RecordField field)
                {
                    if (field.IsTag)
                        return $@"""{field.jsonKey}"" => {field.exJsonTag.PackJson(field.exDefault)}";
                    else
                        return $@"""{field.jsonKey}"" => {field.exJsonTag.PackJson(field.exVarName)}";
                }

                if (serFields.Any(f => !f.IsTag))
                {
                    r += $"def to_json!(args{commaArgs}) do";
                    r++;
                    if (structForm.IsPatch)
                    {
                        r += "%{}";
                        r.Blocks(serFields.Select(f => $@"|> field_to_json(args, {f.exAtomName}, {f.exJsonTag.Tag}, ""{f.jsonKey}"")"));
                    }
                    else if (!serFields.Any(f => f.IsOptional) || (structForm.jsonNulls ?? false))
                    {
                        CodeUtils.DeconstructStruct(r, structType, serFields.Where(f => !f.IsTag), "args");
                        r += "%{";
                        r++;
                        r.Blocks(serFields.Select(PackField), delimiter: ",");
                        r--;
                        r += "}";
                    }
                    else
                    {
                        CodeUtils.DeconstructStruct(r, structType, serFields.Where(f => !f.IsTag), "args");
                        r += "%{}";
                        foreach (var field in serFields)
                        {
                            var value = field.IsTag ? field.exDefault : field.exVarName;
                            r += $@"|> Igor.Json.pack_field(""{field.jsonKey}"", {value}, {field.exJsonTag.Tag})";
                        }
                    }
                    r--;
                    r += "end";
                }
                else if (serFields.Any())
                {
                    r += $"def to_json!({structType.Empty}{commaArgs}) do";
                    r++;
                    r += $"%{{{PackField(serFields.First())}}}";
                    r--;
                    r += "end";
                }
                else
                {
                    r += $"def to_json!({structType.Empty}{commaArgs}), do: %{{}}";
                }
                ex.Function(r.Build());

                if (structForm.IsPatch && serFields.Any())
                {
                    ex.Function(
$@"defp field_to_json(json, map, map_key, type, json_key) do
  case Map.fetch(map, map_key) do
    {{:ok, value}} -> Map.put(json, json_key, Igor.Json.pack_value(value, type))
    :error -> json
  end
end
");
                }
            }
        }

        private void GenVariant(ExModule protocolModule, VariantForm variantForm)
        {
            var ex = protocolModule.Module(variantForm.exName);
            {
                string erlJsonParseVariantClause(RecordForm recordForm) =>
    $@"    {recordForm.TagField.exDefault} -> {JsonSerialization.ParseJson(recordForm.exJsonTag(variantForm), "json", variantForm.Module.exName)}";

                ex.Function(
    $@"@spec from_json!(Igor.Json.json()) :: {variantForm.exLocalType}
def from_json!(json) do
  tag = {ParseField(variantForm.TagField)}
  case tag do
{variantForm.Records.JoinLines(erlJsonParseVariantClause)}
  end
end");
            }
            {
                string erlJsonPackVariantClause(RecordForm recordForm) =>
    $@"def to_json!(struct) when {recordForm.exGuard("struct")} do
  {recordForm.exJsonTag(variantForm).PackJson("struct", variantForm.Module.exName)}
end";

                ex.Function(
  $@"@spec to_json!({variantForm.exLocalType}) :: Igor.Json.json()
{variantForm.Records.JoinLines(erlJsonPackVariantClause)}");
            }
        }

        private void GenUnion(ExModule protocolModule, UnionForm unionForm)
        {
            var ex = protocolModule.Module(unionForm.exName);
            var specArgs = unionForm.IsGeneric ? $", {{{unionForm.Args.JoinStrings(", ", arg => "Igor.Json.type()")}}}" : "";
            var commaArgs = unionForm.IsGeneric ? $", {{{unionForm.Args.JoinStrings(", ", arg => $"{arg.exTypeTagVarName}")}}}" : "";
            
            {
                string exJsonParseClause(UnionClause clause)
                {
                    if (clause.IsSingleton)
                        return $@"def from_json!(""{clause.jsonKey}""{commaArgs}) do
  {clause.exTag}
end";
                    else if (clause.exTagged)
                        return $@"def from_json!(%{{""{clause.jsonKey}"" => json}}{commaArgs}) do
  {{{clause.exTag}, {JsonSerialization.ParseJson(JsonSerialization.JsonTag(clause.Type, unionForm), "json", ex.Name)}}}
end";
                    else
                        return $@"def from_json!(%{{""{clause.jsonKey}"" => json}}{commaArgs}) do
  {JsonSerialization.ParseJson(JsonSerialization.JsonTag(clause.Type, unionForm), "json", ex.Name)}
end";
                }

                ex.Function(
    $@"@spec from_json!(Igor.Json.json(){specArgs}) :: {unionForm.exLocalType}
{unionForm.Clauses.JoinLines(exJsonParseClause)}");
            }
            {
                string exJsonPackClause(UnionClause clause)
                {
                    if (clause.IsSingleton)
                        return $@"def to_json!({clause.exTag}{commaArgs}) do
  ""{clause.jsonKey}""
end";
                    else if (clause.exTagged)
                        return $@"def to_json!({{{clause.exTag}, value}}{commaArgs}) do
  %{{""{clause.jsonKey}"" => {clause.exJsonTag.PackJson("value", unionForm.Module.exName)}}}
end";
                    else
                        return $@"def to_json!(value) when {clause.exGuard("value")} do
  %{{""{clause.jsonKey}"" => {clause.exJsonTag.PackJson("value", unionForm.Module.exName)}}}
end";
                }

                ex.Function(
  $@"@spec to_json!({unionForm.exLocalType}{commaArgs}) :: Igor.Json.json()
{unionForm.Clauses.JoinLines(exJsonPackClause)}");
            }

        }
    }
}
