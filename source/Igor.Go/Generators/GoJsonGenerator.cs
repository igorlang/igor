using Igor.Go.AST;
using Igor.Go.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Go
{
    internal class GoJsonGenerator : IGoGenerator
    {
        public void Generate(GoModel model, Module mod)
        {
            var go = model.FileOf(mod);

            foreach (var type in mod.Types)
            {
                if (type is EnumForm @enum && type.goEnabled && (type.stringEnabled || (type.jsonEnabled && !@enum.jsonNumber)) && !@enum.goStringEnum)
                {
                    GenEnumString(go, @enum);
                }

                if (type.goJsonEnabled)
                {
                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumSerializer(go, enumForm);
                            break;

                        case RecordForm recordForm:
                            GenStructSerializer(go, recordForm);
                            break;

                        case InterfaceForm intfForm:
                            GenStructSerializer(go, intfForm);
                            break;

                        case VariantForm variantForm:
                            GenStructSerializer(go, variantForm);
                            break;

                        case DefineForm defineForm:
                            break;
                    }
                }
            }
        }

        private void GenEnumString(GoFile go, EnumForm enumForm)
        {
            var r = GoRenderer.Create();
            r += "var (";
            r++;
            r += $"_{enumForm.goName}NameToValue = map[string]{enumForm.goName} {{";
            r++;
            r.Table(enumForm.Fields, f => new[] { f.stringValue.Quoted() + ":", f.goName + "," });
            r--;
            r += "}";
            r--;
            r += ")";
            go.Declare(r.Build(), enumForm);

            r.Reset();

            r += $"func ({enumForm.goShortVarName} {enumForm.goName}) String() string {{";
            r++;
            r += $"switch {enumForm.goShortVarName} {{";
            r.Blocks(enumForm.Fields, f => $@"case {f.goName}:
    return ""{f.stringValue}""");
            r += @"default:
    return ""Unknown""
}";
            r--;
            r += "}";
            go.Declare(r.Build(), enumForm);


            r.Reset();
            r += $@"// {enumForm.goName}FromString Returns a {enumForm.goName} enum from string
func {enumForm.goName}FromString(str string) ({enumForm.goName}, error) {{
    val, ok := _{enumForm.goName}NameToValue[str]
    if !ok {{
        return val, fmt.Errorf(""invalid {enumForm.goName} % q"", str)
    }}
    return val, nil
}}";
            go.Declare(r.Build(), enumForm);

            go.Import("fmt");
        }

        private void GenEnumSerializer(GoFile go, EnumForm enumForm)
        {
            if (!enumForm.goStringEnum && !enumForm.jsonNumber)
            {
                go.Import("encoding/json");

                var r = GoRenderer.Create();
                r.Block($@"// MarshalJSON will marshal {enumForm.goName} enum as a string
func ({enumForm.goShortVarName} {enumForm.goName}) MarshalJSON() ([]byte, error) {{
	return json.Marshal({enumForm.goShortVarName}.String())
}}");
                go.Declare(r.Build(), enumForm);

                r.Reset();
                go.Import("fmt");
                go.Declare(r.Build(), enumForm);
                r.Reset();
                r.Comment($"UnmarshalJSON handles unmarshal from json string to {enumForm.goName} enum", "// ");
                r += $"func ({enumForm.goShortVarName} *{enumForm.goName}) UnmarshalJSON(data []byte) error {{";
                r++;
                r.Block($@"var str string
if err := json.Unmarshal(data, &str); err != nil {{
    return fmt.Errorf(""{enumForm.goName} should be a string, got %s"", data)
}}
val, ok := _{enumForm.goName}NameToValue[str]
if !ok {{
    return fmt.Errorf(""invalid {enumForm.goName} % q"", str)
}}
*{enumForm.goShortVarName} = val
return nil");
                r--;
                r += "}";
                go.Declare(r.Build(), enumForm);
            }
        }

        private void GenStructSerializer(GoFile go, StructForm structForm)
        {
            var s = go.Struct(structForm.goName);
            foreach (var f in structForm.Fields.Where(f => f.IsLocal))
            {
                var tag = f.jsonIgnore ? "-" : f.jsonKey;
                if (!f.jsonIgnore && f.goJsonOmitempty)
                    tag += ",omitempty";
                s.Property(f.goName).Tag("json", tag);
            }
        }
    }
}
