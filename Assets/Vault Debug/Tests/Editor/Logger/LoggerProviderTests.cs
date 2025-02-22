using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class LoggerProviderTests
    {
        private IFixture _fixture;
        private LoggerProvider _loggerProvider;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerProvider = new LoggerProvider();
        }

        [Test]
        public void GetLogger_ShouldReturnSameInstance()
        {
            string context = _fixture.Create<string>();

            var logger1 = _loggerProvider.GetLogger(context);
            var logger2 = _loggerProvider.GetLogger(context);

            Assert.AreSame(logger1, logger2);
        }

        [Test]
        public void GetLogger_ShouldReturnDifferentInstancesForDifferentContexts()
        {
            string context1 = _fixture.Create<string>();
            string context2 = _fixture.Create<string>();

            var logger1 = _loggerProvider.GetLogger(context1);
            var logger2 = _loggerProvider.GetLogger(context2);

            Assert.AreNotSame(logger1, logger2);
        }
    }

}
