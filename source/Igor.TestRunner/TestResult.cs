using System.Collections.Generic;

namespace Igor.TestRunner
{
    public enum TestResultCode
    {
        Success,
        Fail,
        Abort,
    }

    public class TestResult
    {
        public TestResultCode Code { get; }
        public long Milliseconds { get; }
        public IReadOnlyList<string> Errors { get; }

        public TestResult(TestResultCode code, long milliseconds, IReadOnlyList<string> errors)
        {
            Code = code;
            Milliseconds = milliseconds;
            Errors = errors;
        }
    }
}
