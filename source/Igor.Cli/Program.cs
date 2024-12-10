using Igor.Compiler;
using Igor.Text;
using System;
using TargetInvocationException = System.Reflection.TargetInvocationException;

namespace Igor.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            options.Parse(args);

            if (options.ShowUsage)
            {
                options.ShowHelp();
                Environment.Exit(1);
            }

            if (options.ShowVersion)
            {
                Console.WriteLine(Version.VersionString);
                Environment.Exit(1);
            }

            if (options.Initialisms != null)
            {
                NotationHelper.RegisterInitialisms(options.Initialisms.Split(','));
            }

            var compilerOutput = new ConsoleCompilerOutput();
            var scriptCompiler = new CSharpScriptCompiler(options.RoslynPath == null ? null : System.IO.Path.GetFullPath(options.RoslynPath));
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();

            try
            {
                var workspace = new Workspace(compilerOutput, scriptCompiler)
                {
                    Verbose = options.Verbose,
                    Bom = options.Bom,
                    AllowCreateDirectory = options.AllowCreateDirectory,
                    OverwriteReadOnly = options.OverwriteReadOnly,
                    OutputPath = options.OutputPath,
                    OutputFile = options.OutputFile,
                    StdOut = options.StdOut,
                    StdIn = options.StdIn,
                    DeleteEmpty = options.DeleteEmpty,
                    TargetVersion = options.TargetVersion,
                };

                workspace.AddSourcePaths(options.SourcePaths);
                workspace.AddSourcePaths(currentDirectory.Yield());
                workspace.AddImportPaths(options.ImportPaths);
                workspace.AddScriptPaths(options.ScriptPaths);
                workspace.AddScriptPaths(currentDirectory.Yield());
                workspace.AddLibPaths(options.LibPaths);
                workspace.AddLibPaths(currentDirectory.Yield());
                workspace.AddSourceFiles(options.SourceFiles);
                workspace.AddImportFiles(options.ImportFiles);
                workspace.AddScriptFiles(options.ScriptFiles);
                workspace.AddLibFiles(options.LibFiles);

                foreach (var attr in options.Attributes)
                    workspace.SetEnvironmentAttribute(attr.Key, AttributeValue.Parse(attr.Value));

                if (!workspace.LoadScriptLibs())
                    Environment.Exit(1);

                if (!workspace.CompileScripts())
                    Environment.Exit(1);

                Workspace.RegisterCommands();
                Workspace.RegisterTargets();

                ITarget target = null;
                if (options.Target != null)
                {
                    target = workspace.Target(options.Target);
                    if (target == null)
                    {
                        workspace.CompilerOutput.Error(Location.NoLocation, $"Unknown target '{options.Target}'", ProblemCode.UnknownTarget);
                        Environment.Exit(1);
                    }
                }

                IWorkspaceCommand command = null;
                if (options.Command != null)
                {
                    command = workspace.Command(options.Command);
                    if (command == null)
                    {
                        workspace.CompilerOutput.Error(Location.NoLocation, $"Unknown command '{options.Command}'", ProblemCode.UnknownCommand);
                        Environment.Exit(1);
                    }
                }

                if (command == null)
                    command = workspace.DefaultCommand;

                command.Run(workspace, target);

                if (workspace.CompilerOutput.HasErrors)
                {
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                var innerException = ex is TargetInvocationException targetInvocationException ? targetInvocationException.InnerException : ex;
                Location location;
                switch (innerException)
                {
                    case CodeException e: location = e.Location; break;
                    default: location = Location.NoLocation; break;
                }
                var message = options.Verbose ? innerException.ToString() : innerException.Message;
                compilerOutput.ReportMessage(CompilerMessageType.Error, location, message, ProblemCode.InternalError);
                Environment.Exit(1);
            }
        }
    }
}
