using Igor.Declarations;
using Igor.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Igor.Compiler
{
    public class IgorSourceFile
    {
        public string FileName { get; }

        public CompilationUnit CompilationUnit { get; }

        public IgorSourceFile(string filename, CompilationUnit compilationUnit)
        {
            FileName = filename;
            CompilationUnit = compilationUnit;
        }
    }

    public class Workspace
    {
        public const string StdInFileName = "&0";

        public CompilerOutput CompilerOutput { get; }

        public IReadOnlyList<string> SourcePaths => sourcePaths;
        public IReadOnlyList<string> ImportPaths => importPaths;
        public IReadOnlyList<string> ScriptPaths => scriptFiles;
        public IReadOnlyList<string> SourceFiles => sourceFiles;
        public IReadOnlyList<string> ImportFiles => importFiles;
        public IReadOnlyList<string> ScriptFiles => scriptFiles;

        public IScriptCompiler ScriptCompiler { get; }

        public bool Verbose { get; set; }
        public bool Bom { get; set; }
        public bool AllowCreateDirectory { get; set; }
        public bool OverwriteReadOnly { get; set; }
        public string OutputPath { get; set; }
        public string OutputFile { get; set; }
        public bool StdOut { get; set; }

        public bool StdIn
        {
            get => sourceFiles.Contains(StdInFileName);
            set
            {
                if (value && !sourceFiles.Contains(StdInFileName))
                    sourceFiles.Add(StdInFileName);
                if (!value)
                    sourceFiles.Remove(StdInFileName);
            }
        }


        public bool DeleteEmpty { get; set; }
        public System.Version TargetVersion { get; set; }

        private readonly List<string> sourcePaths = new List<string>();
        private readonly List<string> importPaths = new List<string>();
        private readonly List<string> scriptPaths = new List<string>();
        private readonly List<string> scriptFiles = new List<string>();
        private readonly List<string> libPaths = new List<string>();
        private readonly List<string> libFiles = new List<string>();
        private readonly List<string> sourceFiles = new List<string>();
        private readonly List<string> importFiles = new List<string>();

        private readonly List<Assembly> scripts = new List<Assembly>();
        private readonly Dictionary<string, IgorSourceFile> parsedFiles = new Dictionary<string, IgorSourceFile>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, AttributeValue> environmentAttributes = new Dictionary<string, AttributeValue>();

        public IReadOnlyDictionary<string, AttributeValue> EnvironmentAttributes => environmentAttributes;

        private static readonly List<ITarget> targets = new List<ITarget>();
        private static readonly List<IWorkspaceCommand> commands = new List<IWorkspaceCommand>();

        public static IReadOnlyList<ITarget> Targets => targets;
        public static IReadOnlyList<IWorkspaceCommand> Commands => commands;

        public IReadOnlyList<Declarations.Module> GetModuleDeclarations() => sourceFiles.SelectMany(f => (parsedFiles[f].CompilationUnit).Modules).ToList();

        public IReadOnlyList<Core.AST.Module> GetModules()
        {
            var mapper = new Core.AST.AstMapper();
            return GetModuleDeclarations().Select(mapper.Map<Declarations.Module, Core.AST.Module>).ToList();
        }

        public Workspace(CompilerOutput compilerOutput, IScriptCompiler scriptCompiler)
        {
            CompilerOutput = compilerOutput;
            ScriptCompiler = scriptCompiler;
        }

        public void AddSourcePaths(IEnumerable<string> paths) => sourcePaths.AddRange(paths);

        public void AddImportPaths(IEnumerable<string> paths) => importPaths.AddRange(paths);

        public void AddScriptPaths(IEnumerable<string> paths) => scriptPaths.AddRange(paths);

        public void AddScriptFiles(IEnumerable<string> fileNames) => AddFiles(scriptFiles, fileNames, scriptPaths, "Script file not found", ProblemCode.ScriptFileNotFound);

        public void AddLibPaths(IEnumerable<string> paths) => libPaths.AddRange(paths);

        public void AddLibFiles(IEnumerable<string> fileNames) => AddFiles(libFiles, fileNames, libPaths, "Lib file not found", ProblemCode.ScriptFileNotFound);

        public void AddSourceFiles(IEnumerable<string> sourceFileNames) => AddFiles(sourceFiles, sourceFileNames, sourcePaths, "Source file not found", ProblemCode.SourceFileNotFound);

        public void AddImportFiles(IEnumerable<string> importFileNames) => AddFiles(importFiles, importFileNames, importPaths.Concat(sourcePaths), "Source file not found", ProblemCode.SourceFileNotFound);

        public void SetEnvironmentAttribute(string name, AttributeValue value) => environmentAttributes[name] = value;

        public ITarget Target(string language)
        {
            return targets.FirstOrDefault(t => t.Name == language);
        }

        public IWorkspaceCommand Command(string commandName)
        {
            return commands.FirstOrDefault(c => c.Name == commandName);
        }

        public IWorkspaceCommand DefaultCommand => new CompileCommand();

        /// <summary>
        /// Get a single attribute value set for this AST statement or inherited using inheritance type defined by attribute descriptor
        /// (or default value if value is unset in Igor source or environment)
        /// </summary>
        /// <typeparam name="T">Attribute value type argument</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Attribute value or default</returns>
        public T EnvironmentAttribute<T>(AttributeDescriptor<T> attribute, T defaultValue)
        {
            if (environmentAttributes.TryGetValue(attribute.Name, out var attributeValue) && attribute.Convert(attributeValue, out T val))
                return val;
            else
                return defaultValue;
        }

        private void AddFiles(List<string> targetFiles, IEnumerable<string> addFiles, IEnumerable<string> searchPaths, string notFoundErrorMessage, ProblemCode notFoundProblemCode)
        {
            var filesNotFound = new List<string>();
            var directoriesNotFound = new List<string>();
            foreach (var file in FileSearch.FileList(addFiles, searchPaths, filesNotFound, directoriesNotFound))
            {
                if (!targetFiles.Contains(file, StringComparer.InvariantCultureIgnoreCase))
                    targetFiles.Add(file);
            }

            foreach (var message in directoriesNotFound)
            {
                CompilerOutput.Error(new Location(null, 0, 0), message, notFoundProblemCode);
            }
            foreach (var path in filesNotFound)
            {
                CompilerOutput.Error(new Location(path, 0, 0), notFoundErrorMessage, notFoundProblemCode);
            }
        }

        public void Parse()
        {
            foreach (var filename in sourceFiles.Concat(importFiles).Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                CompilerOutput.Log($"Processing {filename}..");
                var file = ParseFile(filename);
                parsedFiles.Add(filename, file);
            }
        }

        public IgorSourceFile ParseFile(string filename)
        {
            var source = ReadFile(filename);
            var parser = new IgorParser(source, filename, CompilerOutput);
            var compilationUnit = parser.ParseCompilationUnit();
            return new IgorSourceFile(filename, compilationUnit);
        }

        public bool Compile()
        {
            var context = new CompileContext(CompilerOutput);
            context.RegisterBuiltInSymbols();
            foreach (CompileStage stage in typeof(CompileStage).GetEnumValues())
            {
                foreach (var file in parsedFiles.Values)
                {
                    file.CompilationUnit.Compile(context, stage);
                }
            }

            return !CompilerOutput.HasErrors;
        }

        public IReadOnlyList<AttributeDescriptor> GetAllAttributes(ITarget target)
        {
            var supportedAttributes = CoreAttributes.AllAttributes.ToList();
            if (target != null)
            {
                supportedAttributes.AddRange(target.SupportedAttributes);
            }
            var customClasses = scripts.SelectMany(a => ReflectionHelper.CollectTypesWithAttribute(a, typeof(CustomAttributesAttribute)));
            foreach (var customClass in customClasses)
            {
                foreach (var field in customClass.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (typeof(AttributeDescriptor).IsAssignableFrom(field.FieldType))
                    {
                        supportedAttributes.Add((AttributeDescriptor)field.GetValue(null));
                    }
                }
            }
            return supportedAttributes;
        }

        public bool ValidateAttributes(ITarget target)
        {
            var supportedAttributes = GetAllAttributes(target);
            var attrs = supportedAttributes.GroupBy(attr => attr.Name).ToDictionary(g => g.Key, g => g.First());
            bool result = true;
            foreach (var file in parsedFiles.Values)
            {
                foreach (var mod in file.CompilationUnit.Modules)
                {
                    if (!ValidateAttributes(mod, attrs, target?.Name))
                        result = false;
                }
            }
            return result;
        }

        private bool ValidateAttributes(IAttributeHost host, IReadOnlyDictionary<string, AttributeDescriptor> supportedAttributes, string target)
        {
            bool result = true;
            foreach (var attr in host.Attributes)
            {
                var attrTarget = attr.Target;
                var isKnownTarget = attrTarget == "*" || attrTarget == target;
                if (isKnownTarget)
                {
                    var name = attr.Name;
                    if (supportedAttributes.TryGetValue(name, out var descriptor))
                    {
                        if (descriptor.DeprecationMessage != null)
                        {
                            CompilerOutput.ReportMessage(CompilerMessageType.Warning, attr.Location, $"Deprecated attribute {name}. {descriptor.DeprecationMessage}", ProblemCode.DeprecatedAttribute);
                        }
                        if (!descriptor.ValidateValue(attr.Value))
                        {
                            CompilerOutput.Warning(attr.Location, $"Invalid attribute value {name}={attr.Value}. Expected: {descriptor.SupportedValues}.", ProblemCode.InvalidAttributeValue);
                        }
                    }
                    else
                    {
                        CompilerOutput.Warning(attr.Location, $"Unknown attribute {name}", ProblemCode.UnknownAttribute);
                        result = false;
                    }
                }
            }

            foreach (var nestedHost in host.NestedHosts)
            {
                if (!ValidateAttributes(nestedHost, supportedAttributes, target))
                    result = false;
            }
            return result;
        }

        public bool Generate(ITarget target)
        {
            if (Verbose)
                CompilerOutput.Log($"Generating target: {target.Name}");

            Context.Instance.Verbose = Verbose;
            Context.Instance.Bom = Bom;
            Context.Instance.TargetVersion = TargetVersion ?? target.DefaultVersion;
            Context.Instance.Attributes = EnvironmentAttributes;
            Context.Instance.CompilerOutput = CompilerOutput;
            Context.Instance.Target = target.Name;

            if (Verbose)
            {
                foreach (var attr in Context.Instance.Attributes)
                {
                    CompilerOutput.Log($"{attr.Key} = {attr.Value}");
                }
            }

            var modules = GetModuleDeclarations();
            IReadOnlyCollection<TargetFile> files = null;
            try
            {
                files = target.Generate(modules, scripts);
            }
            catch (Exception exception)
            {
                bool isHandled = false;
                var stacktrace = new System.Diagnostics.StackTrace(exception, true);
                foreach (var frame in stacktrace.GetFrames())
                {
                    var filename = frame.GetFileName();
                    if (scriptFiles.Contains(filename, StringComparer.InvariantCultureIgnoreCase))
                    {
                        var location = new Location(filename, frame.GetFileLineNumber(), frame.GetFileColumnNumber());
                        CompilerOutput.Error(location, exception.ToString(), ProblemCode.ScriptRuntimeError);
                        isHandled = true;
                        break;
                    }
                }
                if (!isHandled)
                    throw;
            }

            if (CompilerOutput.HasErrors)
                return false;

            foreach (var file in files)
                SaveFile(file);

            return true;
        }

        public bool CompileScripts()
        {
            bool result = true;
            if (scriptFiles.Any())
            {
                foreach (var script in scriptFiles)
                {
                    CompilerOutput.Log($"Compiling {script}..");
                }

                var assembly = ScriptCompiler.CompileFiles(scriptFiles, CompilerOutput);
                if (assembly == null)
                    result = false;
                else
                    scripts.Add(assembly);
            }

            return result;
        }

        public bool LoadScriptLibs()
        {
            bool result = true;
            foreach (var lib in libFiles)
            {
                CompilerOutput.Log($"Loading {lib}..");
                var assembly = Assembly.LoadFile(lib);
                scripts.Add(assembly);
            }
            return result;
        }

        public string ReadFile(string filename)
        {
            return filename == StdInFileName ? Console.In.ReadToEnd() : File.ReadAllText(filename);
        }

        public void SaveFile(TargetFile file)
        {
            if (StdOut)
            {
                if (!(file.Empty && DeleteEmpty))
                {
                    Console.Write(Text.TextHelper.FixLineBreaks(file.Text));
                }

                return;
            }

            var targetFile = Path.Combine(OutputPath, OutputFile ?? file.Name);
            try
            {
                if (file.Empty && DeleteEmpty)
                {
                    File.Delete(targetFile);
                }
                else if (!file.Empty)
                {
                    if (AllowCreateDirectory)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                    }
                    var text1 = Text.TextHelper.FixLineBreaks(file.Text);
                    var oldText = ReadAllText(targetFile);
                    if (oldText != text1)
                    {
                        if (File.Exists(targetFile))
                        {
                            var fileInfo = new FileInfo(targetFile);
                            if (fileInfo.IsReadOnly)
                            {
                                if (OverwriteReadOnly)
                                    fileInfo.IsReadOnly = false;
                                else
                                    Context.Instance.CompilerOutput.Warning(Location.NoLocation, $"File {targetFile} is readonly. Use -w command line switch to allow overwrite readonly files.", ProblemCode.TargetFileIsReadonly);
                            }
                        }
                        File.WriteAllText(targetFile, text1, new UTF8Encoding(Context.Instance.Bom));
                    }
                }
            }
            catch
            {
                Context.Instance.CompilerOutput.ReportMessage(CompilerMessageType.Error, Location.NoLocation, $"Failed to write file {targetFile}", ProblemCode.FailedToWriteFile);
                throw;
            }
        }

        private string ReadAllText(string fileName)
        {
            try
            {
                return File.ReadAllText(fileName);
            }
            catch
            {
                return null;
            }
        }

        public static void RegisterTargets()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            targets.AddRange(assemblies.SelectMany(ReflectionHelper.CollectTypes<ITarget>));
        }

        public static void RegisterCommands()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            commands.AddRange(assemblies.SelectMany(ReflectionHelper.CollectTypes<IWorkspaceCommand>));
        }
    }
}
