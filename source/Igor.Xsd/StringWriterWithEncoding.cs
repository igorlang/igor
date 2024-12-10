﻿using System.IO;
using System.Text;

namespace Igor.Xsd
{
    public sealed class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(Encoding encoding)
        {
            this.Encoding = encoding;
        }

        public override Encoding Encoding { get; }
    }
}
