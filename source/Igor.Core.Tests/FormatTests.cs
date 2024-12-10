using NUnit.Framework;

namespace Igor.Text.Tests
{
    [TestFixture]
    public class FormatTests
    {
        [Test]
        public void FormatCamel()
        {
            Assert.AreEqual("123", "123".Format(Notation.UpperCamel));
            Assert.AreEqual("", "".Format(Notation.UpperCamel));
            Assert.AreEqual("Hello", "hello".Format(Notation.UpperCamel));
            Assert.AreEqual("Hello", "HELLO".Format(Notation.UpperCamel));
            Assert.AreEqual("IMyInterface", "IMyInterface".Format(Notation.UpperCamel));
            Assert.AreEqual("IInterface22", "IInterface22".Format(Notation.UpperCamel));
            Assert.AreEqual("One2Three", "One2Three".Format(Notation.UpperCamel));
            Assert.AreEqual("RequestApplySp", "RequestApplySP".Format(Notation.UpperCamel));
        }

        [Test]
        public void FormatUnderscore()
        {
            Assert.AreEqual("123", "123".Format(Notation.LowerUnderscore));
            Assert.AreEqual("", "".Format(Notation.LowerUnderscore));
            Assert.AreEqual("hello", "hello".Format(Notation.LowerUnderscore));
            Assert.AreEqual("hello", "HELLO".Format(Notation.LowerUnderscore));
            Assert.AreEqual("i_my_interface", "IMyInterface".Format(Notation.LowerUnderscore));
            Assert.AreEqual("one2_three", "One2Three".Format(Notation.LowerUnderscore));
            Assert.AreEqual("request_apply_sp", "RequestApplySP".Format(Notation.LowerUnderscore));
        }

        [Test]
        public void FormatTitle()
        {
            Assert.AreEqual("123", "123".Format(Notation.Title));
            Assert.AreEqual("", "".Format(Notation.Title));
            Assert.AreEqual("Hello", "hello".Format(Notation.Title));
            Assert.AreEqual("Hello", "HELLO".Format(Notation.Title));
            Assert.AreEqual("I My Interface", "IMyInterface".Format(Notation.Title));
            Assert.AreEqual("One2 Three", "One2Three".Format(Notation.Title));
            Assert.AreEqual("Request Apply Sp", "RequestApplySP".Format(Notation.Title));
        }

        [Test]
        public void FormatFirstLetterLastWord()
        {
            Assert.AreEqual("t", "Test".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("", "".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("h", "hello".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("h", "Hello".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("i", "IMyInterface".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("i", "IMyInterface22".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("t", "One2Three".Format(Notation.FirstLetterLastWord));
            Assert.AreEqual("s", "RequestApplySP".Format(Notation.FirstLetterLastWord));
        }
    }
}
