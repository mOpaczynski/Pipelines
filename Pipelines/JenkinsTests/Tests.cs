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
        public void FistTest()
        {
            int number = 10;

            Assert.IsTrue(number == 10);
        }

        [Test]
        public void SecondTest()
        {
            int number = 10;

            Assert.IsTrue(number == 10);
        }

        [Test]
        [Ignore("bababa")]
        public void ThirdTest()
        {
            int number = 10;

            Assert.IsTrue(number == 10);
        }

        [Test]
        public void FourthTest()
        {
            int number = 10;

            Assert.IsTrue(number == 10);
        }

        [Test]
        public void Migrate()
        {
            ////MigrateDatabases();
        }
    }
}
