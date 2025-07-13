using UnityEngine;

namespace Abyse.ReUI
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private GameObject stash;
        public GameObject Stash => stash;
    }
}