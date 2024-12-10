using Igor.Erlang.Http;
using Igor.Text;
using System.Collections.Generic;

namespace Igor.Erlang.AST
{
    public partial class Form
    {
        public bool erlHttpFormEnabled => Attribute(CoreAttributes.HttpFormEnabled, false);
    }

    public partial class RecordField
    {
        public Notation httpFormNotation => Attribute(CoreAttributes.HttpFormNotation, Notation.None);
        public string httpFormName => Attribute(CoreAttributes.HttpFormName, Name.Format(httpFormNotation));
    }

    public partial class TypeForm
    {
        private string erlHttpQueryUserPacker => Attribute(ErlAttributes.HttpQueryPacker, null);
        private string erlHttpQueryUserParser => Attribute(ErlAttributes.HttpQueryParser, null);
        private bool erlHttpQueryUserSerializer => erlHttpQueryUserParser != null && erlHttpQueryUserPacker != null;

        private string erlHttpUriUserPacker => Attribute(ErlAttributes.HttpUriPacker, null);
        private string erlHttpUriUserParser => Attribute(ErlAttributes.HttpUriParser, null);
        private bool erlHttpUriUserSerializer => erlHttpUriUserParser != null && erlHttpUriUserPacker != null;

        protected virtual SerializationTag erlHttpDefaultQueryTag(Statement referrer, Statement variable) => erlStringTag(referrer);

        protected virtual SerializationTag erlHttpDefaultUriTag(Statement referrer, Statement variable) => erlStringTag(referrer);

        public SerializationTag erlHttpQueryTag(Statement referrer, Statement variable)
        {
            if (erlHttpQueryUserSerializer)
                return new QuerySerializationTags.CustomQuery(erlHttpQueryUserPacker, erlHttpQueryUserParser, erlArgTags);
            else if (erlStringUserPacker != null)
                return erlStringTag(referrer);
            else
                return erlHttpDefaultQueryTag(referrer, variable);
        }

        public SerializationTag erlHttpUriTag(Statement referrer, Statement variable)
        {
            if (erlHttpUriUserSerializer)
                return new SerializationTags.Custom(erlHttpUriUserPacker, erlHttpUriUserParser, erlArgTags);
            else if (erlStringUserPacker != null)
                return erlStringTag(referrer);
            else
                return erlHttpDefaultUriTag(referrer, variable);
        }

        public bool erlHttpFormGenSerializer => erlHttpFormEnabled;

        public string erlHttpFormGenPackerName => $"{erlName}_to_form";
        public string erlHttpFormGenParserName => $"{erlName}_from_form";

        protected virtual SerializationTag erlHttpDefaultFormTag(Statement referrer, Statement variable) => erlStringTag(referrer);

        public SerializationTag erlHttpFormTag(Statement referrer, Statement variable)
        {
            return erlHttpDefaultFormTag(referrer, variable);
        }
    }

    public partial class GenericType
    {
        public SerializationTag erlHttpUriTag(Statement referrer, Statement variable) => Prototype.erlHttpUriTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.UriTag, referrer, variable));

        public SerializationTag erlHttpQueryTag(Statement referrer, Statement variable) => Prototype.erlHttpQueryTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.HttpQueryTag, referrer, variable));

        public SerializationTag erlHttpFormTag(Statement referrer, Statement variable) => Prototype.erlHttpFormTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.HttpFormTag, referrer, variable));
    }

    partial class DefineForm
    {
        protected override SerializationTag erlHttpDefaultQueryTag(Statement referrer, Statement variable)
            => HttpSerialization.HttpQueryTag(Type, this, variable);

        protected override SerializationTag erlHttpDefaultUriTag(Statement referrer, Statement variable)
            => HttpSerialization.UriTag(Type, this, variable);

        protected override SerializationTag erlHttpDefaultFormTag(Statement referrer, Statement variable)
            => HttpSerialization.HttpFormTag(Type, this, variable);
    }

    partial class StructForm
    {
        public IEnumerable<RecordField> erlHttpFormSerializedFields => Fields;

        protected override SerializationTag erlHttpDefaultFormTag(Statement referrer, Statement variable) => new SerializationTags.Custom($"{Module.erlName}:{erlHttpFormGenPackerName}", $"{Module.erlName}:{erlHttpFormGenParserName}", erlArgTags);
    }

    partial class WebResource
    {
        public bool erlHttpClientLog => Attribute(ErlAttributes.HttpClientLog, false);
    }
}
