using AutoFixture;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Tests.Logger.Customizations
{
    public class VaultLogCustomization : ICustomization
    {
        private readonly LogLevel _logLevel;

        public VaultLogCustomization(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<VaultLog>(composer => composer.FromFactory(() =>
            {
                return new VaultLog(
                    _logLevel, // Override LogLevel
                    fixture.Create<string>(), // Context
                    fixture.Create<string>(), // Message
                    fixture.Create<string>()  // Stacktrace
                );
            }));
        }
    }

}
