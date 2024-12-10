using Json;
using Json.Serialization;

namespace OpenAPI
{
    public class MaybeRef<T> where T : class
    {
        public string Ref { get; }
        public T Value { get; }

        public MaybeRef(string @ref)
        {
            Ref = @ref;
        }

        public MaybeRef(T value)
        {
            Value = value;
        }
    }

    public static class MaybeRefSerializer
    {
        public static MaybeRefSerializer<T> Create<T>(IJsonSerializer<T> valueSerializer) where T : class => new MaybeRefSerializer<T>(valueSerializer);
    }

    public struct MaybeRefSerializer<T> : IJsonSerializer<MaybeRef<T>> where T : class
    {
        private readonly IJsonSerializer<T> valueSerializer;

        public MaybeRefSerializer(IJsonSerializer<T> valueSerializer)
        {
            this.valueSerializer = valueSerializer;
        }

        public MaybeRef<T> Deserialize(ImmutableJson json)
        {
            if (json.IsObject && json.AsObject.ContainsKey("$ref"))
            {
                return new MaybeRef<T>(json.AsObject["$ref"].AsString);
            }
            else
            {
                return new MaybeRef<T>(valueSerializer.Deserialize(json));
            }
        }

        public ImmutableJson Serialize(MaybeRef<T> value)
        {
            return ImmutableJson.EmptyObject;
        }

        public bool Test(ImmutableJson json)
        {
            return json.IsObject;
        }
    }
}
