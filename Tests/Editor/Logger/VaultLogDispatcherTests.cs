using AutoFixture;
using Moq;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;
using VaultDebug.Tests.Editor.Logger.Customizations;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLogDispatcherTests
    {
        private IFixture _fixture;
        private Mock<IVaultLogHandler> _mockHandler;
        private VaultLogDispatcher _vaultLogDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new VaultLogCustomization(LogLevel.Info));
            _mockHandler = _fixture.Freeze<Mock<IVaultLogHandler>>();
            _vaultLogDispatcher = new VaultLogDispatcher();
            _vaultLogDispatcher.RegisterHandler(_mockHandler.Object);
        }

        [Test]
        public void DispatchLog_ShouldSendLogToRegisteredHandler()
        {
            var log = _fixture.Create<VaultLog>();

            _vaultLogDispatcher.DispatchLog(log);

            _mockHandler.Verify(handler => handler.HandleLog(It.Is<VaultLog>(l => l.Message == log.Message)), Times.Once);
        }
    }
}
