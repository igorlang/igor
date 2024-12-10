using NUnit.Framework;
using System.Collections.Generic;

namespace Igor.Tests
{
    [TestFixture]
    public class ReflectionHelperTests
    {
        [Test]
        public void IsEnumerable()
        {
            Assert.IsTrue(ReflectionHelper.IsEnumerable(typeof(List<>)));
        }
    }
}
