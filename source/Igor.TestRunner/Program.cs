using System;
using System.Diagnostics;

namespace Igor.TestRunner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Igor.TestRunner.exe test-path");
            }
            else
            {
                var path = args[0];
                var cases = TestCaseLoader.Load(path);
                var sw = Stopwatch.StartNew();
                var success = 0;
                var fail = 0;
                var abort = 0;
                var total = 0;
                foreach (var test in cases)
                {
                    var result = new TestRunner().Run(test);
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = TestColor(result.Code);
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error);
                    }
                    Console.WriteLine($"{test.Name}: {result.Code} in {result.Milliseconds}ms");
                    Console.ForegroundColor = oldColor;
                    if (result.Code == TestResultCode.Success) success++;
                    if (result.Code == TestResultCode.Abort) abort++;
                    if (result.Code == TestResultCode.Fail) fail++;
                    total++;
                }
                sw.Stop();
                Console.WriteLine($"Total {total}: {success} success, {fail} fail, {abort} abort in {sw.ElapsedMilliseconds}ms");
            }
            Console.ReadLine();
        }

        private static ConsoleColor TestColor(TestResultCode code)
        {
            switch (code)
            {
                case TestResultCode.Success: return ConsoleColor.Green;
                case TestResultCode.Fail: return ConsoleColor.Red;
                case TestResultCode.Abort: return ConsoleColor.Magenta;
                default: return ConsoleColor.Gray;
            }
        }
    }
}
