using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inedo.BuildMasterExtensions.Java;
using MbUnit.Framework;

namespace Java.Tests
{
    [TestFixture]
    public class TestUpdatePOM
    {
        [Test]
        public void TestIt()
        {
            string data = Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(@"C:\src\javatest\InedoExample\pom.xml"));
            var act = new UpdateVersionInPOMAction();
            var result = act.TransformFile(data, "1.0");
            Assert.IsNotEmpty(result);
        }
    }
}
