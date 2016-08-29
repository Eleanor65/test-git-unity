using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var regex = new Regex(@"^(?<skip>\s+.+ [|] \d+ [+-]+)$");
            var match = regex.Match("  README.txt | 1 -");
            var m = match.Success;
            Assert.IsTrue(m);
        }
    }
}
