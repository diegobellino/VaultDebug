using UnityEditor;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Editor.Console.Samples
{
    public class LoggerGeneratorSample
    {
        private static ILoggerProvider _loggerProvider;


        [MenuItem("Vault Debug/Console/Generate test logs")]
        public static void TestLogs()
        {
            _loggerProvider ??= DIBootstrapper.Container.Resolve<ILoggerProvider>();

            var logger1 = _loggerProvider.GetLogger("Sample 1");
            var logger2 = _loggerProvider.GetLogger("Sample 2");

            logger1.Debug("Long debug log from internal logger - Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce at dignissim odio. Suspendisse sed consequat justo. Phasellus consequat, est vitae auctor mollis, mi nunc volutpat tortor, sed auctor magna dui vitae nulla. Curabitur eu tincidunt dui. Donec condimentum libero sit amet magna rhoncus, eu tristique sapien vestibulum. Phasellus volutpat, eros at auctor placerat, ipsum felis venenatis velit, eget mattis turpis tortor vel diam. Nulla eu mauris eu libero congue rhoncus ac sed nunc. Duis maximus ultrices elit, in varius ipsum sodales in. Aenean nisl erat, porttitor nec laoreet non, placerat dignissim enim. ");
            logger2.Error("Error log from another logger");
            logger1.Warn("Warn log from internal logger");
            logger2.Info("Info log from another logger");
        }
    }
}
