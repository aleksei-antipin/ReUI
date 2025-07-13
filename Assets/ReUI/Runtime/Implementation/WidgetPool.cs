using Abyse.Pooling;
using UnityEngine;

namespace Abyse.ReUI
{
    public class WidgetPool : ComponentPool<Widget>
    {
        public WidgetPool(GameObject stash) : base(stash)
        {
        }

        protected override void Destroy(Widget obj)
        {
            if (obj == null)
                return;

            Object.Destroy(obj.gameObject);
        }
    }
}