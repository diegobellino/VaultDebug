using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class LoggerProviderTests
    {
        private LoggerProvider _loggerProvider;

        [SetUp]
        public void Setup()
        {
            _loggerProvider = new LoggerProvider(
                DIBootstrapper.Container.Resolve<IVaultLogPool>(),
                DIBootstrapper.Container.Resolve<IVaultLogDispatcher>());
        }

        [TearDown]
        public void TearDown()
        {
            _loggerProvider.Dispose();
        }

        [Test]
        public void GetLogger_CreatesBurstSafeValue()
        {
            Assert.DoesNotThrow(() => _loggerProvider.GetLogger("Gameplay"));
        }

        [Test]
        public void GetLogger_AcceptsIndependentContexts()
        {
            Assert.DoesNotThrow(() =>
            {
                _loggerProvider.GetLogger("Gameplay");
                _loggerProvider.GetLogger("Physics");
            });
        }
    }

}
