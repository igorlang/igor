using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Cli
{
    public abstract class CliOption
    {
        public string Template { get; }

        public string Description { get; }

        public IReadOnlyList<string> Tags { get; }

        public CliOption(string template, string description)
        {
            Template = template;
            Description = description;

            if (template.StartsWith("-", StringComparison.Ordinal))
            {
                var tagsString = template.Split(' ')[0];
                Tags = tagsString.Split('|');
            }
            else
            {
                Tags = Array.Empty<string>();
            }
        }
    }

    public class CliFlag : CliOption
    {
        public Action Handler { get; }

        public CliFlag(string template, string description, Action handler) : base(template, description)
        {
            Handler = handler;
        }
    }

    public class CliString : CliOption
    {
        public Action<string> Handler { get; }

        public CliString(string template, string description, Action<string> handler) : base(template, description)
        {
            Handler = handler;
        }
    }

    public class CliArgument : CliOption
    {
        public Action<string> Handler { get; }

        public CliArgument(string template, string description, Action<string> handler) : base(template, description)
        {
            Handler = handler;
        }
    }

    public class CliException : Exception
    {
        public CliException(string message) : base(message)
        {
        }
    }

    public class Options
    {
        public const string AppName = "igorc.exe";

        public bool ShowUsage { get; private set; }

        public bool ShowVersion { get; private set; }

        public List<string> SourcePaths { get; } = new List<string>();

        public List<string> ImportFiles { get; } = new List<string>();

        public List<string> ImportPaths { get; } = new List<string>();

        public List<string> ScriptFiles { get; } = new List<string>();

        public List<string> ScriptPaths { get; } = new List<string>();

        public List<string> LibFiles { get; } = new List<string>();

        public List<string> LibPaths { get; } = new List<string>();

        public string RoslynPath { get; private set; }

        public bool Verbose { get; private set; }

        public string Target { get; private set; }

        public System.Version TargetVersion { get; private set; }

        public string Command { get; private set; }

        public string OutputPath { get; private set; } = ".";

        public string OutputFile { get; private set; }
        
        public bool StdOut { get; private set; }
        
        public bool StdIn { get; private set; }

        public bool AllowCreateDirectory { get; private set; }

        public bool OverwriteReadOnly { get; private set; }

        public bool DeleteEmpty { get; private set; }

        public bool Bom { get; private set; }


        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public string Initialisms { get; private set; }

        public List<string> SourceFiles { get; } = new List<string>();


        private readonly List<CliOption> options;

        public Options()
        {
            options = new List<CliOption>
            {
                new CliFlag("-h|-help", "Display this help and exit", () => ShowUsage = true),
                new CliFlag("-V|-version", "Show version and exit", () => ShowVersion = true),
                new CliString("-p|-path <STRING>", "Source path", SourcePaths.Add),
                new CliString("-i|-import <STRING>", "Import file", ImportFiles.Add),
                new CliString("-I|-import-path <STRING>", "Import path", ImportPaths.Add),
                new CliString("-x|-script <STRING>", "Use script", ScriptFiles.Add),
                new CliString("-X|-script-path <STRING>", "Script path", ScriptPaths.Add),
                new CliString("-lib <STRING>", "Use compiled script DLL", LibFiles.Add),
                new CliString("-lib-path <STRING>", "Compiled script DLL path", LibPaths.Add),
                new CliString("-roslyn <STRING>", "Roslyn path", s => RoslynPath = s),
                new CliString("-t|-target <STRING>", "Target", s => Target = s),
                new CliFlag("-cs|-csharp", "Same as -t csharp", () => Target = "csharp"),
                new CliFlag("-erl|-erlang", "Same as -t erlang", () => Target = "erlang"),
                new CliFlag("-schema", "Same as -t schema", () => Target = "schema"),
                new CliFlag("-diagram_schema", "Same as -t diagram_schema", () => Target = "diagram"),
                new CliFlag("-xsd", "Same as -t xsd", () => Target = "xsd"),
                new CliString("-m|-command <STRING>", "Command", s => Command = s),
                new CliFlag("-stdin", "Input from stdin", () => StdIn = true),
                new CliFlag("-O|-stdout", "Output to stdout", () => StdOut = true),
                new CliString("-o|-output <STRING>", "Output path", s => OutputPath = s),
                new CliString("-output-file <STRING>", "Output file", s => OutputFile = s),
                new CliFlag("-d|-delempty", "Delete empty files", () => DeleteEmpty = true),
                new CliFlag("-mkdir", "Allow to create output directory", () => AllowCreateDirectory = true),
                new CliFlag("-w", "Overwrite readonly files", () => OverwriteReadOnly = true),
                new CliFlag("-v|-verbose", "Verbose output", () => Verbose = true),
                new CliFlag("-bom", "Write BOM header", () => Bom = true),
                new CliString("-set", "Set attribute value", SetAttribute),
                new CliString("-target-version", "Target version", SetTargetVersion),
                new CliString("-initialisms", "Comma separated initialisms", s => Initialisms = s),
                new CliArgument("<STRING>", "Source files", SourceFiles.Add)
            };
        }

        public void Parse(string[] args)
        {
            var optionsDict = options.SelectMany(opt => opt.Tags.Select(tag => (tag, opt))).ToDictionary(tuple => tuple.tag, tuple => tuple.opt);
            var argument = options.OfType<CliArgument>().Single();
            try
            {
                CliString currentOption = null;
                string currentTag = null;
                foreach (var arg in args)
                {
                    if (arg.StartsWith("-", StringComparison.Ordinal))
                    {
                        if (currentTag != null)
                            throw new CliException($"missing argument to '{currentTag}'");
                        if (optionsDict.TryGetValue(arg, out var option))
                        {
                            if (option is CliFlag flag)
                            {
                                flag.Handler();
                            }
                            else
                            {
                                currentOption = (CliString)option;
                                currentTag = arg;
                            }
                        }
                        else
                        {
                            throw new CliException($"invalid options '{arg}'");
                        }
                    }
                    else if (currentOption != null)
                    {
                        currentOption.Handler(arg);
                        currentOption = null;
                        currentTag = null;
                    }
                    else
                    {
                        argument.Handler(arg);
                    }
                }
                if (currentTag != null)
                    throw new CliException($"missing argument to '{currentTag}'");
            }
            catch (CliException exception)
            {
                Console.Error.WriteLine($"{AppName}: {exception.Message}");
                Console.Error.WriteLine($"Try '{AppName} -help' for more information.");
                Environment.Exit(1);
            }
        }

        public void ShowHelp()
        {
            var maxTemplate = options.Max(opt => opt.Template.Length);
            foreach (var option in options)
            {
                var padding = new string(' ', maxTemplate - option.Template.Length);
                Console.WriteLine($"    {option.Template}{padding}    {option.Description}");
            }
        }

        private void SetTargetVersion(string value)
        {
            try
            {
                TargetVersion = System.Version.Parse(value);
            }
            catch
            {
                throw new CliException($"failed to parse target version '{value}'");
            }
        }

        private void SetAttribute(string value)
        {
            try
            {
                var (attrName, attrValue) = ParseAttribute(value);
                Attributes[attrName] = attrValue;
            }
            catch
            {
                throw new CliException($"failed to parse attribute '{value}'");
            }
        }

        private static (string attrName, string attrValue) ParseAttribute(string str)
        {
            var sepIndex = str.IndexOf("=", StringComparison.Ordinal);

            if (sepIndex < 0)
            {
                return (str, "true");
            }
            else
            {
                var name = str.Substring(0, sepIndex);
                var value = str.Substring(sepIndex + 1);
                if (value.Length >= 2 && value[0] == '\'' && value[value.Length - 1] == '\'')
                {
                    value = "\"" + value.Substring(1, value.Length - 2) + "\"";
                }
                return (name, value);
            }
        }
    }
}
