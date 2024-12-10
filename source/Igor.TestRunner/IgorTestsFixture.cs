using Igor.Text;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Linq;

namespace Igor.TestRunner
{
    [TestFixture]
    internal class IgorTestsFixture
    {
        private static string SolutionDir => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))));
        private static string TestsDir => Path.Combine(SolutionDir, "Igor.Tests");

        public static IEnumerable TestCases = TestCaseLoader.Load(TestsDir).Select(tc => new TestCaseData(tc).SetName(tc.Name).Returns(TestResultCode.Success));

        [TestCaseSource(typeof(IgorTestsFixture), nameof(TestCases))]
        public TestResultCode IgorTests(TestCase test)
        {
            var testResult = new TestRunner().Run(test);
            if (testResult.Errors.Any())
                Assert.Fail(testResult.Errors.JoinLines());
            return testResult.Code;
        }
    }
}
