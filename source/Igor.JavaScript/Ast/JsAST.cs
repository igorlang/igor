using Igor.Text;
using System.Linq;

namespace Igor.JavaScript.AST
{
    public partial class Statement
    { }

    public partial class Module
    {
        public string tsFileName => Attribute(JsAttributes.File, tsName.Format(Notation.LowerHyphen) + ".ts");
        public string tsName => Attribute(JsAttributes.Name, Name.Format(Notation.UpperCamel));
    }

    public partial class GenericArgument
    {
        public string jsName => Name.Format(Notation.UpperCamel);
        public string jsVarName => Name.Format(Notation.LowerCamel);
    }

    public partial class Form
    {
        public string jsName => Attribute(JsAttributes.Name, Name.Format(Notation.UpperCamel));
        public bool jsEnabled => Attribute(CoreAttributes.Enabled, true);
    }
}
