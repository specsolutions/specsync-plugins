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
        [TestCategory("tc:<enter-a-test-case-number-here>")]
        [TestCategory("story:<enter-a-user-story-number-here>")]
        public void OnePassingTest()
        {
            Console.WriteLine("This is a passing test");
        }

        [TestMethod]
        public void OneFailingTest()
        {
            Console.WriteLine("This is a passing test");
            Assert.Fail("This is a simulated error");
        }
    }
}
