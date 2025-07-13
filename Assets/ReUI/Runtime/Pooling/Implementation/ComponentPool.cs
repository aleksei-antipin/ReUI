using UnityEngine;

namespace Abyse.Pooling
{
    public abstract class ComponentPool<T> : Pool<T> where T : Component
    {
        protected ComponentPool(GameObject stash) : base(stash)
        {
        }

        protected sealed override void MountToStash(T obj, GameObject stash)
        {
            obj.transform.SetParent(stash.transform, true);
        }
    }
}
