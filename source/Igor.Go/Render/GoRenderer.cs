using Igor.Text;

namespace Igor.Go
{
    public static class GoRenderer
    {
        public static Renderer Create() => new Renderer { Tab = "\t", RemoveDoubleSpaces = false };
    }
}
