using NUnit.Framework;

namespace Igor.Text.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void RemoveDoubleSpaces()
        {
            Assert.AreEqual(StringHelper.RemoveDoubleSpaces("    this is  a    test  "), "    this is a test ");
        }

        private void AssertLongestCommonPrefix(string expectedPrefix, string expectedTail1, string expectedTail2, string str1, string str2)
        {
            StringHelper.LongestCommonPrefix(str1, str2, out string prefix, out string tail1, out string tail2);
            Assert.AreEqual(expectedPrefix, prefix);
            Assert.AreEqual(expectedTail1, tail1);
            Assert.AreEqual(expectedTail2, tail2);
        }

        private void AssertLongestCommonPrefix(string expectedPrefix, string expectedTail1, string expectedTail2, string str1, string str2, char sep)
        {
            StringHelper.LongestCommonPrefix(str1, str2, sep, out string prefix, out string tail1, out string tail2);
            Assert.AreEqual(expectedPrefix, prefix);
            Assert.AreEqual(expectedTail1, tail1);
            Assert.AreEqual(expectedTail2, tail2);
        }

        [Test]
        public void LongestCommonPrefix()
        {
            AssertLongestCommonPrefix("", "", "", null, null);
            AssertLongestCommonPrefix("test", "1", "2", "test1", "test2");
            AssertLongestCommonPrefix("test", "123", "2", "test123", "test2");
            AssertLongestCommonPrefix("", "test", "hi", "test", "hi");
            AssertLongestCommonPrefix("test", "", "2", "test", "test2");
            AssertLongestCommonPrefix("test", "1", "", "test1", "test");
            AssertLongestCommonPrefix("", "", "test", "", "test");
            AssertLongestCommonPrefix("", "test", "", "test", "");
        }

        [Test]
        public void LongestCommonPrefixSep()
        {
            AssertLongestCommonPrefix("", "", "", null, null, '.');
            AssertLongestCommonPrefix("System", "Linq", "Collections", "System.Linq", "System.Collections", '.');
            AssertLongestCommonPrefix("System", "Test1", "Test2", "System.Test1", "System.Test2", '.');
            AssertLongestCommonPrefix("", "System", "Igor", "System", "Igor", '.');
            AssertLongestCommonPrefix("System", "", "Linq", "System", "System.Linq", '.');
            AssertLongestCommonPrefix("System", "Linq", "", "System.Linq", "System", '.');
            AssertLongestCommonPrefix("", "", "System", "", "System", '.');
            AssertLongestCommonPrefix("", "System", "", "System", "", '.');
        }
    }
}
