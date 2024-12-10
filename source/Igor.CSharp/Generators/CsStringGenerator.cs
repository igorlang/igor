using Igor.CSharp.AST;
using Igor.CSharp.Model;
using Igor.Text;

namespace Igor.CSharp
{
    internal class CsStringGenerator : ICsGenerator
    {
        public void Generate(CsModel model, Module mod)
        {
            foreach (var type in mod.Types)
            {
                if (type.stringEnabled && type.csEnabled)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            if (enumForm.csStringGenerateSerializer)
                                GenerateEnum(model, enumForm);
                            break;
                    }
                }
            }
        }

        private void GenerateEnum(CsModel model, EnumForm enumForm)
        {
            var file = model.FileOf(enumForm);
            file.Use("System.Text");
            var csTypeName = enumForm.csType.relativeName(enumForm.csStringNamespace);
            var c = file.Namespace(enumForm.csStringNamespace).Class(enumForm.csStringGeneratedSerializerClass);
            c.Sealed = true;
            c.Interface($"Igor.Serialization.IStringSerializer<{csTypeName}>");

            var serFields = enumForm.Fields.JoinLines(f => $"        case {f.csQualifiedName(enumForm.csStringNamespace)}: return {f.stringValue.Quoted()}; ");
            c.Method(
$@"public string Serialize({csTypeName} value)
{{
    switch (value)
    {{
{serFields}
        default: throw new System.ArgumentOutOfRangeException({CsVersion.NameOf("value")});
    }}
}}
");
            c.Method(
            $@"public void Serialize(StringBuilder stringBuilder, {csTypeName} value)
{{
    stringBuilder.Append(Serialize(value));
}}
");

            var deserFields = enumForm.Fields.JoinLines(f => $"        case {f.stringValue.Quoted()}: return {f.csQualifiedName(enumForm.csStringNamespace)};");
            c.Method(
$@"public {csTypeName} Deserialize(string value)
{{
    switch (value)
    {{
{deserFields}
        default: throw new System.ArgumentOutOfRangeException({CsVersion.NameOf("value")});
    }}
}}
");

            c.Property("Instance",
$@"public static readonly {enumForm.csStringGeneratedSerializerClass} Instance = new {enumForm.csStringGeneratedSerializerClass}();
");
        }
    }
}
