using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MsTestTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [TestCategory("MyCategory")]
        [TestCategory("tc:143")]
        [TestCategory("story:131")]
        public void OnePassingTest()
        {
            // perform test
        }

        [TestMethod]
        [TestCategory("tc:144")]
        public void OneFailingTest()
        {
            Console.WriteLine("This is a passing test");
            Assert.Fail("This is a simulated error");
        }

        [TestMethod]
        [DataRow("foo", 1, DisplayName = "First")]
        [DataRow("bar", 2, DisplayName = "Second")]
        [DataRow("baz", 3, DisplayName = "Third")]
        [TestCategory("tc:145")]
        public void SampleDataDriven(string a, int b)
        {
            Console.WriteLine($"Testing: {a}, {b}");
        }
    }
}
