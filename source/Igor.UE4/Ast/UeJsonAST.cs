namespace Igor.UE4.AST
{
    public partial class RecordField
    {
        public string ueWriteValue => IsTag ? ueTagGetter + "()" : ueName;
    }

    public partial class Form
    {
        public bool ueJsonCustomSerializer => Attribute(UeAttributes.JsonCustomSerializer, false);
    }
}
