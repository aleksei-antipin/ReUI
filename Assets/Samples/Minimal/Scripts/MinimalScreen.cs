using UnityEngine;
using UnityEngine.UI;

namespace Abyse.ReUI.Samples.Minimal
{
    public class MinimalScreen : Widget
    {
        [SerializeField] private Button closeButton;

        protected override void OnInitialize()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnCloseButtonClicked()
        {
            RequestClose();
        }

        protected override void OnDeInitialize()
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}