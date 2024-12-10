using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Igor.Text;

namespace Igor.Model
{
    public class IgorRenderer : Renderer
    {
        public void WriteFile(IgorFile file)
        {
            Comment(file.Comment, "// ");
            Blocks(file.Usings.Select(u => $"using {u};"));
            ForEach(file.Modules, WriteDeclaration, emptyLineDelimiter: true);
        }

        public void WriteDeclaration(IgorDeclaration declaration)
        {
            Comment(declaration.Annotation, "# ");
            WriteAttributes(declaration.Attributes);
            switch (declaration)
            {
                case IgorModule module: 
                    WriteModule(module);
                    break;
                case IgorEnum e:
                    WriteEnum(e);
                    break;
                case IgorStruct s:
                    WriteStruct(s);
                    break;
                case IgorEnumField ef:
                    WriteEnumField(ef);
                    break;
                case IgorStructField sf:
                    WriteStructField(sf);
                    break;
                case IgorDefine d:
                    WriteDefine(d);
                    break;
                case IgorWebService ws:
                    WriteWebService(ws);
                    break;
                case IgorWebResource wr:
                    WriteWebResource(wr);
                    break;
            }
        }

        public void WriteAttributes(IReadOnlyList<IgorAttribute> attributes)
        {
            foreach (var group in attributes.GroupBy(a => a.Target))
            {
                string AttributeDefinition(IgorAttribute a)
                {
                    if (a.Value != null)
                        return $"{a.Name}={a.Value}";
                    else
                        return a.Name;
                }
                Line($"[{group.Key} {group.JoinStrings(" ", AttributeDefinition)}]");
            }
        }

        public void WriteModule(IgorModule module)
        {
            Line($"module {module.Name}");
            Line("{");
            Indent();
            ForEach(module.Declarations, WriteDeclaration, emptyLineDelimiter: true);
            Outdent();
            Line("}");
        }

        public void WriteEnum(IgorEnum e)
        {
            Line($"enum {e.Name}");
            Line("{");
            Indent();
            ForEach(e.Fields, WriteDeclaration, emptyLineDelimiter: e.Fields.Any(f => f.Annotation != null));
            Outdent();
            Line("}");
        }

        public void WriteEnumField(IgorEnumField f)
        {
            var value = f.Value.HasValue ? $" = {f.Value}" : "";
            Line($"{f.Name}{value};");
        }

        public void WriteStruct(IgorStruct s)
        {
            var ancestor = s.Ancestor == null ? "" : s.Ancestor + ".";
            var tagValue = s.TagValue == null ? "" : s.TagValue.Quoted("[", "]");
            switch (s.StructType)
            {
                case IgorStructType.Record:
                    Line($"record {ancestor}{s.Name}{tagValue}");
                    break;
                case IgorStructType.Variant:
                    Line($"variant {ancestor}{s.Name}");
                    break;
                case IgorStructType.Interface:
                    Line($"interface {s.Name}");
                    break;
            }
            Line("{");
            Indent();
            ForEach(s.Fields, WriteDeclaration, emptyLineDelimiter: s.Fields.Any(f => f.Annotation != null));
            Outdent();
            Line("}");
        }

        public void WriteStructField(IgorStructField f)
        {
            var value = f.Default != null ? $" = {f.Default}" : "";
            var tag = f.IsTag ? "tag " : "";
            Line($"{tag}{f.Type} {f.Name}{value};");
        }

        public void WriteDefine(IgorDefine d)
        {
            Line($"define {d.Name} {d.Type};");
        }

        public void WriteWebService(IgorWebService s)
        {
            Line($"webservice {s.Name}");
            Line("{");
            Indent();
            ForEach(s.Resources, WriteDeclaration, emptyLineDelimiter: true);
            Outdent();
            Line("}");
        }

        private string FormatVariable(IgorWebVariable variable)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            if (!string.IsNullOrEmpty(variable.Annotation))
            {
                sb.Append($"<# {variable.Annotation} #> ");
            }
            sb.Append(variable.Type);
            sb.Append(" ");
            sb.Append(variable.Name);
            if (variable.Format != DataFormat.Default)
            {
                sb.Append(" as ");
                sb.Append(variable.Format.ToString().ToLower());
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string FormatContent(IgorWebVariable variable)
        {
            if (variable.Name == null)
            {
                var format = variable.Format == DataFormat.Default ? "" : $" as {variable.Format.ToString().ToLower()}";
                return $"{variable.Type}{format}";
            }
            else
            {
                return FormatVariable(variable);
            }
        }

        private string FormatPath(string template, List<IgorWebVariable> pathVariables)
        {
            foreach (var pathVariable in pathVariables)
                template = template.Replace($"{{{pathVariable.Name}}}", FormatVariable(pathVariable));
            return template;
        }

        private string FormatQueryParameter(IgorWebQueryParameter query)
        {
            if (query.Variable != null)
            {
                return $"{query.Name}={FormatVariable(query.Variable)}";
            }
            else
            {
                return $"{query.Name}={query.Static}";
            }
        }

        private string FormatQuery(IReadOnlyList<IgorWebQueryParameter> query)
        {
            if (query.Count == 0)
                return "";
            else
                return "?" + query.JoinStrings("&", FormatQueryParameter);
        }

        public void WriteWebResource(IgorWebResource r)
        {
            var headers = r.RequestHeaders.JoinStrings(" ", p => $"~{p.Key}: {p.Value}");
            var requestConent = r.MaybeRequestContent == null ? "" : " " + FormatContent(r.MaybeRequestContent);
            Line($"{r.Name} => {r.Method} {FormatPath(r.PathTemplate, r.PathVariables)}{FormatQuery(r.Query)} {headers}{requestConent} ->");
            Indent();
            ForEach(r.Responses, WriteWebResponse);
            Outdent();
        }

        public void WriteWebResponse(IgorWebResponse r, bool isLast)
        {
            if (!string.IsNullOrEmpty(r.Annotation))
            {
                Comment(r.Annotation, "# ");
            }

            if (r.Code.HasValue)
            {
                Append(r.Code.Value.ToString());
                if (r.Status != null)
                {
                    Append(" ");
                    Append(r.Status);
                }

                Append(": ");
            }

            if (r.MaybeContent != null)
            {
                Append(FormatContent(r.MaybeContent));
            }
            Append(isLast ? ";" : ",");
            NewLine();
        }

        protected override EmptyLineMode GetEmptyLineModeBetweenLines(string prev, string next)
        {
            if (prev == "{")
                return EmptyLineMode.Forbid;
            else if (prev.StartsWith("[") && prev.EndsWith("]"))
                return EmptyLineMode.Forbid;
            else if (next == "}")
                return EmptyLineMode.Forbid;
            else
                return EmptyLineMode.Keep;
        }
    }
}
