using System;
using NUnit.Framework;

namespace NUnitTestProject
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        [Category("MyCategory")]
        [Category("story:131")]
        [Category("tc:193")]
        public void OnePassingTest()
        {
            // perform test
        }

        [Test]
        [Category("tc:194")]
        public void OneFailingTest()
        {
            Console.WriteLine("This is a passing test");
            Assert.Fail("This is a simulated error");
        }

        [Test]
        [TestCase("foo", 1, Description = "First")]
        [TestCase("bar", 2, Description = "Second")]
        [TestCase("baz", 3, Description = "Third")]
        [Category("tc:195")]
        public void SampleDataDriven(string a, int b)
        {
            Console.WriteLine($"Testing: {a}, {b}");
        }
    }
}
