using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyUnitTestProject
{
    [TestClass]
    public class OtherUnitTest
    {
        [TestMethod]
        public void SomeOtherTest()
        {
            Console.WriteLine("Testing some other test");
        }

        [DataTestMethod]
        [DataRow("foo", 1, DisplayName = "First")]
        [DataRow("bar", 2, DisplayName = "Second")]
        [DataRow("baz", 3, DisplayName = "Third")]
        public void SampleDataDriven(string a, int b)
        {
            Console.WriteLine($"Testing: {a}, {b}");
        }
    }
}
