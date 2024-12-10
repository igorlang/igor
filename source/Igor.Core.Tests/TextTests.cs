using NUnit.Framework;
using static System.Environment;

namespace Igor.Text.Tests
{
    [TestFixture]
    public class TextTests
    {
        [Test]
        public void Indent()
        {
            Assert.AreEqual("", "".Indent(4));
            Assert.AreEqual("    1\r\n    2", "1\r\n2".Indent(4));
        }

        [Test]
        public void Comment()
        {
            Assert.AreEqual("%%", "".Comment("%%"));
            Assert.AreEqual("%%1\r\n%%  2", "1\r\n  2".Comment("%%"));
        }

        [Test]
        public void FixLineBreaks()
        {
            Assert.AreEqual("hi\r\ntest".FixLineBreaks(), "hi\ntest".FixLineBreaks());
            Assert.AreEqual(NewLine + NewLine + NewLine, "\n\n\n".FixLineBreaks());
        }

        [Test]
        public void Tabulize()
        {
            const string source = @"class Test
{
void Nop()
{
 if (true)
{
	return;
 }
}
}
";

            const string expected = @"class Test
{
    void Nop()
    {
        if (true)
        {
            return;
        }
    }
}
";

            var actual = TextHelper.Tabulize(source, s => s.EndsWith("{"), s => s.StartsWith("}"), "    ");

            Assert.AreEqual(expected, actual.FixLineBreaks());
        }
    }
}
