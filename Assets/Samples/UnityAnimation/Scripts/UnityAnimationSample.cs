using UnityEngine;

namespace Abyse.ReUI.Samples.UnityAnimation
{
    public class UnityAnimationSample : MonoBehaviour
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
            foreach (var layout in widgets) _reUI.RegisterWidget(layout);
        }

        private void OpenScreen()
        {
            var widget = _reUI.Open<UnityAnimationScreen>();
        }
    }
}