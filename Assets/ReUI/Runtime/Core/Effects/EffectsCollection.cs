using System.Collections.Generic;
using UnityEngine;

namespace Abyse.ReUI
{
    public abstract class EffectsCollection : MonoBehaviour
    {
        public abstract IEnumerable<IUIEffect> Effects { get; }
    }
}