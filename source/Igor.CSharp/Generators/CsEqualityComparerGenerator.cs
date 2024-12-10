using Igor.CSharp.AST;
using Igor.CSharp.Model;

namespace Igor.CSharp
{
    internal class CsEqualityComparerGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var enumForm in mod.Enums)
            {
                if (enumForm.csEnabled && enumForm.csEqualityComparer)
                    GenerateEqualityComparer(model, enumForm);
            }
        }

        public void GenerateEqualityComparer(CsModel model, EnumForm enumForm)
        {
            var baseClass = $"EqualityComparer<{enumForm.csName}>";
            var cl = model.NamespaceOf(enumForm).DefineClass(enumForm.csEqualityComparerName, baseClass: baseClass);
            cl.Method(
$@"
public override bool Equals({enumForm.csName} x, {enumForm.csName} y)
{{
    return x == y;
}}
");

            cl.Method(
$@"
public override int GetHashCode({enumForm.csName} x)
{{
    return (int)x;
}}
");
            cl.Property("Instance", $"public static readonly {enumForm.csEqualityComparerName} Instance = new {enumForm.csEqualityComparerName}();");
        }
    }
}
