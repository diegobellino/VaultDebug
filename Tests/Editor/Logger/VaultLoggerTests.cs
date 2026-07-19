using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
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
        private Mock<IVaultLogHandler> _mockHandler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _logger = new VaultLogger("TestContext", UnityEngine.Color.aquamarine);
            _mockHandler = _fixture.Freeze<Mock<IVaultLogHandler>>(); // AutoMoq creates a mock
            DIBootstrapper.Container.Resolve<IVaultLogDispatcher>().RegisterHandler(_mockHandler.Object);
        }

        [Test]
        public void InfoLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            ExpectUnityLog(LogType.Log, testMessage);
            _logger.Info(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void DebugLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            ExpectUnityLog(LogType.Log, testMessage);
            _logger.Debug(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void WarnLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            ExpectUnityLog(LogType.Warning, testMessage);
            _logger.Warn(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void ErrorLog_ShouldDispatchLog()
        {
            string testMessage = _fixture.Create<string>();
            var testProperties = _fixture.Create<Dictionary<string, object>>();

            ExpectUnityLog(LogType.Error, testMessage);
            _logger.Error(testMessage, testProperties);

            _mockHandler.Verify(h => h.HandleLog(It.Is<IVaultLog>(log => log.Message == testMessage && log.Properties.SequenceEqual(testProperties))), Times.Once);
        }

        [Test]
        public void InfoLog_ShouldNotThrow_WhenMessageIsNull()
        {
            ExpectUnityLog(LogType.Log, string.Empty);

            Assert.DoesNotThrow(() => _logger.Info(null));
        }

        [Test]
        public void ErrorLog_ShouldNotThrow_WhenMessageIsEmpty()
        {
            ExpectUnityLog(LogType.Error, string.Empty);

            Assert.DoesNotThrow(() => _logger.Error(""));
        }

        static void ExpectUnityLog(LogType logType, string message)
        {
            var pattern = $@"\[TestContext\](?:</color>)? {Regex.Escape(message)}$";
            LogAssert.Expect(logType, new Regex(pattern));
        }
    }
}
