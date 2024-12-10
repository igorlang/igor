namespace Igor
{
    public class Location
    {
        public Location(string fileName, int line, int column = 1)
        {
            this.FileName = fileName;
            this.Line = line;
            this.Column = column;
        }

        public string FileName { get; }
        public int Line { get; }
        public int Column { get; }

        public static readonly Location NoLocation = new Location(null, 0, 0);

        public override string ToString()
        {
            return $"{FileName}({Line},{Column})";
        }
    }

    public interface ILocated
    {
        Location Location { get; }
    }

}
