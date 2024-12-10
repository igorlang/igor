using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Mapper.Tests
{
    public class FlatSource
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class FlatDest
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class DeepSource
    {
        public List<FlatSource> List { get; set; }
        public FlatSource Object { get; set; }
    }

    public class DeepDest
    {
        public List<FlatDest> List { get; set; }
        public FlatDest Object { get; set; }
    }

    public class CrossSourceA
    {
        public int Value { get; set; }
        public CrossSourceB B { get; set; }
    }

    public class CrossSourceB
    {
        public int Value { get; set; }
        public CrossSourceA A { get; set; }
    }

    public class CrossDestA
    {
        public int Value { get; set; }
        public CrossDestB B { get; set; }
    }

    public class CrossDestB
    {
        public int Value { get; set; }
        public CrossDestA A { get; set; }
    }

    public class FlatMapper : MapperBase
    {
        public FlatMapper()
        {
            RegisterAuto<FlatSource, FlatDest>();
        }
    }

    public class DeepMapper : MapperBase
    {
        public DeepMapper()
        {
            RegisterAuto<FlatSource, FlatDest>();
            RegisterAuto<DeepSource, DeepDest>();
        }
    }

    public class CrossMapper : MapperBase
    {
        public CrossMapper()
        {
            RegisterAuto<CrossSourceA, CrossDestA>();
            RegisterAuto<CrossSourceB, CrossDestB>();
        }
    }

    [TestFixture]
    public class MapperTests
    {
        [Test]
        public void FlatMap()
        {
            var mapper = new FlatMapper();
            var source = new FlatSource() { Name = "test", Value = 2 };
            var dest = mapper.Map<FlatSource, FlatDest>(source);
            Assert.AreEqual("test", dest.Name);
            Assert.AreEqual(2, dest.Value);
        }

        [Test]
        public void DeepMap()
        {
            var mapper = new DeepMapper();
            var flatSource = new FlatSource() { Name = "test", Value = 2 };
            var deepSource = new DeepSource() { Object = flatSource, List = Enumerable.Range(0, 5).Select(i => new FlatSource() { Value = i }).ToList() };
            var dest = mapper.Map<DeepSource, DeepDest>(deepSource);
            Assert.AreEqual("test", dest.Object.Name);
            Assert.AreEqual(2, dest.Object.Value);
            Assert.AreEqual(3, dest.List[3].Value);
        }

        [Test]
        public void CrossMap()
        {
            var mapper = new CrossMapper();
            var a = new CrossSourceA() { Value = 1 };
            var b = new CrossSourceB() { Value = 2, A = a };
            a.B = b;
            var dest = mapper.Map<CrossSourceA, CrossDestA>(a);
            Assert.AreSame(dest, dest.B.A);
        }
    }
}
