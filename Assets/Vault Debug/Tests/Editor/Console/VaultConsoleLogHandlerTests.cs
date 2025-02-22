using AutoFixture;
using NUnit.Framework;
using VaultDebug.Editor.Console;
using VaultDebug.Runtime.Logger;
using VaultDebug.Tests.Editor.Logger.Customizations;

namespace VaultDebug.Tests.Editor.Console
{
    [TestFixture]
    public class VaultConsoleLogHandlerTests
    {
        private IFixture _fixture;
        private VaultEditorLogHandler _logHandler;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new VaultLogCustomization(LogLevel.Info));
            _logHandler = _fixture.Create<VaultEditorLogHandler>();
            _logHandler.Init();
        }

        [Test]
        public void HandleLog_ShouldStoreLog()
        {
            var log = _fixture.Create<VaultLog>();

            _logHandler.HandleLog(log);

            var logs = _logHandler.GetLogsFiltered(null);
            Assert.IsTrue(logs.Exists(l => l.Message == log.Message));
        }

        [Test]
        public void ClearLogs_ShouldRemoveAllLogs()
        {
            _logHandler.ClearLogs();
            var logs = _logHandler.GetLogsFiltered(null);
            Assert.IsEmpty(logs);
        }

        [Test]
        public void HandleLog_ShouldNotThrow_WhenLogIsMalformed()
        {
            var malformedLog = new VaultLog((LogLevel)9999, "TestContext", "Invalid log", "InvalidStacktrace");

            Assert.DoesNotThrow(() => _logHandler.HandleLog(malformedLog));
        }
    }

}
