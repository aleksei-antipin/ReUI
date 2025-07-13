using System;
using System.Collections.Generic;

namespace Abyse.ReUI
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<string, ServiceEntry> _idBoundServices = new();
        private readonly Dictionary<Type, ServiceEntry> _typeBoundServices = new();

        public void Register<T>(Func<object> factory, string id = null)

        {
            var entry = new ServiceEntry(factory);
            RegisterInternal<T>(entry, id);
        }

        public void Register<T>(T instance, string id = null)
        {
            var entry = new ServiceEntry(instance);
            RegisterInternal<T>(entry, id);
        }

        public bool TryGet<T>(out T result, string id = null)
        {
            ServiceEntry entry;
            result = default;
            if (!string.IsNullOrEmpty(id))
            {
                if (!_idBoundServices.TryGetValue(id, out entry)) return false;
            }
            else
            {
                if (!_typeBoundServices.TryGetValue(typeof(T), out entry)) return false;
            }

            if (entry.Instance is not T service) return false;
            result = service;
            return true;
        }

        public T Get<T>(string id = null)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (_idBoundServices.TryGetValue(id, out var entry))
                    return (T)entry.Instance;

                throw new InvalidOperationException($"Service with id '{id}' is not registered.");
            }
            else
            {
                if (_typeBoundServices.TryGetValue(typeof(T), out var entry))
                {
                    if (entry.Instance is T service)
                        return service;
                    throw new InvalidCastException(
                        $"Service of type '{typeof(T)}' is registered but cannot be cast to '{typeof(T)}'.");
                }

                throw new InvalidOperationException($"Service of type '{typeof(T)}' is not registered.");
            }
        }

        private void RegisterInternal<T>(ServiceEntry entry, string id = null)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (!_idBoundServices.TryAdd(id, entry))
                    throw new InvalidOperationException($"Service with id '{id}' is already registered.");
            }
            else
            {
                if (!_typeBoundServices.TryAdd(typeof(T), entry))
                    throw new InvalidOperationException($"Service of type '{typeof(T)}' is already registered.");
            }
        }

        private class ServiceEntry
        {
            private readonly Func<object> _factory;
            private readonly object _instance;

            public ServiceEntry(Func<object> factory)
            {
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
                _instance = null;
            }

            public ServiceEntry(object instance)
            {
                _instance = instance ?? throw new ArgumentNullException(nameof(instance));
                _factory = null;
            }

            public object Instance => _instance ?? _factory?.Invoke();
        }
    }
}