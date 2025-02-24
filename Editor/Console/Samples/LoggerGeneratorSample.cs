using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using VaultDebug.Runtime.Logger;
using Debug = UnityEngine.Debug;

namespace VaultDebug.Editor.Console.Samples
{
    public class LoggerGeneratorSample
    {
        private static ILoggerProvider _loggerProvider;
        private static int _logCount = 10000;

        [MenuItem("Vault Debug/Console/Generate test logs")]
        public static void TestLogs()
        {
            _loggerProvider ??= DIBootstrapper.Container.Resolve<ILoggerProvider>();

            var logger1 = _loggerProvider.GetLogger("Sample 1");
            var logger2 = _loggerProvider.GetLogger("Sample 2");

            logger1.Debug("Long debug log from internal logger - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce at dignissim odio. Suspendisse sed consequat justo. Phasellus consequat, est vitae auctor mollis, mi nunc volutpat tortor, sed auctor magna dui vitae nulla. Curabitur eu tincidunt dui. Donec condimentum libero sit amet magna rhoncus, eu tristique sapien vestibulum. Phasellus volutpat, eros at auctor placerat, ipsum felis venenatis velit, eget mattis turpis tortor vel diam. Nulla eu mauris eu libero congue rhoncus ac sed nunc. Duis maximus ultrices elit, in varius ipsum sodales in. Aenean nisl erat, porttitor nec laoreet non, placerat dignissim enim. ");
            logger2.Error("Error log from another logger", new Dictionary<string, object> { { "sampleProperty", "property 1" } });
            logger1.Warn("Warn log from internal logger");
            logger2.Info("Info log from another logger");
        }

        [MenuItem("Vault Debug/Console/Benchmark logs")]
        public static void Benchmark()
        {
            _loggerProvider ??= DIBootstrapper.Container.Resolve<ILoggerProvider>();
            var logger = _loggerProvider.GetLogger("Benchmark");

            Stopwatch stopwatch = new Stopwatch();

            // Benchmark Unity's Debug.Log
            stopwatch.Start();
            for (int i = 0; i < _logCount; i++)
            {
                Debug.Log("Unity Debug log " + i);
            }
            stopwatch.Stop();
            var unityLogTime = stopwatch.ElapsedMilliseconds;

            // Benchmark VaultLogger logging
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < _logCount; i++)
            {
                logger.Info("Vault log " + i);
            }
            stopwatch.Stop();

            Debug.Log($"VaultLogger.Info: {stopwatch.ElapsedMilliseconds} ms for {_logCount} logs");
            Debug.Log($"Unity Debug.Log: {unityLogTime} ms for {_logCount} logs");
        }
    }
}
