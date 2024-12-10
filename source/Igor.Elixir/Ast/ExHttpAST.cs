using Igor.Text;
using System.Collections.Generic;

namespace Igor.Elixir.AST
{
    public partial class Form
    {
        public bool exHttpFormEnabled => Attribute(CoreAttributes.HttpFormEnabled, false);
    }

    public partial class RecordField
    {
        public Notation httpFormNotation => Attribute(CoreAttributes.HttpFormNotation, Notation.None);
        public string httpFormName => Attribute(CoreAttributes.HttpFormName, Name.Format(httpFormNotation));
    }

    public partial class TypeForm
    {
        private string exHttpQueryCustom => Attribute(ExAttributes.HttpQueryCustom, null);

        private string exHttpUriCustom => Attribute(ExAttributes.HttpUriCustom, null);

        protected virtual SerializationTag exHttpDefaultQueryTag(Statement referrer, Statement variable) => exStringTag(referrer);

        protected virtual SerializationTag exHttpDefaultUriTag(Statement referrer, Statement variable) => exStringTag(referrer);

        public SerializationTag exHttpQueryTag(Statement referrer, Statement variable)
        {
            if (exHttpQueryCustom != null)
                return new QuerySerializationTags.CustomQuery(exHttpQueryCustom, exArgTags);
            else if (exStringCustom != null)
                return exStringTag(referrer);
            else
                return exHttpDefaultQueryTag(referrer, variable);
        }

        public SerializationTag exHttpUriTag(Statement referrer, Statement variable)
        {
            if (exHttpUriCustom != null)
                return new SerializationTags.Custom(exHttpUriCustom, exArgTags);
            else if (exStringCustom != null)
                return exStringTag(referrer);
            else
                return exHttpDefaultUriTag(referrer, variable);
        }

        public bool exHttpFormGenSerializer => exHttpFormEnabled;

        public string exHttpFormGenPackerName => $"{exName}.to_form!";
        public string exHttpFormGenParserName => $"{exName}.from_form!";

        protected virtual SerializationTag exHttpDefaultFormTag(Statement referrer, Statement variable) => exStringTag(referrer);

        public SerializationTag exHttpFormTag(Statement referrer, Statement variable)
        {
            return exHttpDefaultFormTag(referrer, variable);
        }
    }

    public partial class GenericType
    {
        public SerializationTag exHttpUriTag(Statement referrer, Statement variable) => Prototype.exHttpUriTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.UriTag, referrer, variable));

        public SerializationTag exHttpQueryTag(Statement referrer, Statement variable) => Prototype.exHttpQueryTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.HttpQueryTag, referrer, variable));

        public SerializationTag exHttpFormTag(Statement referrer, Statement variable) => Prototype.exHttpFormTag(referrer, variable).Instantiate(PrepareArgs(HttpSerialization.HttpFormTag, referrer, variable));
    }

    partial class DefineForm
    {
        protected override SerializationTag exHttpDefaultQueryTag(Statement referrer, Statement variable)
            => HttpSerialization.HttpQueryTag(Type, this, variable);

        protected override SerializationTag exHttpDefaultUriTag(Statement referrer, Statement variable)
            => HttpSerialization.UriTag(Type, this, variable);

        protected override SerializationTag exHttpDefaultFormTag(Statement referrer, Statement variable)
            => HttpSerialization.HttpFormTag(Type, this, variable);
    }

    partial class StructForm
    {
        public IEnumerable<RecordField> exHttpFormSerializedFields => Fields;

        protected override SerializationTag exHttpDefaultFormTag(Statement referrer, Statement variable) => new SerializationTags.Custom($"{Module.exName}.{exName}", exArgTags);
    }
}
