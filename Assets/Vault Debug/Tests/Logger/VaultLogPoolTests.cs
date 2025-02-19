using AutoFixture;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;
using VaultDebug.Tests.Logger.Customizations;

namespace VaultDebug.Tests.Logger
{
    [TestFixture]
    public class VaultLogPoolTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new VaultLogCustomization(LogLevel.Info));
        }

        [Test]
        public void GetLog_ShouldReuseInstances()
        {
            var log1 = VaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            VaultLogPool.ReleaseLog(log1);
            var log2 = VaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            Assert.AreEqual(log1, log2);
        }
    }

}
