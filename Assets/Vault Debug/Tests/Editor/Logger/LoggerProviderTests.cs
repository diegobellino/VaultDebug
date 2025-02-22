using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class LoggerProviderTests
    {
        private IFixture _fixture;
        private Mock<IVaultLogPool> _logPoolMock;
        private LoggerProvider _loggerProvider;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _logPoolMock = _fixture.Freeze<Mock<IVaultLogPool>>();
            _loggerProvider = new LoggerProvider();
        }

        [Test]
        public void GetLogger_ShouldReturnSameInstance()
        {
            string context = _fixture.Create<string>();

            var logger1 = _loggerProvider.GetLogger(context, _logPoolMock.Object);
            var logger2 = _loggerProvider.GetLogger(context, _logPoolMock.Object);

            Assert.AreSame(logger1, logger2);
        }

        [Test]
        public void GetLogger_ShouldReturnDifferentInstancesForDifferentContexts()
        {
            string context1 = _fixture.Create<string>();
            string context2 = _fixture.Create<string>();

            var logger1 = _loggerProvider.GetLogger(context1, _logPoolMock.Object);
            var logger2 = _loggerProvider.GetLogger(context2, _logPoolMock.Object);

            Assert.AreNotSame(logger1, logger2);
        }
    }

}
