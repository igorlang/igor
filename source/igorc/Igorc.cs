namespace Igor.Cli
{
    public class SingleExe
    {
        public static void Main(string[] args)
        {
            // Load target assemblies by referencing types
            new CSharp.Target();
            new Erlang.Target();
            new Go.Target();
            new Lua.Target();
            new Python.Target();
            new Elixir.Target();
            new Schema.SchemaTarget();
            new Schema.DiagramSchemaTarget();
            new Sql.SqlTarget();
            new JavaScript.Target();
            new TypeScript.Target();
            new UE4.Target();
            new Xsd.XsdTarget();
            new JsonSchema.JsonSchemaTarget();
            Program.Main(args);
        }
    }
}
