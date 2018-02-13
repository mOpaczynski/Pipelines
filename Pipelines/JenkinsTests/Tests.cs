using NUnit.Framework;

namespace JenkinsTests
{
    [TestFixture]
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
        }

        [Test]
        public void FirstTest()
        {
            int number = 10;

            Assert.IsTrue(number == 10);
        }
    }
}
