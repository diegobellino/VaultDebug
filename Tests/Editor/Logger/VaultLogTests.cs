using NUnit.Framework;
using UnityEngine;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Editor.Logger
{
    [TestFixture]
    public class VaultLogTests
    {
        [Test]
        public void Constructor_ShouldStoreColor_WhenProvided()
        {
            var expectedColor = Color.red;
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace", color: expectedColor);

            Assert.AreEqual(expectedColor, log.Color);
        }

        [Test]
        public void Constructor_ShouldLeaveColorUnset_WhenNotProvided()
        {
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace");

            Assert.IsFalse(log.Color.HasValue);
        }

        [Test]
        public void Init_ShouldClearColor_WhenNotProvided()
        {
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace", color: Color.red);
            log.Init(LogLevel.Warn, "Other", "Other message", "Other stacktrace");

            Assert.IsFalse(log.Color.HasValue);
        }

        [Test]
        public void Init_ShouldSetColor_WhenProvided()
        {
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace");
            var expectedColor = Color.green;
            log.Init(LogLevel.Warn, "Other", "Other message", "Other stacktrace", color: expectedColor);

            Assert.AreEqual(expectedColor, log.Color);
        }

        [Test]
        public void Clone_ShouldCopyColor()
        {
            var expectedColor = Color.blue;
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace", color: expectedColor);
            var clone = log.Clone();

            Assert.AreEqual(expectedColor, clone.Color);
        }

        [Test]
        public void ApplyDeserializedColorDefault_ShouldUseWhite_WhenColorMissing()
        {
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace");
            log.ApplyDeserializedColorDefault();

            Assert.AreEqual(Color.white, log.Color);
        }

        [Test]
        public void ApplyDeserializedColorDefault_ShouldPreserveExplicitColor()
        {
            var log = new VaultLog(LogLevel.Info, "Context", "Message", "Stacktrace", color: Color.red);
            log.ApplyDeserializedColorDefault();

            Assert.AreEqual(Color.red, log.Color);
        }
    }
}
