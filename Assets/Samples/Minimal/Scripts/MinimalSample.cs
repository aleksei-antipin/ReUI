using UnityEngine;

namespace Abyse.ReUI.Samples.Minimal
{
    public class MinimalSample : MonoBehaviour
    {
        [SerializeField] private ReUI reUI;

        private void Awake()
        {
            reUI.Initialize();
            var widget = reUI.Open<MinimalScreen>();
        }
    }
}