using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class UnityConsoleLogHandlerTests
    {
        UnityConsoleLogHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _handler = new UnityConsoleLogHandler();
        }

        [TestCase(LogLevel.Info, LogType.Log)]
        [TestCase(LogLevel.Debug, LogType.Log)]
        [TestCase(LogLevel.Warn, LogType.Warning)]
        [TestCase(LogLevel.Error, LogType.Error)]
        [TestCase(LogLevel.Exception, LogType.Error)]
        public void HandleLog_WritesExpectedUnityLogType(LogLevel level, LogType expectedLogType)
        {
            LogAssert.Expect(expectedLogType, new Regex(@"^\[TestContext\] Test message$"));

            _handler.HandleLog(CreateLog(level));
        }

        [Test]
        public void HandleLog_WithColor_ColorsOnlyContextTag()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"^<color=#FF0000FF>\[TestContext\]</color> Test message$"));

            _handler.HandleLog(CreateLog(LogLevel.Info, Color.red));
        }

        [Test]
        public void HandleLog_IgnoresPropertiesAndTimestamp()
        {
            var properties = new Dictionary<string, object>
            {
                { "secret", "do not print" }
            };
            var log = new VaultLog(LogLevel.Info, "TestContext", "Test message", "stacktrace", properties);

            LogAssert.Expect(LogType.Log, new Regex(@"^\[TestContext\] Test message$"));

            _handler.HandleLog(log);
        }

        [Test]
        public void VaultLogger_WritesToUnityConsoleByDefault()
        {
            var provider = (LoggerProvider)DIBootstrapper.Container.Resolve<ILoggerProvider>();
            var logger = provider.GetLogger("TestContext");

            LogAssert.Expect(LogType.Log, new Regex(@"^\[TestContext\] Test message$"));

            logger.Info("Test message");
            provider.Drain();
        }

        static VaultLog CreateLog(LogLevel level, Color? color = null)
        {
            return new VaultLog(level, "TestContext", "Test message", "stacktrace", color: color);
        }
    }
}
