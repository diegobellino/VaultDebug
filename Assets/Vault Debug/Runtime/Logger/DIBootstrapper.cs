#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// A lightweight DI Bootstrapper that initializes a basic DIContainer for dependency resolution.
    /// Accessible from both editor scripts and runtime.
    /// TODO: Replace with a more extensible container (e.g., UnityContainer) in the future.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class DIBootstrapper
    {
        private static DIContainer _container;

        /// <summary>
        /// Gets the dependency injection container.
        /// </summary>
        public static DIContainer Container => _container;

        static DIBootstrapper()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the DI container and registers common dependencies.
        /// </summary>
        public static void Initialize()
        {
            if (_container != null)
            {
                return;
            }

            _container = new DIContainer();

            // Register common dependencies.
            Container.Register<IVaultLogPool, VaultLogPool>(Lifetime.Singleton);
            Container.Register<IVaultLogDispatcher, VaultLogDispatcher>(Lifetime.Singleton);
            Container.Register<ILoggerProvider, LoggerProvider>(Lifetime.Singleton);
            Container.Register<ILogIdProvider, LogIdProvider>(Lifetime.Singleton);

            Debug.Log("DIBootstrapper: DIContainer initialized.");
        }
    }
}
