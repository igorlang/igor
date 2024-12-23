﻿using System;
using System.Collections.Generic;
using System.Linq;
using Igor.JsonSchema.AST;
using Json;

namespace Igor.JsonSchema
{
    public class JsonSchemaBuilder
    {
        public Dictionary<string, SchemaObject> Defs = new Dictionary<string, SchemaObject>();
        public TypeForm RootType { get; }

        public JsonSchemaBuilder(TypeForm rootType)
        {
            RootType = rootType;
        }
        
        public SchemaObject Build()
        {
            var schema = TypeFormSchema(RootType);
            schema.Schema = "https://json-schema.org/draft/2020-12/schema";
            if (Defs.Any())
                schema.Defs = Defs;
            return schema;
        }

        public SchemaObject TypeSchema(IType rootType)
        {
            switch (rootType)
            {
                case BuiltInType.Integer _: return new SchemaObject { Type = SimpleType.Integer };
                case BuiltInType.Float _: return new SchemaObject { Type = SimpleType.Number };
                case BuiltInType.String _: return new SchemaObject { Type = SimpleType.String };
                case BuiltInType.Atom _: return new SchemaObject { Type = SimpleType.String };
                case BuiltInType.Binary _: return new SchemaObject { Type = SimpleType.String };
                case BuiltInType.Bool _: return new SchemaObject { Type = SimpleType.Boolean};
                case BuiltInType.Json _: return new SchemaObject { };
                case BuiltInType.List list: return new SchemaObject { Type = SimpleType.Array, Items = TypeSchema(list.ItemType) };
                case BuiltInType.Dict dict: return new SchemaObject { Type = SimpleType.Object, AdditionalProperties = TypeSchema(dict.ValueType) };
                case BuiltInType.Flags flags: return new SchemaObject { Type = SimpleType.Array, Items = TypeSchema(flags.ItemType) };
                case TypeForm typeForm: return EnsureDef(typeForm.Name, () => TypeFormSchema(typeForm));
                default: return new SchemaObject();
            }
        }

        private SchemaObject TypeFormSchema(TypeForm typeForm)
        {
            switch (typeForm)
            {
                case DefineForm defineForm: return TypeSchema(defineForm.Type);
                case EnumForm enumForm: return EnumSchema(enumForm);
                case RecordForm recordForm: return RecordSchema(recordForm);
                case VariantForm variantForm: return VariantSchema(variantForm);
                default: return new SchemaObject();
            }
        }

        public ImmutableJson SchemaValue(Value value)
        {
            switch (value)
            {
                case Value.Bool b: return b.Value;
                case Value.Integer i: return i.Value;
                case Value.Float f: return f.Value;
                case Value.String s: return s.Value;
                case Value.EmptyObject _: return ImmutableJson.EmptyObject;
                case Value.List _: return ImmutableJson.EmptyArray;
                case Value.Enum e: return e.Field.jsonKey;
                default: return ImmutableJson.Null;
            }
        }

        private SchemaObject EnsureDef(string typeName, Func<SchemaObject> lazySchema)
        {
            if (RootType.Name == typeName)
                return new SchemaObject { Ref = $"#" };
            if (!Defs.ContainsKey(typeName))
            {
                Defs.Add(typeName, lazySchema());
            }
            return new SchemaObject { Ref = $"#/$defs/{typeName}" };
        }

        private SchemaObject EnumSchema(EnumForm enumForm)
        {
            return new SchemaObject { Enum = enumForm.Fields.Select(f => f.jsonKey).ToList() };
        }

        private SchemaObject RecordSchema(RecordForm recordForm)
        {
            var fields = recordForm.Fields.Where(f => !f.jsonIgnore);
            var result = new SchemaObject { Properties = fields.ToDictionary(f => f.jsonKey, RecordFieldSchema) };
            if (fields.Any(f => !f.IsOptional))
            {
                result.Required = fields.Where(f => !f.IsOptional).Select(f => f.jsonKey).ToList();
            }
            return result;
        }

        private SchemaObject RecordFieldSchema(RecordField recordField)
        {
            if (recordField.IsTag)
            {
                return new SchemaObject { Const = SchemaValue(recordField.Default) };
            }
            var schema = TypeSchema(recordField.Type);
            if (recordField.DefaultValue != null)
            {
                schema.Default = SchemaValue(recordField.DefaultValue);
            }
            return schema;
        }

        private SchemaObject VariantSchema(VariantForm variantForm)
        {
            return new SchemaObject { OneOf = variantForm.Records.Select(TypeSchema).ToList() };
        }
    }
}