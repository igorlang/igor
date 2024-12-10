using NUnit.Framework;

namespace Igor.Parser.Tests
{
    [TestFixture]
    public class LineIndexTests
    {
        [Test]
        public void LineIndexTest()
        {
            var source = @"test
123";
            var lineIndex = new LineIndex(source);
            var position = source.IndexOf('1');
            lineIndex.GetLineAndColumn(position, out var line, out var column);
            Assert.AreEqual(line, 2);
            Assert.AreEqual(column, 1);
        }
    }
}
