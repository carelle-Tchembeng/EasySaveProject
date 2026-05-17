// EasySave.WPF/DI/ServiceContainer.cs — same lightweight DI as ConsoleApp
namespace EasySave.WPF.DI
{
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T instance) where T : notnull
            => _services[typeof(T)] = instance;

        public T Resolve<T>() where T : notnull
        {
            if (_services.TryGetValue(typeof(T), out object? instance)) return (T)instance;
            throw new InvalidOperationException($"No service registered for '{typeof(T).Name}'.");
        }
    }
}
