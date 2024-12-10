// Author: Igor compiler
// Compiler version: igorc 2.1.4
// DO NOT EDIT THIS FILE - it is machine generated

using System.Collections.Generic;

using JsonSerializer = Json.Serialization.JsonSerializer;

namespace Igor.JsonSchema
{
    public enum SimpleType
    {
        Array = 1,
        Boolean = 2,
        Integer = 3,
        Null = 4,
        Number = 5,
        Object = 6,
        String = 7,
    }

    public sealed class SchemaObject
    {
        public string Id { get; set; }

        public string Schema { get; set; }

        public string Ref { get; set; }

        public string Anchor { get; set; }

        public string DynamicRef { get; set; }

        public string DynamicAnchor { get; set; }

        public Dictionary<string, bool> Vocabulary { get; set; }

        public string Comment { get; set; }

        public Dictionary<string, SchemaObject> Defs { get; set; }

        public Dictionary<string, SchemaObject> Definitions { get; set; }

        public List<SchemaObject> PrefixItems { get; set; }

        public SchemaObject Items { get; set; }

        public SchemaObject Contains { get; set; }

        public OneOf.OneOf<SchemaObject, bool>? AdditionalProperties { get; set; }

        public Dictionary<string, SchemaObject> Properties { get; set; }

        public Dictionary<string, SchemaObject> PatternProperties { get; set; }

        public Dictionary<string, SchemaObject> DependentSchemas { get; set; }

        public SchemaObject PropertyNames { get; set; }

        public SchemaObject If { get; set; }

        public SchemaObject Then { get; set; }

        public SchemaObject Else { get; set; }

        public List<SchemaObject> AllOf { get; set; }

        public List<SchemaObject> AnyOf { get; set; }

        public List<SchemaObject> OneOf { get; set; }

        public SchemaObject Not { get; set; }

        public OneOf.OneOf<SimpleType, List<SimpleType>>? Type { get; set; }

        public Json.ImmutableJson Const { get; set; }

        public List<string> Enum { get; set; }

        public double? MultipleOf { get; set; }

        public double? Maximum { get; set; }

        public double? ExclusiveMaximum { get; set; }

        public double? Minimum { get; set; }

        public double? ExclusiveMinimum { get; set; }

        public uint? MaxLength { get; set; }

        public uint? MinLength { get; set; }

        public string Pattern { get; set; }

        public uint? MaxItems { get; set; }

        public uint? MinItems { get; set; }

        public bool? UniqueItems { get; set; }

        public uint? MaxContains { get; set; }

        public uint? MinContains { get; set; }

        public uint? MaxProperties { get; set; }

        public uint? MinProperties { get; set; }

        public List<string> Required { get; set; }

        public Dictionary<string, List<string>> DependentRequired { get; set; }

        public SchemaObject UnevaluatedItems { get; set; }

        public SchemaObject UnevaluatedProperties { get; set; }

        public string Format { get; set; }

        public string ContentEncoding { get; set; }

        public string ContentMediaType { get; set; }

        public SchemaObject ContentSchema { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Json.ImmutableJson Default { get; set; }

        public bool? Deprecated { get; set; }

        public bool? ReadOnly { get; set; }

        public bool? WriteOnly { get; set; }

        public List<Json.ImmutableJson> Examples { get; set; }
    }

    public sealed class SimpleTypeJsonSerializer : Json.Serialization.IJsonSerializer<SimpleType>, Json.Serialization.IJsonKeySerializer<SimpleType>
    {
        public static readonly SimpleTypeJsonSerializer Instance = new SimpleTypeJsonSerializer();

        public Json.ImmutableJson Serialize(SimpleType value)
        {
            return SerializeKey(value);
        }

        public SimpleType Deserialize(Json.ImmutableJson json)
        {
            if (json == null)
                throw new System.ArgumentNullException(nameof(json));
            return DeserializeKey(json.AsString);
        }

        public bool Test(Json.ImmutableJson json)
        {
            if (json == null)
                throw new System.ArgumentNullException(nameof(json));
            return json.IsString;
        }

        public string SerializeKey(SimpleType value)
        {
            switch (value)
            {
                case SimpleType.Array: return "array";
                case SimpleType.Boolean: return "boolean";
                case SimpleType.Integer: return "integer";
                case SimpleType.Null: return "null";
                case SimpleType.Number: return "number";
                case SimpleType.Object: return "object";
                case SimpleType.String: return "string";
                default: throw new System.ArgumentOutOfRangeException(nameof(value));
            }
        }

        public SimpleType DeserializeKey(string jsonKey)
        {
            switch (jsonKey)
            {
                case "array": return SimpleType.Array;
                case "boolean": return SimpleType.Boolean;
                case "integer": return SimpleType.Integer;
                case "null": return SimpleType.Null;
                case "number": return SimpleType.Number;
                case "object": return SimpleType.Object;
                case "string": return SimpleType.String;
                default: throw new System.ArgumentOutOfRangeException(nameof(jsonKey));
            }
        }
    }

    public sealed class SchemaObjectJsonSerializer : Json.Serialization.IJsonSerializer<SchemaObject>
    {
        public static readonly SchemaObjectJsonSerializer Instance = new SchemaObjectJsonSerializer();

        public Json.ImmutableJson Serialize(SchemaObject value)
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));

            var json = new Json.JsonObject();
            if (value.Id != null)
                json["$id"] = JsonSerializer.String.Serialize(value.Id);
            if (value.Schema != null)
                json["$schema"] = JsonSerializer.String.Serialize(value.Schema);
            if (value.Ref != null)
                json["$ref"] = JsonSerializer.String.Serialize(value.Ref);
            if (value.Anchor != null)
                json["$anchor"] = JsonSerializer.String.Serialize(value.Anchor);
            if (value.DynamicRef != null)
                json["$dynamicRef"] = JsonSerializer.String.Serialize(value.DynamicRef);
            if (value.DynamicAnchor != null)
                json["$dynamicAnchor"] = JsonSerializer.String.Serialize(value.DynamicAnchor);
            if (value.Vocabulary != null)
                json["$vocabulary"] = JsonSerializer.Dict(JsonSerializer.String, JsonSerializer.Bool).Serialize(value.Vocabulary);
            if (value.Comment != null)
                json["$comment"] = JsonSerializer.String.Serialize(value.Comment);
            if (value.Defs != null)
                json["$defs"] = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Serialize(value.Defs);
            if (value.Definitions != null)
                json["definitions"] = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Serialize(value.Definitions);
            if (value.PrefixItems != null)
                json["prefixItems"] = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Serialize(value.PrefixItems);
            if (value.Items != null)
                json["items"] = SchemaObjectJsonSerializer.Instance.Serialize(value.Items);
            if (value.Contains != null)
                json["contains"] = SchemaObjectJsonSerializer.Instance.Serialize(value.Contains);
            if (value.AdditionalProperties.HasValue)
                json["additionalProperties"] = Json.OneOfJsonSerializer.Instance(SchemaObjectJsonSerializer.Instance, JsonSerializer.Bool).Serialize(value.AdditionalProperties.Value);
            if (value.Properties != null)
                json["properties"] = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Serialize(value.Properties);
            if (value.PatternProperties != null)
                json["patternProperties"] = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Serialize(value.PatternProperties);
            if (value.DependentSchemas != null)
                json["dependentSchemas"] = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Serialize(value.DependentSchemas);
            if (value.PropertyNames != null)
                json["propertyNames"] = SchemaObjectJsonSerializer.Instance.Serialize(value.PropertyNames);
            if (value.If != null)
                json["if"] = SchemaObjectJsonSerializer.Instance.Serialize(value.If);
            if (value.Then != null)
                json["then"] = SchemaObjectJsonSerializer.Instance.Serialize(value.Then);
            if (value.Else != null)
                json["else"] = SchemaObjectJsonSerializer.Instance.Serialize(value.Else);
            if (value.AllOf != null)
                json["allOf"] = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Serialize(value.AllOf);
            if (value.AnyOf != null)
                json["anyOf"] = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Serialize(value.AnyOf);
            if (value.OneOf != null)
                json["oneOf"] = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Serialize(value.OneOf);
            if (value.Not != null)
                json["not"] = SchemaObjectJsonSerializer.Instance.Serialize(value.Not);
            if (value.Type.HasValue)
                json["type"] = Json.OneOfJsonSerializer.Instance(SimpleTypeJsonSerializer.Instance, JsonSerializer.List(SimpleTypeJsonSerializer.Instance)).Serialize(value.Type.Value);
            if (value.Const != null)
                json["const"] = JsonSerializer.Json.Serialize(value.Const);
            if (value.Enum != null)
                json["enum"] = JsonSerializer.List(JsonSerializer.String).Serialize(value.Enum);
            if (value.MultipleOf.HasValue)
                json["multipleOf"] = JsonSerializer.Double.Serialize(value.MultipleOf.Value);
            if (value.Maximum.HasValue)
                json["maximum"] = JsonSerializer.Double.Serialize(value.Maximum.Value);
            if (value.ExclusiveMaximum.HasValue)
                json["exclusiveMaximum"] = JsonSerializer.Double.Serialize(value.ExclusiveMaximum.Value);
            if (value.Minimum.HasValue)
                json["minimum"] = JsonSerializer.Double.Serialize(value.Minimum.Value);
            if (value.ExclusiveMinimum.HasValue)
                json["exclusiveMinimum"] = JsonSerializer.Double.Serialize(value.ExclusiveMinimum.Value);
            if (value.MaxLength.HasValue)
                json["maxLength"] = JsonSerializer.UInt.Serialize(value.MaxLength.Value);
            if (value.MinLength.HasValue)
                json["minLength"] = JsonSerializer.UInt.Serialize(value.MinLength.Value);
            if (value.Pattern != null)
                json["pattern"] = JsonSerializer.String.Serialize(value.Pattern);
            if (value.MaxItems.HasValue)
                json["maxItems"] = JsonSerializer.UInt.Serialize(value.MaxItems.Value);
            if (value.MinItems.HasValue)
                json["minItems"] = JsonSerializer.UInt.Serialize(value.MinItems.Value);
            if (value.UniqueItems.HasValue)
                json["uniqueItems"] = JsonSerializer.Bool.Serialize(value.UniqueItems.Value);
            if (value.MaxContains.HasValue)
                json["maxContains"] = JsonSerializer.UInt.Serialize(value.MaxContains.Value);
            if (value.MinContains.HasValue)
                json["minContains"] = JsonSerializer.UInt.Serialize(value.MinContains.Value);
            if (value.MaxProperties.HasValue)
                json["maxProperties"] = JsonSerializer.UInt.Serialize(value.MaxProperties.Value);
            if (value.MinProperties.HasValue)
                json["minProperties"] = JsonSerializer.UInt.Serialize(value.MinProperties.Value);
            if (value.Required != null)
                json["required"] = JsonSerializer.List(JsonSerializer.String).Serialize(value.Required);
            if (value.DependentRequired != null)
                json["dependentRequired"] = JsonSerializer.Dict(JsonSerializer.String, JsonSerializer.List(JsonSerializer.String)).Serialize(value.DependentRequired);
            if (value.UnevaluatedItems != null)
                json["unevaluatedItems"] = SchemaObjectJsonSerializer.Instance.Serialize(value.UnevaluatedItems);
            if (value.UnevaluatedProperties != null)
                json["unevaluatedProperties"] = SchemaObjectJsonSerializer.Instance.Serialize(value.UnevaluatedProperties);
            if (value.Format != null)
                json["format"] = JsonSerializer.String.Serialize(value.Format);
            if (value.ContentEncoding != null)
                json["contentEncoding"] = JsonSerializer.String.Serialize(value.ContentEncoding);
            if (value.ContentMediaType != null)
                json["contentMediaType"] = JsonSerializer.String.Serialize(value.ContentMediaType);
            if (value.ContentSchema != null)
                json["contentSchema"] = SchemaObjectJsonSerializer.Instance.Serialize(value.ContentSchema);
            if (value.Title != null)
                json["title"] = JsonSerializer.String.Serialize(value.Title);
            if (value.Description != null)
                json["description"] = JsonSerializer.String.Serialize(value.Description);
            if (value.Default != null)
                json["default"] = JsonSerializer.Json.Serialize(value.Default);
            if (value.Deprecated.HasValue)
                json["deprecated"] = JsonSerializer.Bool.Serialize(value.Deprecated.Value);
            if (value.ReadOnly.HasValue)
                json["readOnly"] = JsonSerializer.Bool.Serialize(value.ReadOnly.Value);
            if (value.WriteOnly.HasValue)
                json["writeOnly"] = JsonSerializer.Bool.Serialize(value.WriteOnly.Value);
            if (value.Examples != null)
                json["examples"] = JsonSerializer.List(JsonSerializer.Json).Serialize(value.Examples);
            return json;
        }

        public SchemaObject Deserialize(Json.ImmutableJson json)
        {
            if (json == null)
                throw new System.ArgumentNullException(nameof(json));
            var result = new SchemaObject();
            Deserialize(json, result);
            return result;
        }

        public void Deserialize(Json.ImmutableJson json, SchemaObject value)
        {
            if (json == null)
                throw new System.ArgumentNullException(nameof(json));
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));
            if (json.AsObject.TryGetValue("$id", out var jsonId) && !jsonId.IsNull)
                value.Id = JsonSerializer.String.Deserialize(jsonId);
            if (json.AsObject.TryGetValue("$schema", out var jsonSchema) && !jsonSchema.IsNull)
                value.Schema = JsonSerializer.String.Deserialize(jsonSchema);
            if (json.AsObject.TryGetValue("$ref", out var jsonRef) && !jsonRef.IsNull)
                value.Ref = JsonSerializer.String.Deserialize(jsonRef);
            if (json.AsObject.TryGetValue("$anchor", out var jsonAnchor) && !jsonAnchor.IsNull)
                value.Anchor = JsonSerializer.String.Deserialize(jsonAnchor);
            if (json.AsObject.TryGetValue("$dynamicRef", out var jsonDynamicRef) && !jsonDynamicRef.IsNull)
                value.DynamicRef = JsonSerializer.String.Deserialize(jsonDynamicRef);
            if (json.AsObject.TryGetValue("$dynamicAnchor", out var jsonDynamicAnchor) && !jsonDynamicAnchor.IsNull)
                value.DynamicAnchor = JsonSerializer.String.Deserialize(jsonDynamicAnchor);
            if (json.AsObject.TryGetValue("$vocabulary", out var jsonVocabulary) && !jsonVocabulary.IsNull)
                value.Vocabulary = JsonSerializer.Dict(JsonSerializer.String, JsonSerializer.Bool).Deserialize(jsonVocabulary);
            if (json.AsObject.TryGetValue("$comment", out var jsonComment) && !jsonComment.IsNull)
                value.Comment = JsonSerializer.String.Deserialize(jsonComment);
            if (json.AsObject.TryGetValue("$defs", out var jsonDefs) && !jsonDefs.IsNull)
                value.Defs = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Deserialize(jsonDefs);
            if (json.AsObject.TryGetValue("definitions", out var jsonDefinitions) && !jsonDefinitions.IsNull)
                value.Definitions = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Deserialize(jsonDefinitions);
            if (json.AsObject.TryGetValue("prefixItems", out var jsonPrefixItems) && !jsonPrefixItems.IsNull)
                value.PrefixItems = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Deserialize(jsonPrefixItems);
            if (json.AsObject.TryGetValue("items", out var jsonItems) && !jsonItems.IsNull)
                value.Items = SchemaObjectJsonSerializer.Instance.Deserialize(jsonItems);
            if (json.AsObject.TryGetValue("contains", out var jsonContains) && !jsonContains.IsNull)
                value.Contains = SchemaObjectJsonSerializer.Instance.Deserialize(jsonContains);
            if (json.AsObject.TryGetValue("additionalProperties", out var jsonAdditionalProperties) && !jsonAdditionalProperties.IsNull)
                value.AdditionalProperties = Json.OneOfJsonSerializer.Instance(SchemaObjectJsonSerializer.Instance, JsonSerializer.Bool).Deserialize(jsonAdditionalProperties);
            if (json.AsObject.TryGetValue("properties", out var jsonProperties) && !jsonProperties.IsNull)
                value.Properties = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Deserialize(jsonProperties);
            if (json.AsObject.TryGetValue("patternProperties", out var jsonPatternProperties) && !jsonPatternProperties.IsNull)
                value.PatternProperties = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Deserialize(jsonPatternProperties);
            if (json.AsObject.TryGetValue("dependentSchemas", out var jsonDependentSchemas) && !jsonDependentSchemas.IsNull)
                value.DependentSchemas = JsonSerializer.Dict(JsonSerializer.String, SchemaObjectJsonSerializer.Instance).Deserialize(jsonDependentSchemas);
            if (json.AsObject.TryGetValue("propertyNames", out var jsonPropertyNames) && !jsonPropertyNames.IsNull)
                value.PropertyNames = SchemaObjectJsonSerializer.Instance.Deserialize(jsonPropertyNames);
            if (json.AsObject.TryGetValue("if", out var jsonIf) && !jsonIf.IsNull)
                value.If = SchemaObjectJsonSerializer.Instance.Deserialize(jsonIf);
            if (json.AsObject.TryGetValue("then", out var jsonThen) && !jsonThen.IsNull)
                value.Then = SchemaObjectJsonSerializer.Instance.Deserialize(jsonThen);
            if (json.AsObject.TryGetValue("else", out var jsonElse) && !jsonElse.IsNull)
                value.Else = SchemaObjectJsonSerializer.Instance.Deserialize(jsonElse);
            if (json.AsObject.TryGetValue("allOf", out var jsonAllOf) && !jsonAllOf.IsNull)
                value.AllOf = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Deserialize(jsonAllOf);
            if (json.AsObject.TryGetValue("anyOf", out var jsonAnyOf) && !jsonAnyOf.IsNull)
                value.AnyOf = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Deserialize(jsonAnyOf);
            if (json.AsObject.TryGetValue("oneOf", out var jsonOneOf) && !jsonOneOf.IsNull)
                value.OneOf = JsonSerializer.List(SchemaObjectJsonSerializer.Instance).Deserialize(jsonOneOf);
            if (json.AsObject.TryGetValue("not", out var jsonNot) && !jsonNot.IsNull)
                value.Not = SchemaObjectJsonSerializer.Instance.Deserialize(jsonNot);
            if (json.AsObject.TryGetValue("type", out var jsonType) && !jsonType.IsNull)
                value.Type = Json.OneOfJsonSerializer.Instance(SimpleTypeJsonSerializer.Instance, JsonSerializer.List(SimpleTypeJsonSerializer.Instance)).Deserialize(jsonType);
            if (json.AsObject.TryGetValue("const", out var jsonConst) && !jsonConst.IsNull)
                value.Const = JsonSerializer.Json.Deserialize(jsonConst);
            if (json.AsObject.TryGetValue("enum", out var jsonEnum) && !jsonEnum.IsNull)
                value.Enum = JsonSerializer.List(JsonSerializer.String).Deserialize(jsonEnum);
            if (json.AsObject.TryGetValue("multipleOf", out var jsonMultipleOf) && !jsonMultipleOf.IsNull)
                value.MultipleOf = JsonSerializer.Double.Deserialize(jsonMultipleOf);
            if (json.AsObject.TryGetValue("maximum", out var jsonMaximum) && !jsonMaximum.IsNull)
                value.Maximum = JsonSerializer.Double.Deserialize(jsonMaximum);
            if (json.AsObject.TryGetValue("exclusiveMaximum", out var jsonExclusiveMaximum) && !jsonExclusiveMaximum.IsNull)
                value.ExclusiveMaximum = JsonSerializer.Double.Deserialize(jsonExclusiveMaximum);
            if (json.AsObject.TryGetValue("minimum", out var jsonMinimum) && !jsonMinimum.IsNull)
                value.Minimum = JsonSerializer.Double.Deserialize(jsonMinimum);
            if (json.AsObject.TryGetValue("exclusiveMinimum", out var jsonExclusiveMinimum) && !jsonExclusiveMinimum.IsNull)
                value.ExclusiveMinimum = JsonSerializer.Double.Deserialize(jsonExclusiveMinimum);
            if (json.AsObject.TryGetValue("maxLength", out var jsonMaxLength) && !jsonMaxLength.IsNull)
                value.MaxLength = JsonSerializer.UInt.Deserialize(jsonMaxLength);
            if (json.AsObject.TryGetValue("minLength", out var jsonMinLength) && !jsonMinLength.IsNull)
                value.MinLength = JsonSerializer.UInt.Deserialize(jsonMinLength);
            if (json.AsObject.TryGetValue("pattern", out var jsonPattern) && !jsonPattern.IsNull)
                value.Pattern = JsonSerializer.String.Deserialize(jsonPattern);
            if (json.AsObject.TryGetValue("maxItems", out var jsonMaxItems) && !jsonMaxItems.IsNull)
                value.MaxItems = JsonSerializer.UInt.Deserialize(jsonMaxItems);
            if (json.AsObject.TryGetValue("minItems", out var jsonMinItems) && !jsonMinItems.IsNull)
                value.MinItems = JsonSerializer.UInt.Deserialize(jsonMinItems);
            if (json.AsObject.TryGetValue("uniqueItems", out var jsonUniqueItems) && !jsonUniqueItems.IsNull)
                value.UniqueItems = JsonSerializer.Bool.Deserialize(jsonUniqueItems);
            if (json.AsObject.TryGetValue("maxContains", out var jsonMaxContains) && !jsonMaxContains.IsNull)
                value.MaxContains = JsonSerializer.UInt.Deserialize(jsonMaxContains);
            if (json.AsObject.TryGetValue("minContains", out var jsonMinContains) && !jsonMinContains.IsNull)
                value.MinContains = JsonSerializer.UInt.Deserialize(jsonMinContains);
            if (json.AsObject.TryGetValue("maxProperties", out var jsonMaxProperties) && !jsonMaxProperties.IsNull)
                value.MaxProperties = JsonSerializer.UInt.Deserialize(jsonMaxProperties);
            if (json.AsObject.TryGetValue("minProperties", out var jsonMinProperties) && !jsonMinProperties.IsNull)
                value.MinProperties = JsonSerializer.UInt.Deserialize(jsonMinProperties);
            if (json.AsObject.TryGetValue("required", out var jsonRequired) && !jsonRequired.IsNull)
                value.Required = JsonSerializer.List(JsonSerializer.String).Deserialize(jsonRequired);
            if (json.AsObject.TryGetValue("dependentRequired", out var jsonDependentRequired) && !jsonDependentRequired.IsNull)
                value.DependentRequired = JsonSerializer.Dict(JsonSerializer.String, JsonSerializer.List(JsonSerializer.String)).Deserialize(jsonDependentRequired);
            if (json.AsObject.TryGetValue("unevaluatedItems", out var jsonUnevaluatedItems) && !jsonUnevaluatedItems.IsNull)
                value.UnevaluatedItems = SchemaObjectJsonSerializer.Instance.Deserialize(jsonUnevaluatedItems);
            if (json.AsObject.TryGetValue("unevaluatedProperties", out var jsonUnevaluatedProperties) && !jsonUnevaluatedProperties.IsNull)
                value.UnevaluatedProperties = SchemaObjectJsonSerializer.Instance.Deserialize(jsonUnevaluatedProperties);
            if (json.AsObject.TryGetValue("format", out var jsonFormat) && !jsonFormat.IsNull)
                value.Format = JsonSerializer.String.Deserialize(jsonFormat);
            if (json.AsObject.TryGetValue("contentEncoding", out var jsonContentEncoding) && !jsonContentEncoding.IsNull)
                value.ContentEncoding = JsonSerializer.String.Deserialize(jsonContentEncoding);
            if (json.AsObject.TryGetValue("contentMediaType", out var jsonContentMediaType) && !jsonContentMediaType.IsNull)
                value.ContentMediaType = JsonSerializer.String.Deserialize(jsonContentMediaType);
            if (json.AsObject.TryGetValue("contentSchema", out var jsonContentSchema) && !jsonContentSchema.IsNull)
                value.ContentSchema = SchemaObjectJsonSerializer.Instance.Deserialize(jsonContentSchema);
            if (json.AsObject.TryGetValue("title", out var jsonTitle) && !jsonTitle.IsNull)
                value.Title = JsonSerializer.String.Deserialize(jsonTitle);
            if (json.AsObject.TryGetValue("description", out var jsonDescription) && !jsonDescription.IsNull)
                value.Description = JsonSerializer.String.Deserialize(jsonDescription);
            if (json.AsObject.TryGetValue("default", out var jsonDefault) && !jsonDefault.IsNull)
                value.Default = JsonSerializer.Json.Deserialize(jsonDefault);
            if (json.AsObject.TryGetValue("deprecated", out var jsonDeprecated) && !jsonDeprecated.IsNull)
                value.Deprecated = JsonSerializer.Bool.Deserialize(jsonDeprecated);
            if (json.AsObject.TryGetValue("readOnly", out var jsonReadOnly) && !jsonReadOnly.IsNull)
                value.ReadOnly = JsonSerializer.Bool.Deserialize(jsonReadOnly);
            if (json.AsObject.TryGetValue("writeOnly", out var jsonWriteOnly) && !jsonWriteOnly.IsNull)
                value.WriteOnly = JsonSerializer.Bool.Deserialize(jsonWriteOnly);
            if (json.AsObject.TryGetValue("examples", out var jsonExamples) && !jsonExamples.IsNull)
                value.Examples = JsonSerializer.List(JsonSerializer.Json).Deserialize(jsonExamples);
        }

        public bool Test(Json.ImmutableJson json)
        {
            if (json == null)
                throw new System.ArgumentNullException(nameof(json));
            return json.IsObject;
        }
    }
}
