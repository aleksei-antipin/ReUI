using System.Linq;
using UnityEngine;

namespace Abyse.ReUI.Samples.Minimal
{
    public class MinimalSample : MonoBehaviour
    {
        [SerializeField] private UIRoot uiRoot;
        [SerializeField] private Widget[] widgets;
        private ReUI _reUI;

        private void Awake()
        {
            CreateReUI();
            OpenScreen();
        }

        private void CreateReUI()
        {
            _reUI = new ReUI(uiRoot);
            _reUI.RegisterWidget(widgets.First() as MinimalScreen);
        }

        private void OpenScreen()
        {
            var widget = _reUI.Open<MinimalScreen>();
        }
    }
}