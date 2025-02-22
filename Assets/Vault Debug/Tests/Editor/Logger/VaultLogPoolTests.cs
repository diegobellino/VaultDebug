using AutoFixture;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;
using VaultDebug.Tests.Editor.Logger.Customizations;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLogPoolTests
    {
        private IFixture _fixture;
        private VaultLogPool _vaultLogPool;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new VaultLogCustomization(LogLevel.Info));
            _vaultLogPool = new VaultLogPool();
        }

        [Test]
        public void GetLog_ShouldReuseInstances()
        {
            var log1 = _vaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
            _vaultLogPool.ReleaseLog(log1);
            var log2 = _vaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            Assert.AreEqual(log1, log2);
        }
    }

}
