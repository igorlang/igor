namespace Igor
{
    public static class HttpUtils
    {
        public static string GetDefaultContentType(DataFormat format, bool includeCharset)
        {
            if (includeCharset == false)
            {
                switch (format)
                {
                    case DataFormat.Text:
                        return "text/plain";
                    case DataFormat.Xml:
                        return "application/xml";
                    case DataFormat.Form:
                        return "application/x-www-form-urlencoded";
                    case DataFormat.Json:
                    default:
                        return "application/json";
                }
            }
            else
            {
                return GetDefaultContentType(format, false) + "; charset=utf-8";
            }
        }

        public static DataFormat GetDataFormatFromContentType(string contentType)
        {
            switch (contentType)
            {
                case "text/plain":
                    return DataFormat.Text;
                case "application/xml":
                    return DataFormat.Xml;
                case "application/x-www-form-urlencoded":
                    return DataFormat.Form;
                default:
                    return DataFormat.Default;
            }
        }
    }
}
