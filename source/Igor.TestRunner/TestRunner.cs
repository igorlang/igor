using Igor.Compiler;
using Igor.Core.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Igor.TestRunner
{
    public class TestRunner
    {
        public TestResult Run(TestCase @case)
        {
            var sw = Stopwatch.StartNew();
            TestResultCode code;
            var errors = new List<string>();
            try
            {
                code = Execute(@case, errors);
            }
            catch (Exception exception)
            {
                code = TestResultCode.Abort;
                errors.Add(exception.ToString());
            }
            sw.Stop();
            return new TestResult(code, sw.ElapsedMilliseconds, errors);
        }

        private TestResultCode Execute(TestCase test, List<string> errors)
        {
            var compilerOutput = new TestCompilerOutput();
            var parser = new ExpectationParser();
            var workspace = new Workspace(compilerOutput, null);
            workspace.AddSourcePaths(test.Folder.Yield());
            workspace.AddSourceFiles(test.FileNames);
            workspace.Parse();
            IReadOnlyList<Module> modules = new List<Module>();
            if (workspace.Compile())
            {
                workspace.ValidateAttributes(null);
                modules = workspace.GetModules();
            }

            var result = TestResultCode.Success;
            foreach (var file in test.FileNames)
            {
                var fullFileName = System.IO.Path.Combine(test.Folder, file);
                var source = System.IO.File.ReadAllText(fullFileName);
                var module = modules.FirstOrDefault(mod => mod.Location.FileName == fullFileName);
                try
                {
                    var expectations = parser.Parse(source);
                    var messages = compilerOutput.Messages.Where(m => m.Location.FileName == fullFileName);
                    var testResult = CheckErrorExpectations(file, messages, expectations, errors) && CheckMatchExpectations(module, expectations, errors) && CheckUnmetExpectations(file, expectations, errors);
                    if (result == TestResultCode.Success && !testResult)
                        result = TestResultCode.Fail;
                }
                catch (ExpectationParserException parserException)
                {
                    errors.Add($"{System.IO.Path.GetFileName(file)}:{parserException.Line} Failed to parse expectation {parserException.ExpectationString}");
                    result = TestResultCode.Abort;
                }
            }
            return result;
        }

        private bool CheckErrorSpec(CompilerMessage message, string spec)
        {
            var m = Regex.Match(message.Text, spec, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            return m.Success;
        }

        private bool CheckErrorExpectations(string file, IEnumerable<CompilerMessage> messages, List<Expectation> expectations, List<string> errors)
        {
            var met = new List<Expectation>();
            foreach (var message in messages)
            {
                var line = message.Location.Line;
                if (message.Type == CompilerMessageType.Error)
                {
                    var exp = expectations.OfType<ErrorExpectation>().FirstOrDefault(e => (e.Line == line || e.Line == line - 1) && CheckErrorSpec(message, e.Spec));
                    if (exp == null)
                    {
                        errors.Add($"Unexpected error \"{message.Text}\" at {file}:{line}");
                        return false;
                    }
                    else
                    {
                        if (!met.Contains(exp))
                            met.Add(exp);
                    }
                }
            }
            foreach (var e in met)
                expectations.Remove(e);
            return true;
        }

        private bool CheckMatchExpectations(Module module, List<Expectation> expectations, List<string> errors)
        {
            if (module == null)
                return true;
            var matchExpectations = expectations.OfType<MatchExpectation>().ToList();
            foreach (var exp in matchExpectations)
            {
                var left = exp.Left.Evaluate(module);
                var right = exp.Right.Evaluate(module);
                if (SmartEquals(left, right))
                {
                    expectations.Remove(exp);
                }
                else
                {
                    errors.Add($"Unmet expectation: {exp}");
                    return false;
                }
            }
            return true;
        }

        private bool SmartEquals(object obj1, object obj2)
        {
            if (obj1 == null)
                return obj2 == null;
            else if (obj2 == null)
                return false;
            else if ((obj1 is IConvertible) && (obj2 is IConvertible))
            {
                var converted2 = Convert.ChangeType(obj2, obj1.GetType());
                return obj1.Equals(converted2);
            }
            else
                return Equals(obj1, obj2);
        }

        private bool CheckUnmetExpectations(string file, List<Expectation> expectations, List<string> errors)
        {
            if (expectations.Any())
            {
                errors.Add($"{file}: Unmet expectation: {expectations.First()}");
                return false;
            }
            else
                return true;
        }
    }
}
