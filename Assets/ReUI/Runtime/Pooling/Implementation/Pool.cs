using System;
using System.Collections.Generic;
using Abyse.ReUI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abyse.Pooling
{
    public abstract class Pool<TBase> : IPool<TBase> where TBase : Object
    {
        private readonly Dictionary<string, Func<TBase>> _factories = new();
        private readonly Dictionary<string, Stack<TBase>> _pool = new();
        private readonly Dictionary<string, TBase> _prefabs = new();
        private readonly GameObject _stash;
        private readonly Dictionary<TBase, string> _tracked = new();
        private readonly Dictionary<Type, string> _typeIdMap = new();

        protected Pool(GameObject stash)
        {
            _stash = stash;
        }

        public bool TryGet<TDerived>(out TDerived obj, string id = null) where TDerived : TBase
        {
            id = GetId<TDerived>(id);
            obj = null;

            if (string.IsNullOrEmpty(id))
                return false;

            var (resultObj, isNew) = GetOrCreate<TDerived>(id);
            if (resultObj == null)
                return false;

            if (resultObj is TDerived derivedObj)
            {
                obj = derivedObj;
                return true;
            }

            HandleUnusedObject(resultObj, isNew);
            return false;
        }

        public TDerived Get<TDerived>(string id = null) where TDerived : TBase
        {
            id = GetId<TDerived>(id);
            if (string.IsNullOrEmpty(id))
                throw new KeyNotFoundException($"No prefab or factory registered for type '{typeof(TDerived).Name}'.");

            var (obj, isNew) = GetOrCreate<TDerived>(id);
            if (obj == null)
                throw new KeyNotFoundException($"No prefab or factory registered for id '{id}'.");

            if (obj is TDerived derivedObj) return derivedObj;

            HandleUnusedObject(obj, isNew);
            throw new InvalidCastException($"Object with id '{id}' is not of type '{typeof(TDerived).Name}'.");
        }


        public bool TryReturn(TBase obj)
        {
            if (!_tracked.Remove(obj, out var id)) return false;

            MountToStash(obj, _stash);
            if (!_pool.TryGetValue(id, out var stack))
            {
                stack = new Stack<TBase>();
                _pool[id] = stack;
            }

            stack.Push(obj);
            return true;
        }

        public void Register<TDerived>(TBase prefab, string id = null) where TDerived : TBase
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");

            if (!string.IsNullOrEmpty(id))
                RegisterPrefabById(id, prefab);
            else
                RegisterPrefabByType<TDerived>(prefab);
        }


        public void Register<TDerived>(Func<TBase> factory, string id = null) where TDerived : TBase
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");

            if (!string.IsNullOrEmpty(id))
                RegisterFactoryById(id, factory);
            else
                RegisterFactoryByType<TDerived>(factory);
        }


        private (TBase obj, bool isNew) GetOrCreate<TDerived>(string id) where TDerived : TBase
        {
            if (_pool.TryGetValue(id, out var stack) && stack.Count > 0)
            {
                var obj = stack.Pop();
                _tracked[obj] = id;
                return (obj, false);
            }

            var prefab = GetPrefab(id);
            if (prefab == null)
                return (null, false);
            var newObj = Object.Instantiate(prefab, _stash.transform);
            _tracked[newObj] = id;
            return (newObj, true);
        }

        private void HandleUnusedObject(TBase obj, bool isNew)
        {
            if (isNew)
                Destroy(obj);
            else
                TryReturn(obj);
        }

        protected abstract void MountToStash(TBase obj, GameObject stash);


        protected abstract void Destroy(TBase obj);


        private string GetId<TDerived>(string id = null) where TDerived : TBase
        {
            if (!string.IsNullOrEmpty(id)) return id;
            var type = typeof(TDerived);

            return !_typeIdMap.TryGetValue(type, out id) ? null : id;
        }

        private TBase GetPrefab(string id)
        {
            if (_prefabs.TryGetValue(id, out var prefab))
                return prefab;

            return _factories.TryGetValue(id, out var factory) ? factory() : null;
        }


        private void RegisterPrefabById(string id, TBase prefab)
        {
            if (!_prefabs.TryAdd(id, prefab))
                throw new InvalidOperationException($"Prefab with id '{id}' is already registered.");
        }

        private void RegisterPrefabByType<TDerived>(TBase prefab) where TDerived : TBase
        {
            var type = typeof(TDerived);
            var id = Guid.NewGuid().ToString();
            if (!_typeIdMap.TryAdd(type, id))
                throw new InvalidOperationException($"Prefab for type '{type.Name}' is already registered.");
            RegisterPrefabById(id, prefab);
        }

        private void RegisterFactoryById(string id, Func<TBase> factory)
        {
            if (!_factories.TryAdd(id, factory))
                throw new InvalidOperationException($"Factory with id '{id}' is already registered.");
        }

        private void RegisterFactoryByType<TDerived>(Func<TBase> factory) where TDerived : TBase
        {
            var type = typeof(TDerived);
            var id = Guid.NewGuid().ToString();
            if (!_typeIdMap.TryAdd(type, id))
                throw new InvalidOperationException($"Factory for type '{type.Name}' is already registered.");
            RegisterFactoryById(id, factory);
        }
    }
}