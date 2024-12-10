using System;
using System.Collections.Generic;
using System.Text;
using Json;
using Json.Serialization;
using OneOf;

namespace Json
{
    public static class OneOfJsonSerializer
    {
        public static OneOfJsonSerializer<T1, T2> Instance<T1, T2>(IJsonSerializer<T1> t1Serializer, IJsonSerializer<T2> t2Serializer)
        {
            return new OneOfJsonSerializer<T1, T2>(t1Serializer, t2Serializer);
        }
    }

    public struct OneOfJsonSerializer<T1, T2> : IJsonSerializer<OneOf<T1, T2>>
    {
        private readonly IJsonSerializer<T1> t1Serializer;
        private readonly IJsonSerializer<T2> t2Serializer;

        public OneOf<T1, T2> Deserialize(ImmutableJson json)
        {
            if (t1Serializer.Test(json))
                return t1Serializer.Deserialize(json);
            if (t2Serializer.Test(json))
                return t2Serializer.Deserialize(json);
            throw new ArgumentException("All OneOf tests failed");
        }

        public ImmutableJson Serialize(OneOf<T1, T2> value)
        {
            return value.Match(t1Serializer.Serialize, t2Serializer.Serialize);
        }

        public bool Test(ImmutableJson json)
        {
            return t1Serializer.Test(json) || t2Serializer.Test(json);
        }

        public OneOfJsonSerializer(IJsonSerializer<T1> t1Serializer, IJsonSerializer<T2> t2Serializer)
        {
            this.t1Serializer = t1Serializer;
            this.t2Serializer = t2Serializer;
        }
    }
}
