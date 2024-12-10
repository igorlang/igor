using Igor.Text;
using System;
using System.Linq;
using System.Text;

namespace Igor.Go.AST
{
    public partial class WebServiceForm
    {
        public string goShortVarName => goName.Format(Notation.FirstLetterLastWord);
    }
    public partial class WebResource
    {
        public string goName => Attribute(GoAttributes.Name, Name.Format(Notation.UpperCamel));
    }

    public partial class WebVariable
    {
        public string goName => Attribute(GoAttributes.Name, Name.Format(Notation.LowerCamel));
    }

    public partial class WebContent
    {
        public string goName => goType.Name.Format(Notation.LowerCamel);
	public GoType goType => Helper.TargetType(Type);
    }
}
