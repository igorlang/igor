using System;

namespace Igor.Compiler
{
    public interface IWorkspaceCommand
    {
        string Name { get; }
        
        void Run(Workspace workspace, ITarget target);
    }

    public class CompileCommand : IWorkspaceCommand
    {
        public string Name => "compile";

        public void Run(Workspace workspace, ITarget target)
        {
            if (workspace.SourceFiles.Count == 0)
            {
                workspace.CompilerOutput.Log("No source files, exiting");
                Environment.Exit(1);
            }

            workspace.Parse();
            workspace.Compile();
            workspace.ValidateAttributes(target);

            if (workspace.CompilerOutput.HasErrors)
            {
                Environment.Exit(1);
            }

            if (target != null)
            {
                if (!workspace.Generate(target))
                    Environment.Exit(1);
            }
        }
    }

    public class ListCommandsWorkspaceCommand : IWorkspaceCommand
    {
        public string Name => "list-commands";

        public void Run(Workspace workspace, ITarget target)
        {
            foreach (var t in Workspace.Commands)
            {
                if (t.Name != null)
                    Console.WriteLine(t.Name);
            }
        }
    }

    public class ListTargetsWorkspaceCommand : IWorkspaceCommand
    {
        public string Name => "list-targets";

        public void Run(Workspace workspace, ITarget target)
        {
            foreach (var t in Workspace.Targets)
            {
                Console.WriteLine(t.Name);
            }
        }
    }

    public class ListAttributesWorkspaceCommand : IWorkspaceCommand
    {
        public string Name => "list-attributes";

        public void Run(Workspace workspace, ITarget target)
        {
            var attributes = workspace.GetAllAttributes(target);
            foreach (var attr in attributes)
            {
                Console.WriteLine(attr.Name);
            }
        }
    }
}
