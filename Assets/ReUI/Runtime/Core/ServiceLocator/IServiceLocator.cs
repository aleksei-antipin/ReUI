using System;

namespace Abyse.ReUI
{
    public interface IServiceLocator
    {
        bool TryGet<T>(out T result, string id = null);
        T Get<T>(string id = null);
        void Register<T>(Func<object> factory, string id = null);
        void Register<T>(T instance, string id = null);
    }
}