using NUnit.Framework;
using System.Collections;

namespace Igor.TestRunner.Tests
{
    using static Expectations;

    [TestFixture]
    internal class ExpectationParserTests
    {
        public static IEnumerable Samples = new TestCaseData[] {
            new TestCaseData("Expect that 1 = 1").Returns(Int(1).Match(Int(1))),
            new TestCaseData("Expect that 1 = \"value\"").Returns(Int(1).Match(String("value"))),
            new TestCaseData("Expect that 1 = 1.1").Returns(Int(1).Match(Float(1.1))),
            new TestCaseData("Expect that 1 = false").Returns(Int(1).Match(Bool(false))),
            new TestCaseData("Expect that @name = 1").Returns(Name("name").Match(Int(1))),
            new TestCaseData("Expect that @name_underscore = 1").Returns(Name("name_underscore").Match(Int(1))),
            new TestCaseData("Expect that @name.value = 1").Returns(Name("name").Access("value").Match(Int(1))),
            new TestCaseData("Expect that @name.value1.value2 = 1").Returns(Name("name").Access("value1").Access("value2").Match(Int(1))),
            new TestCaseData("Expect that @name[0] = 1").Returns(Name("name").Access(0).Match(Int(1))),
        };

        [TestCaseSource(typeof(ExpectationParserTests), nameof(Samples))]
        public Expectation ParserTest(string input)
        {
            return new ExpectationParser().ParseExpectation(input, 0).Value;
        }
    }
}
