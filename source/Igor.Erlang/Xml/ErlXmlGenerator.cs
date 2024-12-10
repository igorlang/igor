using Igor.Erlang.AST;
using Igor.Erlang.Model;
using Igor.Text;
using System.Linq;

namespace Igor.Erlang.Xml
{
    internal class ErlXmlGenerator : IErlangGenerator
    {
        public void Generate(ErlModel model, Module mod)
        {
            var erl = model.Module(mod.erlFileName);

            foreach (var type in mod.Types)
            {
                if (type.erlXmlGenSerializer)
                {
                    erl.IncludeLib("xmerl/include/xmerl.hrl");

                    switch (type)
                    {
                        case EnumForm enumForm:
                            GenEnumPacker(erl, enumForm);
                            GenEnumParser(erl, enumForm);
                            break;

                        case VariantForm variantForm:
                            GenVariantParser(erl, variantForm);
                            GenVariantPacker(erl, variantForm);
                            break;

                        case RecordForm recordForm:
                            ValidateRecord(recordForm);
                            GenRecordPacker(erl, recordForm);
                            GenRecordParser(erl, recordForm);
                            break;

                        case InterfaceForm interfaceForm when interfaceForm.erlInterfaceRecords:
                            ValidateRecord(interfaceForm);
                            GenRecordPacker(erl, interfaceForm);
                            GenRecordParser(erl, interfaceForm);
                            break;

                        case UnionForm unionForm:
                            ValidateUnion(unionForm);
                            GenUnionPacker(erl, unionForm);
                            GenUnionParser(erl, unionForm);
                            break;
                    }
                }
            }
        }

        private string ErlXmlType(XmlType type)
        {
            switch (type)
            {
                case XmlType.SimpleType: return "string()";
                case XmlType.Content: return "igor_xml:xml_content()";
                default: return "igor_xml:xml_element()";
            }
        }

        private void GenEnumParser(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlXmlParserName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlXmlParserName}(string()) -> {enumForm.erlLocalType}.

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlXmlParserName}(""{field.xmlName}"") -> {field.erlName}")}.", enumForm);
        }

        private void GenEnumPacker(ErlModule erl, EnumForm enumForm)
        {
            erl.Export(enumForm.erlXmlPackerName, 1, enumForm);

            erl.Function(
$@"-spec {enumForm.erlXmlPackerName}({enumForm.erlLocalType}) -> string().

{enumForm.Fields.JoinStrings(";\n", field => $@"{enumForm.erlXmlPackerName}({field.erlName}) -> ""{field.xmlName}""")}.", enumForm);
        }

        private void ValidateRecord(StructForm structForm)
        {
            var type = structForm.xmlType;
            if (type == XmlType.SimpleType)
                structForm.Error("XML simple types are not supported for generated records. Remove xml.simple_type attribute or provide custom xml.packer.");
            if (type == XmlType.Content)
            {
                foreach (var f in structForm.Fields)
                {
                    if (f.xmlAttribute)
                        f.Error("XML attributes are not allowed in content records.");
                }
            }
        }

        private void GenRecordPacker(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            var type = structForm.xmlType;
            var arity = 1 + structForm.Arity;
            if (type == XmlType.ComplexType)
                arity++;
            erl.IncludeLib("stdlib/include/assert.hrl");
            erl.Export(structForm.erlXmlPackerName, arity, structForm);

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_xml:pack_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";
            var erlNameArg = type == XmlType.ComplexType ? ", ElementName" : "";
            var erlNameSpec = type == XmlType.ComplexType ? ", atom()" : "";
            var serFields = structForm.Fields.Where(f => !f.xmlIgnore && !f.IsTag);
            var empty = !serFields.Any();
            var attributes = serFields.Where(f => f.xmlAttribute).ToList();
            var elements = serFields.Where(f => !f.xmlAttribute).ToList();

            var xmlVar = empty && structForm.xmlType != XmlType.Element ? "_XmlElement" : "XmlElement";

            var r = new Renderer();
            r += $@"-spec {structForm.erlXmlPackerName}({structForm.erlLocalType}{erlNameSpec}{erlSpecArgs}) -> {ErlXmlType(structForm.xmlType)}.";
            r += null;
            if (empty)
            {
                r += $"{structForm.erlXmlPackerName}(_Record{erlNameArg}{erlCommaArgs}) ->";
                r++;
                switch (type)
                {
                    case XmlType.Element:
                        r += $"#xmlElement{{name = {structForm.erlXmlElementNameAtom}}}.";
                        break;

                    case XmlType.Content:
                        r += "[].";
                        break;

                    case XmlType.ComplexType:
                        r += "#xmlElement{name = ElementName}.";
                        break;
                }
            }
            else
            {
                r += $"{structForm.erlXmlPackerName}(Record{erlNameArg}{erlCommaArgs}) ->";
                r++;
                CodeUtils.DeconstructStruct(r, structType, serFields, "Record");

                string attrsValue = null;
                if (attributes.Any())
                {
                    attrsValue = ", attributes = Attributes";
                    var requiredAttrs = attributes.Where(a => !a.IsOptional);
                    var optionalAttrs = attributes.Where(a => a.IsOptional);
                    bool hasReq = requiredAttrs.Any();
                    bool hasOpt = optionalAttrs.Any();
                    string packAttribute(RecordField attr) =>
                        $"#xmlAttribute{{name = {Helper.AtomName(attr.xmlAttributeName)}, value = igor_xml:pack_simple_type({attr.erlVarName}, {attr.erlXmlPackTag.PackTag})}}";
                    if (hasReq)
                    {
                        var postfix = hasOpt ? "\n" : ",\n";
                        CodeUtils.Group(r, "Attributes = [", requiredAttrs.Select(packAttribute), "]" + postfix, 1);
                    }
                    if (hasOpt)
                    {
                        var prefix = hasReq ? "++ " : "Attributes = ";
                        var c = optionalAttrs.Count();
                        int i = 0;
                        foreach (var attr in optionalAttrs)
                        {
                            var comma = i == c - 1 ? "," : "";
                            r += $"{prefix}[{packAttribute(attr)} || {attr.erlVarName} =/= undefined]{comma}";
                            prefix = "++ ";
                            i++;
                        }
                    }
                }

                string contentValue = null;
                if (elements.Any())
                {
                    contentValue = ", content = Content";
                    string packElement(RecordField f)
                    {
                        if (f.xmlText)
                            return $"[#xmlText{{value = igor_xml:pack_simple_type({f.erlVarName}, {f.erlXmlPackTag.PackTag})}}]";
                        else if (f.xmlContent)
                            return $"igor_xml:pack_content({f.erlVarName}, {f.erlXmlPackTag.PackTag})";
                        else
                            return $"igor_xml:pack_subelement({Helper.AtomName(f.xmlElementName)}, {f.erlVarName}, {f.erlXmlPackTag.PackTag})";
                    }
                    var prefix = "Content = ";
                    var c = elements.Count;
                    int i = 0;
                    foreach (var element in elements)
                    {
                        var comma = i == c - 1 ? "," : "";
                        r += $"{prefix}{packElement(element)}{comma}";
                        prefix = "++ ";
                        i++;
                    }
                }

                switch (type)
                {
                    case XmlType.Element:
                        r += $"#xmlElement{{name = {structForm.erlXmlElementNameAtom}{attrsValue}{contentValue}}}.";
                        break;

                    case XmlType.Content:
                        r += "[].";
                        break;

                    case XmlType.ComplexType:
                        r += $"#xmlElement{{name = ElementName{attrsValue}{contentValue}}}.";
                        break;
                }
            }
            erl.Function(r.Build(), structForm);
        }

        private void GenRecordParser(ErlModule erl, StructForm structForm)
        {
            var structType = structForm.erlStructType;
            erl.Export(structForm.erlXmlParserName, 1 + structForm.Arity, structForm);

            var erlSpecArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", igor_xml:parse_type({arg.erlName})")}" : "";
            var erlCommaArgs = structForm.IsGeneric ? $"{structForm.Args.JoinStrings(arg => $", {arg.erlName}")}" : "";
            var serFields = structForm.Fields.Where(f => !f.xmlIgnore && !f.IsTag);
            var empty = !serFields.Any();
            var attributes = serFields.Where(f => f.xmlAttribute);
            var elements = serFields.Where(f => !f.xmlAttribute).ToList();

            var xmlVar = empty && structForm.xmlType != XmlType.Element ? "_XmlElement" : "XmlElement";

            var r = new Renderer();
            r += $@"-spec {structForm.erlXmlParserName}({ErlXmlType(structForm.xmlType)}{erlSpecArgs}) -> {structForm.erlLocalType}.";
            r += null;
            r += $"{structForm.erlXmlParserName}({xmlVar}{erlCommaArgs}) ->";
            r++;
            if (structForm.xmlType == XmlType.Element)
            {
                r += $"{structForm.erlXmlElementNameAtom} = igor_xml:name(XmlElement),";
            }
            foreach (var attr in attributes)
            {
                var defaultValue = (attr.HasDefault || attr.IsOptional) ? $", {attr.erlValue}" : "";
                r += $"{attr.erlVarName} = igor_xml:parse_attribute(XmlElement, {Helper.AtomName(attr.xmlAttributeName)}, {attr.erlXmlParseTag.ParseTag}{defaultValue}),";
            }
            if (elements.Count > 0)
            {
                if (structForm.xmlOrdered)
                {
                    if (elements.Any(e => !e.xmlText))
                        r += "XmlContent = igor_xml:content(XmlElement),";
                    int i = 0;
                    foreach (var field in elements)
                    {
                        var defaultValue = (field.HasDefault || field.IsOptional) ? $", {field.erlValue}" : "";
                        var resultContent = i == elements.Count - 1 ? "_" : $"XmlContent{i}";
                        var sourceContent = i == 0 ? "XmlContent" : $"XmlContent{i - 1}";
                        if (field.xmlText)
                            r += $"{field.erlVarName} = igor_xml:parse_text(XmlElement, {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                        else if (field.xmlContent)
                            r += $"{field.erlVarName} = igor_xml:parse_content({sourceContent}, {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                        else
                            r += $"{{{field.erlVarName}, {resultContent}}} = igor_xml:parse_sequence_subelement({sourceContent}, {Helper.AtomName(field.xmlElementName)}, {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                        i++;
                    }
                }
                else
                {
                    foreach (var field in elements)
                    {
                        var defaultValue = (field.HasDefault || field.IsOptional) ? $", {field.erlValue}" : "";
                        if (field.xmlText)
                            r += $"{field.erlVarName} = igor_xml:parse_text(XmlElement, {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                        else if (field.xmlContent)
                            r += $"{field.erlVarName} = igor_xml:parse_content(igor_xml:content(XmlElement), {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                        else
                            r += $"{field.erlVarName} = igor_xml:parse_subelement(XmlElement, {Helper.AtomName(field.xmlElementName)}, {field.erlXmlParseTag.ParseTag}{defaultValue}),";
                    }
                }
            }
            CodeUtils.ReturnStruct(r, structType, serFields, structType.IsTuple ? 100 : 3);
            erl.Function(r.Build(), structForm);
        }

        private void GenVariantPacker(ErlModule erl, VariantForm variantForm)
        {
            erl.Export(variantForm.erlXmlPackerName, 1, variantForm);

            string Clause(RecordForm rec) =>
$@"{variantForm.erlXmlPackerName}(Record) when {rec.erlGuard("Record")} ->
    {rec.erlXmlTag(variantForm, null).PackXmlElement("Record", erl.Name)}";

            erl.Function(
$@"-spec {variantForm.erlXmlPackerName}({variantForm.erlLocalType}) -> igor_xml:xml_element().

{variantForm.Records.JoinStrings(";\n", Clause)}.", variantForm);
        }

        private void GenVariantParser(ErlModule erl, VariantForm variantForm)
        {
            erl.Export(variantForm.erlXmlParserName, 1, variantForm);

            string Clause(RecordForm rec) =>
$@"        {rec.erlXmlElementNameAtom} ->
            {rec.erlXmlTag(variantForm, null).ParseXml("XmlElement", erl.Name)}";

            erl.Function(
$@"-spec {variantForm.erlXmlParserName}(igor_xml:xml_element()) -> {variantForm.erlLocalType}.

{variantForm.erlXmlParserName}(XmlElement) ->
    case igor_xml:name(XmlElement) of
{variantForm.Records.JoinStrings(";\n", Clause)}
    end.
", variantForm);
        }

        private void GenUnionPacker(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_xml:pack_type({arg.erlName})")}" : "";

            erl.Export(unionForm.erlXmlPackerName, 1 + unionForm.Arity, unionForm);

            var r = new Renderer();
            r += $@"-spec {unionForm.erlXmlPackerName}({unionForm.erlLocalType}{erlSpecArgs}) -> {ErlXmlType(unionForm.xmlType)}.";
            r += null;

            for (int i = 0; i < unionForm.Clauses.Count; i++)
            {
                var sep = i == unionForm.Clauses.Count - 1 ? "." : ";";
                var clause = unionForm.Clauses[i];

                if (clause.IsSingleton)
                    r += $@"{unionForm.erlXmlPackerName}({clause.erlTag}{clause.erlGenericArgs}) ->";
                else if (clause.erlTagged)
                    r += $@"{unionForm.erlXmlPackerName}({{{clause.erlTag}, Value}}{clause.erlGenericArgs}) ->";
                else
                    r += $@"{unionForm.erlXmlPackerName}(Value{clause.erlGenericArgs}) when {clause.erlGuard("Value")} ->";

                r++;

                switch (unionForm.xmlType)
                {
                    case XmlType.Element:
                        {
                            if (clause.IsSingleton)
                                r += $"#xmlElement{{name = {Helper.AtomName(clause.xmlElementName)}}}{sep}";
                            else if (clause.xmlText)
                                r += $"#xmlText{{value = igor_xml:pack_simple_type(Value, {clause.erlXmlTag.PackTag})}}{sep}";
                            else if (clause.xmlContent)
                            {
                                if (!(clause.erlXmlTag is XmlSerializationTags.CustomElement))
                                    clause.Error("xml.content attribute is only supported for elements in union clause.");
                                r += $"{clause.erlXmlTag.PackXmlElement("Value", erl.Name)}{sep}";
                            }
                            else
                                r += $"igor_xml:pack_complex_type({Helper.AtomName(clause.xmlElementName)}, Value, {clause.erlXmlTag.PackTag}){sep}";
                        }
                        break;
                    case XmlType.Content:
                        {
                            if (clause.IsSingleton)
                                r += $"[#xmlElement{{name = {Helper.AtomName(clause.xmlElementName)}}}]{sep}";
                            else if (clause.xmlText)
                                r += $"[#xmlText{{value = igor_xml:pack_simple_type(Value, {clause.erlXmlTag.PackTag})}}]{sep}";
                            else if (clause.xmlContent)
                            {
                                if (clause.erlXmlTag is XmlSerializationTags.CustomComplexType)
                                    clause.Error("xml.content attribute is not supported for complex types.");
                                r += $"igor_xml:pack_content(Value, {clause.erlXmlTag.PackTag}){sep}";
                            }
                            else
                                r += $"igor_xml:pack_subelement({Helper.AtomName(clause.xmlElementName)}, Value, {clause.erlXmlTag.PackTag}){sep}";
                        }
                        break;
                }

                r--;
            }

            erl.Function(r.Build(), unionForm);
        }

        private void ValidateUnion(UnionForm unionForm)
        {
            var type = unionForm.xmlType;
            if (type == XmlType.SimpleType)
                unionForm.Error("XML simple types are not supported for unions yet.");
            if (type == XmlType.ComplexType)
                unionForm.Error("xml.complex_type is not supported for unions.");
        }

        private void GenUnionParser(ErlModule erl, UnionForm unionForm)
        {
            var erlSpecArgs = unionForm.IsGeneric ? $"{unionForm.Args.JoinStrings(arg => $", igor_xml:parse_type({arg.erlName})")}" : "";

            erl.Export(unionForm.erlXmlParserName, 1 + unionForm.Arity, unionForm);

            var r = new Renderer();
            r.Line($@"-spec {unionForm.erlXmlParserName}({ErlXmlType(unionForm.xmlType)}{erlSpecArgs}) -> {unionForm.erlLocalType}.");
            r.EmptyLine();
            switch (unionForm.xmlType)
            {
                case XmlType.SimpleType:
                    break;

                case XmlType.ComplexType:
                    break;

                case XmlType.Element:
                    {
                        foreach (var clause in unionForm.Clauses)
                        {
                            bool isLast = clause == unionForm.Clauses[unionForm.Clauses.Count - 1];
                            var sep = isLast ? "." : ";";
                            if (clause.IsSingleton)
                            {
                                if (clause.xmlText)
                                {
                                    r.Block($@"{unionForm.erlXmlParserName}(#xmlText{{}}{clause.erlGenericArgs}) ->
    {clause.erlTag}{sep}");
                                }
                                else if (clause.xmlContent)
                                {
                                    r.Block($@"{unionForm.erlXmlParserName}(_XmlElement{clause.erlGenericArgs}) ->
    {clause.erlTag}{sep}");
                                }
                                else
                                {
                                    r.Block($@"{unionForm.erlXmlParserName}(#xmlElement{{name = {Helper.AtomName(clause.xmlElementName)}}}{clause.erlGenericArgs}) ->
    {clause.erlTag}{sep}");
                                }
                            }
                            else
                            {
                                var tag = clause.erlXmlTag;
                                string MakeValue(string value)
                                {
                                    if (clause.erlTagged)
                                        return $"{{{clause.erlTag}, {value}}}";
                                    else
                                        return value;
                                }
                                if (clause.xmlText)
                                {
                                    r.Block(
    $@"{unionForm.erlXmlParserName}(#xmlText{{value = Value}}{clause.erlGenericArgs}) ->
    {MakeValue(tag.ParseXmlSimpleType("Value", erl.Name))}{sep}");
                                }
                                else if (clause.xmlContent)
                                {
                                    r.Block($@"{unionForm.erlXmlParserName}(XmlElement{clause.erlGenericArgs}) ->
    {MakeValue(tag.ParseXmlContent("XmlElement", erl.Name))}{sep}");
                                }
                                else
                                {
                                    r.Block($@"{unionForm.erlXmlParserName}(#xmlElement{{name = {Helper.AtomName(clause.xmlElementName)}}} = XmlElement{clause.erlGenericArgs}) ->
    {MakeValue(tag.ParseXmlComplexType("XmlElement", erl.Name))}{sep}");
                                }
                            }
                        }
                    }
                    break;

                case XmlType.Content:
                    {
                        foreach (var clause in unionForm.Clauses)
                        {
                            bool isLast = clause == unionForm.Clauses[unionForm.Clauses.Count - 1];
                            var sep = isLast ? "." : ";";
                            var tag = clause.erlXmlTag;
                            if (clause.xmlText)
                            {
                                r.Block(
$@"{unionForm.erlXmlParserName}([#xmlText{{value = Value}}]) ->
    {{{clause.erlTag}, {tag.ParseXmlSimpleType("Value", erl.Name)}}}{sep}");
                            }
                            else if (clause.xmlContent)
                            {
                                r.Block($@"{unionForm.erlXmlParserName}(XmlContent) ->
    {{{clause.erlTag}, {tag.ParseXmlContent("XmlContent", erl.Name)}}}{sep}");
                            }
                            else
                            {
                                r.Block($@"{unionForm.erlXmlParserName}([#xmlElement{{name = {Helper.AtomName(clause.xmlElementName)}}}|_] = XmlContent) ->
    {{{clause.erlTag}, {tag.ParseXmlContent("XmlContent", erl.Name)}}}{sep}");
                            }
                        }
                    }
                    break;
            }

            erl.Function(r.Build(), unionForm);
        }
    }
}
