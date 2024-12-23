﻿using Igor.Erlang.Model;
using System.Text;

namespace Igor.Erlang.Render
{
    public class HrlRenderer : ErlRenderer
    {
        public string FileHeader =>
$@"Author: Igor compiler
Compiler version: {Version.HeaderVersionString}
DO NOT EDIT THIS FILE - it is machine generated";

        public void WriteHrl(ErlHeader hrl)
        {
            Comment(FileHeader, "%% ");
            ForEach(hrl.Records, WriteRecord, true);
            Blocks(hrl.Defines, true);
        }

        public void WriteRecord(ErlRecord record)
        {
            Comment(record.Comment, "% ");
            Line($"-record({record.Name}, {{");
            Indent();
            ForEach(record.Fields, f => WriteRecordField(f, record));
            Outdent();
            Line("}).");
        }

        public void WriteRecordField(ErlRecordField field, ErlRecord record)
        {
            Comment(field.Comment, "% ");
            var sb = new StringBuilder();
            sb.Append(field.Name);
            if (field.Default != null)
                sb.Append(" = " + field.Default);
            if (field.TypeSpec != null)
                sb.Append(" :: " + field.TypeSpec);
            if (field != record.Fields[record.Fields.Count - 1])
                sb.Append(",");
            Line(sb.ToString());
        }

        public static string Render(ErlHeader hrl)
        {
            var renderer = new HrlRenderer();
            renderer.WriteHrl(hrl);
            return renderer.Build();
        }
    }
}
