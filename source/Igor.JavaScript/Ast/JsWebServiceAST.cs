using Igor.Text;

namespace Igor.JavaScript.AST
{
    public partial class WebServiceForm
    {
        public string jsFileName => Attribute(JsAttributes.File, jsName.Format(Notation.LowerHyphen) + ".js");
    }

    public partial class WebResource
    {
        public string jsName => Attribute(JsAttributes.Name, Name.Format(Notation.LowerCamel));
    }

    public partial class WebContent
    {
        public string jsName => Var == null ? "payload" : Var.jsName;
    }

    public partial class WebHeader
    {
        public string jsHeaderValue => IsStatic ? StaticValue.Quoted() : Var.jsName;
    }

    public partial class WebVariable
    {
        public string jsName => Name.Format(Notation.LowerCamel);
    }
}
