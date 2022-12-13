using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyUnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [TestCategory("MyCategory")]
        [TestCategory("OtherCategory")]
        [TestCategory("tc:150")]
        [TestCategory("story:131")]
        public void OnePassingTest()
        {
            // Step: This is the first step
            Console.WriteLine("This is a passing test");
            // Step: this is the second step
            Console.WriteLine("Does something here");
            // Step: and this is the third step
            Console.WriteLine("And here as well");
            // Assertion: And this is what we verify
            Console.WriteLine("And check");
        }

        [TestMethod]
        public void OneFailingTest()
        {
            Console.WriteLine("This is a passing test");
            Assert.Fail("This is a simulated error");
        }
    }
}
