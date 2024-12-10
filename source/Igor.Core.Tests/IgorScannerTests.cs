using System.Globalization;
using Igor.Compiler;
using Igor.Parser;
using NUnit.Framework;

namespace Igor.Core.Tests
{
    [TestFixture]
    public class IgorScannerTests
    {
        [Test]
        public void StringLiteralTest()
        {
            var source = @"""test""";
            var scanner = new IgorScanner(source, "test.txt", new TestCompilerOutput());
            Assert.IsTrue(StringLiteralTerm.Default.TryMatch(scanner, out var token));
            Assert.AreEqual("test", token.Text);
            Assert.IsTrue(scanner.IsEof());
        }

        [Test]
        public void StringEscapeTest()
        {
            var source = @"""\""test\""""";
            var scanner = new IgorScanner(source, "test.txt", new TestCompilerOutput());
            Assert.IsTrue(StringLiteralTerm.Default.TryMatch(scanner, out var token));
            Assert.AreEqual(@"""test""", token.Text);
            Assert.IsTrue(scanner.IsEof());
        }

        [Test]
        public void IdentifierTest()
        {
            var source = "test ";
            var scanner = new IgorScanner(source, "test.txt", new TestCompilerOutput());
            Assert.IsTrue(IdentifierTerm.MaybeInvalidIdentifier.TryMatch(scanner, out var token));
            Assert.AreEqual("test", token.Text);
            Assert.IsTrue(scanner.TestChar(' '));
        }

        [Test]
        public void BlockAnnotationTest()
        {
            var source = @"<#test
123#> ";
            var scanner = new IgorScanner(source, "test.txt", new TestCompilerOutput());
            Assert.IsTrue(BlockAnnotationTerm.Default.TryMatch(scanner, out var token));
            Assert.AreEqual("test 123", token.Text);
            Assert.IsTrue(scanner.TestChar(' '));
        }

        [Test]
        public void HexNumberTest()
        {
            var source = "0x100 and something else";
            var scanner = new IgorScanner(source, "test.txt", new TestCompilerOutput());
            Assert.IsTrue(NumberTerm.Default.TryMatch(scanner, out var token));
            Assert.AreEqual("0x100", token.Text);
            Assert.IsTrue(NumberTerm.TryParseIntegral(token, out var value, out var numberBase));
            Assert.AreEqual(0x100, value);
            Assert.AreEqual(NumberBase.Hex, numberBase);
        }
    }
}
