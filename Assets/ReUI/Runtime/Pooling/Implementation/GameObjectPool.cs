using UnityEngine;

namespace Abyse.Pooling
{
    public class GameObjectPool : Pool<GameObject>
    {
        public GameObjectPool(GameObject stash) : base(stash)
        {
        }

        protected sealed override void MountToStash(GameObject obj, GameObject stash)
        {
            obj.transform.SetParent(stash.transform, true);
        }

        protected override void Destroy(GameObject obj)
        {
            if (obj == null)
                return;

            Object.Destroy(obj);
        }
    }
}