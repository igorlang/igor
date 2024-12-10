namespace Igor.Parser
{
    public static class WebUtils
    {
        public static bool TryParseHttpMethod(string text, out HttpMethod method)
        {
            switch (text)
            {
                case "GET":
                    method = HttpMethod.GET;
                    return true;

                case "PUT":
                    method = HttpMethod.PUT;
                    return true;

                case "POST":
                    method = HttpMethod.POST;
                    return true;

                case "DELETE":
                    method = HttpMethod.DELETE;
                    return true;

                case "HEAD":
                    method = HttpMethod.HEAD;
                    return true;

                case "OPTIONS":
                    method = HttpMethod.OPTIONS;
                    return true;

                case "PATCH":
                    method = HttpMethod.PATCH;
                    return true;

                default:
                    method = HttpMethod.POST;
                    return false;
            }
        }

        public static bool TryParseDataFormat(string text, out DataFormat format)
        {
            switch (text)
            {
                case "json":
                    format = DataFormat.Json;
                    return true;
                case "xml":
                    format = DataFormat.Xml;
                    return true;
                case "text":
                    format = DataFormat.Text;
                    return true;
                case "binary":
                    format = DataFormat.Binary;
                    return true;
                case "form":
                    format = DataFormat.Form;
                    return true;
                default:
                    format = DataFormat.Default;
                    return false;
            }
        }
    }
}
