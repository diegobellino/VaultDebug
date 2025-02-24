using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLoggerTests
    {
        private IFixture _fixture;
        private VaultLogger _logger;
        private Mock<IVaultLogHandler> _mockHandler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _logger = new VaultLogger("TestContext");
            _mockHandler = _fixture.Freeze<Mock<IVaultLogHandler>>(); // AutoMoq creates a mock
            DIBootstrapper.Container.Resolve<IVaultLogDispatcher>().RegisterHandler(_mockHandler.Object);
        }

        [Test]
        public void InfoLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            _logger.Info(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void DebugLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            _logger.Debug(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void WarnLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            _logger.Warn(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void ErrorLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            _logger.Error(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
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
