namespace Igor.UE4.Model
{
    public class UeTypedef
    {
        public string Name { get; }
        public string Declaration { get; }
        public string Comment { get; set; }

        internal UeTypedef(string name, string declaration)
        {
            this.Name = name;
            this.Declaration = declaration;
        }
    }
}
