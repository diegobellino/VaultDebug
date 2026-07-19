using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLoggerTests
    {
        private IFixture _fixture;
        private VaultLogger _logger;
        private LoggerProvider _loggerProvider;
        private Mock<IVaultLogHandler> _mockHandler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerProvider = (LoggerProvider)DIBootstrapper.Container.Resolve<ILoggerProvider>();
            _logger = _loggerProvider.GetLogger("TestContext", UnityEngine.Color.aquamarine);
            _mockHandler = _fixture.Freeze<Mock<IVaultLogHandler>>(); // AutoMoq creates a mock
            DIBootstrapper.Container.Resolve<IVaultLogDispatcher>().RegisterHandler(_mockHandler.Object);
        }

        [Test]
        public void InfoLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            VaultLogProperties testProperties = default;
            testProperties.TryAdd("message", testMessage);

            ExpectUnityLog(LogType.Log, testMessage);
            _logger.Info(testMessage, testProperties);
            _loggerProvider.Drain();

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && (string)log.Properties["message"] == testMessage)), Times.Once);
        }

        [Test]
        public void DebugLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            VaultLogProperties testProperties = default;
            testProperties.TryAdd("message", testMessage);

            ExpectUnityLog(LogType.Log, testMessage);
            _logger.Debug(testMessage, testProperties);
            _loggerProvider.Drain();

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && (string)log.Properties["message"] == testMessage)), Times.Once);
        }

        [Test]
        public void WarnLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            VaultLogProperties testProperties = default;
            testProperties.TryAdd("message", testMessage);

            ExpectUnityLog(LogType.Warning, testMessage);
            _logger.Warn(testMessage, testProperties);
            _loggerProvider.Drain();

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && (string)log.Properties["message"] == testMessage)), Times.Once);
        }

        [Test]
        public void ErrorLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            VaultLogProperties testProperties = default;
            testProperties.TryAdd("message", testMessage);

            ExpectUnityLog(LogType.Error, testMessage);
            _logger.Error(testMessage, testProperties);
            _loggerProvider.Drain();

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && (string)log.Properties["message"] == testMessage)), Times.Once);
        }

        [Test]
        public void InfoLog_ShouldNotThrow_WhenMessageIsNull()
        {
            ExpectUnityLog(LogType.Log, string.Empty);

            Assert.DoesNotThrow(() => _logger.Info(null));
            _loggerProvider.Drain();
        }

        [Test]
        public void ErrorLog_ShouldNotThrow_WhenMessageIsEmpty()
        {
            ExpectUnityLog(LogType.Error, string.Empty);

            Assert.DoesNotThrow(() => _logger.Error(""));
            _loggerProvider.Drain();
        }

        static void ExpectUnityLog(LogType logType, string message)
        {
            var pattern = $@"\[TestContext\](?:</color>)? {Regex.Escape(message)}$";
            LogAssert.Expect(logType, new Regex(pattern));
        }
    }
}
