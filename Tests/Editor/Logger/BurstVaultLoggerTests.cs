using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class BurstVaultLoggerTests
    {
        LoggerProvider _provider;
        IVaultLogDispatcher _dispatcher;
        Mock<IVaultLogHandler> _handler;
        List<IVaultLog> _received;

        [SetUp]
        public void SetUp()
        {
            var pool = DIBootstrapper.Container.Resolve<IVaultLogPool>();
            _dispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
            _provider = new LoggerProvider(pool, _dispatcher);
            _received = new List<IVaultLog>();
            _handler = new Mock<IVaultLogHandler>();
            _handler
                .Setup(handler => handler.HandleLog(It.IsAny<IVaultLog>()))
                .Callback<IVaultLog>(log => _received.Add(log.Clone()));
            _dispatcher.RegisterHandler(_handler.Object, new[] { "Burst" });
        }

        [TearDown]
        public void TearDown()
        {
            _dispatcher.UnregisterHandler(_handler.Object);
            _provider.Dispose();
        }

        [Test]
        public void BurstJob_ForwardsTypedStructuredProperties()
        {
            var logger = _provider.GetLogger("Burst");
            VaultLogProperties properties = default;
            properties.TryAdd("entityId", 42L);
            properties.TryAdd("active", true);
            properties.TryAdd("speed", 2.5d);
            properties.TryAdd("name", new FixedString128Bytes("Player"));

            new WriteLogJob
            {
                Logger = logger,
                Message = new FixedString512Bytes("Entity updated"),
                Properties = properties
            }.Schedule().Complete();
            _provider.Drain();

            var log = _received.Single();
            Assert.That(log.Level, Is.EqualTo(LogLevel.Info));
            Assert.That(log.Message, Is.EqualTo("Entity updated"));
            Assert.That(log.Properties["entityId"], Is.EqualTo(42L));
            Assert.That(log.Properties["active"], Is.EqualTo(true));
            Assert.That(log.Properties["speed"], Is.EqualTo(2.5d));
            Assert.That(log.Properties["name"], Is.EqualTo("Player"));
            Assert.That(log.Stacktrace, Is.Empty);
        }

        [Test]
        public void StructuredPayload_RejectsPropertiesPastItsFixedCapacity()
        {
            VaultLogProperties properties = default;

            for (var index = 0; index < VaultLogProperties.Capacity; index++)
            {
                Assert.That(properties.TryAdd(new FixedString64Bytes($"field{index}"), index), Is.True);
            }

            Assert.That(properties.TryAdd("overflow", 9L), Is.False);
            Assert.That(properties.Count, Is.EqualTo(VaultLogProperties.Capacity));
        }

        [Test]
        public void Logger_RetainsOnlyItsConfiguredPropertyLimit()
        {
            var logger = _provider.GetLogger("Burst", maxProperties: 2);
            VaultLogProperties properties = default;
            properties.TryAdd("one", 1L);
            properties.TryAdd("two", 2L);
            properties.TryAdd("three", 3L);

            logger.Info(new FixedString512Bytes("Limited"), properties);
            _provider.Drain();

            var log = _received.Single();
            Assert.That(log.Properties.Count, Is.EqualTo(2));
            Assert.That(log.Properties.ContainsKey("one"), Is.True);
            Assert.That(log.Properties.ContainsKey("two"), Is.True);
            Assert.That(log.Properties.ContainsKey("three"), Is.False);
        }

        [BurstCompile]
        struct WriteLogJob : IJob
        {
            public VaultLogger Logger;
            public FixedString512Bytes Message;
            public VaultLogProperties Properties;

            public void Execute()
            {
                Logger.Info(Message, Properties);
            }
        }
    }
}
