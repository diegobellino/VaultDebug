#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VaultDebug.Runtime.Logger
{
    /// <summary>
    /// Lightweight DI Bootstrapper to help resolve dependencies using a basic DIContainer. It's meant to be accessible from both editor scripts and runtime.
    /// TODO: Substitute for something more extensible in the future, like UnityContainer
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class DIBootstrapper
    {
        private static DIContainer _container;
        public static DIContainer Container => _container;

        static DIBootstrapper()
        {
            Initialize();
        }

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

            Debug.Log("DIBootstrapper: DIContainer initialized.");
        }
    }
}
