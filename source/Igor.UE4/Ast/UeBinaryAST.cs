namespace Igor.UE4.AST
{
    public partial class RecordField
    {
        public string ueBinaryHeader => IsOptional ? $"{ueName}.IsSet()" : "true";
    }
}
