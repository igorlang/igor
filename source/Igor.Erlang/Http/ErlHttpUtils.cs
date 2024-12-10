using Igor.Erlang.AST;
using Igor.Erlang.Json;
using Igor.Erlang.Strings;

namespace Igor.Erlang.Http
{
    public static class ErlHttpUtils
    {
        public static string DecodeContent(WebContent content, WebResource resource, string sourceVar, string module)
        {
            var variable = content.Var;
            switch (content.Format)
            {
                case DataFormat.Xml:
                    return $"{Xml.XmlSerialization.ParseXml(Xml.XmlSerialization.XmlTag(content.Type, resource, null), $"igor_xml:decode({sourceVar})", module)}";
                case DataFormat.Text:
                    return StringSerialization.StringTag(content.Type, resource).ParseString(sourceVar, module);
                case DataFormat.Form:
                    return HttpSerialization.HttpFormTag(content.Type, resource, (Statement)variable ?? resource).ParseHttpForm(sourceVar, module);
                case DataFormat.Json:
                default:
                    return JsonSerialization.JsonTag(content.Type, resource).ParseJson($"jsx:decode({sourceVar}, [return_maps])");
            }
        }

        public static string EncodeContent(WebContent content, WebResource resource, string sourceVar, string module)
        {
            switch (content.Format)
            {
                case DataFormat.Xml:
                    return $"igor_xml:encode({Xml.XmlSerialization.PackXmlElement(Xml.XmlSerialization.XmlTag(content.Type, resource, null), sourceVar, module)})";
                case DataFormat.Form:
                    return "iolist_to_binary(" + HttpSerialization.HttpFormTag(content.Type, resource, resource).PackHttpForm(sourceVar, module) + ")";
                case DataFormat.Text:
                    return StringSerialization.StringTag(content.Type, resource).PackString(sourceVar, module);
                case DataFormat.Json:
                default:
                    return $"jsx:encode({JsonSerialization.JsonTag(content.Type, resource).PackJson(sourceVar, module)})";
            }
        }
    }
}
