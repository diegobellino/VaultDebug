using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLoggerTests
    {
        private IFixture _fixture;
        private VaultLogger _logger;
        private Mock<IVaultLogHandler> _mockHandler;
        private Mock<IVaultLogPool> _logPoolMock;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _logPoolMock = _fixture.Freeze<Mock<IVaultLogPool>>();
            _logPoolMock.Setup(x => x.GetLog(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((LogLevel level, string context, string message, string stackTrace) => new VaultLog(level, context, message, stackTrace));
            _logger = new VaultLogger("TestContext", _logPoolMock.Object);
            _mockHandler = _fixture.Freeze<Mock<IVaultLogHandler>>(); // AutoMoq creates a mock
            VaultLogDispatcher.RegisterHandler(_mockHandler.Object);
        }

        [Test]
        public void InfoLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();

            _logger.Info(testMessage);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage)), Times.Once);
        }

        [Test]
        public void DebugLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();

            _logger.Debug(testMessage);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage)), Times.Once);
        }

        [Test]
        public void WarnLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();

            _logger.Warn(testMessage);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage)), Times.Once);
        }

        [Test]
        public void ErrorLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();

            _logger.Error(testMessage);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage)), Times.Once);
        }

        [Test]
        public void InfoLog_ShouldNotThrow_WhenMessageIsNull()
        {
            Assert.DoesNotThrow(() => _logger.Info(null));
        }

        [Test]
        public void ErrorLog_ShouldNotThrow_WhenMessageIsEmpty()
        {
            Assert.DoesNotThrow(() => _logger.Error(""));
        }
    }
}
