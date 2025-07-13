using System;

namespace Abyse.ReUI
{
    public interface IPool<TBase> where TBase : class
    {
        bool TryGet<TDerived>(out TDerived obj, string id = null) where TDerived : TBase;
        TDerived Get<TDerived>(string id = null) where TDerived : TBase;
        bool TryReturn(TBase obj);
        void Register<TDerived>(TBase prefab, string id = null) where TDerived : TBase;
        void Register<TDerived>(Func<TBase> factory, string id = null) where TDerived : TBase;
    }
}