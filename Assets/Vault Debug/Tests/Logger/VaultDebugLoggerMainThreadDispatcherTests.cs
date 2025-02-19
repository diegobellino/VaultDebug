using NUnit.Framework;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Logger
{

    [TestFixture]
    public class VaultDebugLoggerMainThreadDispatcherTests
    {
        [Test]
        public void Enqueue_ShouldExecuteOnMainThread()
        {
            bool wasExecuted = false;

            var dispatcher = VaultDebugLoggerMainThreadDispatcher.Instance(obj => { });

            Assert.IsNotNull(dispatcher, "Dispatcher instance should not be null");

            dispatcher.Enqueue(() => wasExecuted = true);

            Assert.IsFalse(wasExecuted, "Action should not execute immediately");

            // Directly call Update() since Invoke does not work for MonoBehaviour methods
            dispatcher.GetType().GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dispatcher, null);

            Assert.IsTrue(wasExecuted, "Action should execute after update");
        }


    }

}
