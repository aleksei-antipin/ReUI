using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abyse.ReUI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class Widget : MonoBehaviour
    {
        [SerializeField] private string[] customEffectCategories;

        private readonly List<IDisposable> _disposables = new();

        private string[] _effectCategories;

        private bool _isEffectsInitialized;

        internal Action<Widget, bool> OnCloseRequested;

        protected void AddDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDeInitialize()
        {
        }

        protected virtual IEnumerator OnOpen(bool animated)
        {
            yield break;
        }


        protected virtual IEnumerator OnClose(bool animated)
        {
            yield break;
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        internal void DeInitialize()
        {
            OnDeInitialize();

            foreach (var disposable in _disposables) disposable.Dispose();
        }

        internal IEnumerator Open(bool animated)
        {
            yield return OnOpen(animated);
        }

        internal IEnumerator Close(bool animated)
        {
            yield return OnClose(animated);
        }

        protected void RequestClose(bool animated = true)
        {
            OnCloseRequested?.Invoke(this, animated);
        }
    }
}