using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLoggerFactoryTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [Test]
        public void GetOrCreateLogger_ShouldReturnSameInstance()
        {
            string context = _fixture.Create<string>();

            var logger1 = VaultLoggerFactory.GetOrCreateLogger(context);
            var logger2 = VaultLoggerFactory.GetOrCreateLogger(context);

            Assert.AreSame(logger1, logger2);
        }

        [Test]
        public void GetOrCreateLogger_ShouldReturnDifferentInstancesForDifferentContexts()
        {
            string context1 = _fixture.Create<string>();
            string context2 = _fixture.Create<string>();

            var logger1 = VaultLoggerFactory.GetOrCreateLogger(context1);
            var logger2 = VaultLoggerFactory.GetOrCreateLogger(context2);

            Assert.AreNotSame(logger1, logger2);
        }
    }

}
