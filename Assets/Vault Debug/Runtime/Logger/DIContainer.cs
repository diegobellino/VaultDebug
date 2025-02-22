using System;
using System.Collections.Generic;
using System.Linq;

namespace VaultDebug.Runtime.Logger
{
    public enum Lifetime
    {
        Transient,
        Singleton
    }

    public class DIContainer
    {
        // Internal registration record
        private class Registration
        {
            public Type ImplementationType { get; }
            public Lifetime Lifetime { get; }
            public object Instance { get; set; }

            public Registration(Type implementationType, Lifetime lifetime)
            {
                ImplementationType = implementationType;
                Lifetime = lifetime;
            }
        }

        // Mapping from abstraction to its registration details
        private readonly Dictionary<Type, Registration> _registrations = new();

        /// <summary>
        /// Registers a type mapping with the container.
        /// </summary>
        /// <typeparam name="TInterface">The abstraction.</typeparam>
        /// <typeparam name="TImplementation">The concrete implementation.</typeparam>
        /// <param name="lifetime">Lifetime of the instance.</param>
        public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Transient)
            where TImplementation : TInterface
        {
            _registrations[typeof(TInterface)] = new Registration(typeof(TImplementation), lifetime);
        }

        /// <summary>
        /// Registers an already created instance with the container.
        /// </summary>
        /// <typeparam name="TInterface">The abstraction.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public void RegisterInstance<TInterface>(TInterface instance)
        {
            _registrations[typeof(TInterface)] = new Registration(null, Lifetime.Singleton)
            {
                Instance = instance
            };
        }

        /// <summary>
        /// Resolves an instance of the specified type.
        /// </summary>
        /// <typeparam name="TInterface">The type to resolve.</typeparam>
        /// <returns>An instance of the type.</returns>
        public TInterface Resolve<TInterface>()
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        private object Resolve(Type interfaceType)
        {
            if (!_registrations.TryGetValue(interfaceType, out Registration registration))
            {
                // If not explicitly registered and it's a concrete type, try resolving it directly.
                if (!interfaceType.IsAbstract && !interfaceType.IsInterface)
                {
                    registration = new Registration(interfaceType, Lifetime.Transient);
                }
                else
                {
                    throw new InvalidOperationException($"Type {interfaceType.FullName} has not been registered.");
                }
            }

            // For singletons, return the cached instance if available.
            if (registration.Lifetime == Lifetime.Singleton && registration.Instance != null)
            {
                return registration.Instance;
            }

            // Determine the type to instantiate.
            var implementationType = registration.ImplementationType ?? interfaceType;

            // Get the constructor with the most parameters (simple strategy).
            var constructor = implementationType
                .GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();

            // Recursively resolve constructor parameters.
            var parameters = constructor.GetParameters()
                                        .Select(p => Resolve(p.ParameterType))
                                        .ToArray();

            // Create the instance.
            var instance = Activator.CreateInstance(implementationType, parameters);

            // If singleton, cache the instance.
            if (registration.Lifetime == Lifetime.Singleton)
            {
                registration.Instance = instance;
            }

            return instance;
        }
    }
}
