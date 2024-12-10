using Igor.Compiler;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igor.Cli
{
    internal class ProviderOptions : IProviderOptions
    {
        public string RoslynPath { get; }

        public ProviderOptions(string roslynPath) => RoslynPath = roslynPath;

        public string CompilerFullPath => System.IO.Path.Combine(RoslynPath, "csc.exe");
        public int CompilerServerTimeToLive => 60 * 15;
        public string CompilerVersion { get; } = "";
        public bool WarnAsError => false;
        public bool UseAspNetSettings => false;
        public IDictionary<string, string> AllOptions { get; } = new Dictionary<string, string>();
    }

    public class CSharpScriptCompiler : IScriptCompiler
    {
        private readonly string roslynPath;

        public CSharpScriptCompiler(string roslynPath)
        {
            this.roslynPath = roslynPath;
        }

        public static string AssemblyPath
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                return Uri.UnescapeDataString(uri.Path);
            }
        }

        public Assembly CompileFiles(IReadOnlyCollection<string> filenames, CompilerOutput compilerOutput)
        {
            var args = new Dictionary<string, string>() { ["CompilerVersion"] = "v4.0" };
            var provider = roslynPath == null ? new Microsoft.CSharp.CSharpCodeProvider(args) : new CSharpCodeProvider(new ProviderOptions(roslynPath));
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add(AssemblyPath);
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("mscorlib.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.IncludeDebugInformation = true;
            parameters.WarningLevel = 3;
            var results = provider.CompileAssemblyFromFile(parameters, filenames.ToArray());
            if (results.Errors.HasErrors || results.Errors.HasWarnings)
            {
                foreach (CompilerError error in results.Errors)
                {
                    var location = new Location(error.FileName, error.Line, error.Column);
                    var type = error.IsWarning ? CompilerMessageType.Warning : CompilerMessageType.Error;
                    compilerOutput.ReportMessage(type, location, error.ErrorText, error.ErrorNumber);
                }
            }
            if (results.Errors.HasErrors)
                return null;
            else
                return results.CompiledAssembly;
        }
    }
}
