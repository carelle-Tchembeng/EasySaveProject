// EasySave.ConsoleApp/DI/ServiceContainer.cs

namespace EasySave.ConsoleApp.DI
{
    /// <summary>
    /// Minimal dependency injection container for EasySave.
    /// Stores singleton instances registered by type and resolves them on demand.
    /// Designed to be simple, transparent, and easy to understand.
    /// For a production application, Microsoft.Extensions.DependencyInjection
    /// would be preferred, but this lightweight version is sufficient for v1.0.
    /// </summary>
    public class ServiceContainer
    {
        // ─────────────────────────────────────────────────────────────
        // Internal registry
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps each registered type to its singleton instance.
        /// </summary>
        private readonly Dictionary<Type, object> _services = new();

        // ─────────────────────────────────────────────────────────────
        // Registration
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Registers a singleton instance under its type T.
        /// If T is already registered, the previous instance is replaced.
        /// </summary>
        /// <typeparam name="T">The type (usually an interface) to register under.</typeparam>
        /// <param name="instance">The instance to store.</param>
        public void Register<T>(T instance) where T : notnull
        {
            _services[typeof(T)] = instance;
        }

        // ─────────────────────────────────────────────────────────────
        // Resolution
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves and returns the registered instance for type T.
        /// Throws InvalidOperationException if T has not been registered.
        /// </summary>
        /// <typeparam name="T">The type to resolve.</typeparam>
        /// <returns>The registered instance cast to T.</returns>
        public T Resolve<T>() where T : notnull
        {
            if (_services.TryGetValue(typeof(T), out object? instance))
                return (T)instance;

            throw new InvalidOperationException(
                $"No service registered for type '{typeof(T).Name}'. " +
                "Ensure it was registered in ServiceContainer.Build().");
        }
    }
}
