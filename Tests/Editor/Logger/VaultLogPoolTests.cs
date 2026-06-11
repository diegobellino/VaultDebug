using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
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
            var log1 = _vaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Dictionary<string, object>>());
            _vaultLogPool.ReleaseLog(log1);
            var log2 = _vaultLogPool.GetLog(_fixture.Create<LogLevel>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Dictionary<string, object>>());

            Assert.AreEqual(log1, log2);
        }

        [Test]
        public void GetLog_ShouldNotRetainColor_WhenReusedWithoutColor()
        {
            var coloredLog = _vaultLogPool.GetLog(
                LogLevel.Info,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Dictionary<string, object>>(),
                Color.red);
            _vaultLogPool.ReleaseLog(coloredLog);

            var reusedLog = _vaultLogPool.GetLog(
                LogLevel.Warn,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Dictionary<string, object>>());

            Assert.IsFalse(reusedLog.Color.HasValue);
        }

        [Test]
        public void GetLog_ShouldSetColor_WhenProvided()
        {
            var expectedColor = Color.cyan;
            var log = _vaultLogPool.GetLog(
                LogLevel.Info,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Dictionary<string, object>>(),
                expectedColor);

            Assert.AreEqual(expectedColor, log.Color);
        }
    }

}
