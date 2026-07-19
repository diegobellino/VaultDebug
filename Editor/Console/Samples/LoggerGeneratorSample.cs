using System.Diagnostics;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
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

            var logger1 = _loggerProvider.GetLogger("Sample 1", Color.magenta);
            var logger2 = _loggerProvider.GetLogger("Sample 2");

            logger1.Debug("Debug log from internal logger.");
            VaultLogProperties properties = default;
            properties.TryAdd("sampleProperty", new FixedString128Bytes("property 1"));
            logger2.Error(new FixedString512Bytes("Error log from another logger"), properties);
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
