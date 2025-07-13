using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abyse.ReUI
{
    public abstract class Widget : MonoBehaviour
    {
        [SerializeField] private EffectsCollection[] effectCollections;

        [SerializeField] private string[] customEffectCategories;

        private readonly HashSet<Widget> _children = new();

        private readonly List<IDisposable> _disposables = new();

        private Dictionary<string, IUIEffect[]> _categorizedEffects;

        private string[] _effectCategories;

        private bool _isEffectsInitialized;

        private ReUI _reUI;

        internal Action<Widget, bool> OnCloseRequested;

        public IEnumerable<string> CustomEffectCategories => customEffectCategories;
        public IEnumerable<string> EffectCategories => _effectCategories;

        protected IReadOnlyCollection<Widget> Children => _children;

        internal IEnumerable<IUIEffect> GetEffects(params string[] categories)
        {
            var animations = new List<IUIEffect>();
            foreach (var category in categories)
                if (_categorizedEffects.TryGetValue(category, out var categoryAnimations))
                    animations.AddRange(categoryAnimations);
                else
                    throw new ReUIException($"Effect category {category} not defined of layout {name}");


            var distinct = animations.Distinct().ToArray();
            return distinct;
        }

        private void InitializeEffects()
        {
            if (_isEffectsInitialized) return;
            _effectCategories = customEffectCategories.Concat(DefaultEffectsCategories.Categories).ToArray();

            var animations = effectCollections.SelectMany(a => a.Effects);
            _categorizedEffects = _effectCategories.ToDictionary(
                c => c,
                c => animations.Where(a => a.Categories.Contains(c)).ToArray());


            _isEffectsInitialized = true;
        }

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

        protected virtual IEnumerator OnOpen()
        {
            yield break;
        }

        protected virtual IEnumerator OnClose()
        {
            yield break;
        }

        internal void Initialize()
        {
            InitializeEffects();
            OnInitialize();

            foreach (var child in Children) child.Initialize();
        }

        internal void DeInitialize()
        {
            OnDeInitialize();

            foreach (var child in Children) child.DeInitialize();

            foreach (var disposable in _disposables) disposable.Dispose();
        }

        internal IEnumerator Open(bool animated)
        {
            var openCoroutines = new List<IEnumerator>
            {
                OnOpen()
            };
            openCoroutines.AddRange(Children.Select(child => child.Open(animated)));

            var openEffects = GetEffects(DefaultEffectsCategories.Open);
            if (animated) openCoroutines.AddRange(openEffects.Select(effect => effect.Play()));

            yield return WhenAll(openCoroutines.ToArray());
        }

        internal IEnumerator Close(bool animated)
        {
            var closeCoroutines = new List<IEnumerator>
            {
                OnClose()
            };
            closeCoroutines.AddRange(Children.Select(child => child.Close(animated)));
            var closeEffects = GetEffects(DefaultEffectsCategories.Close);
            var closeBackwardEffects = GetEffects(DefaultEffectsCategories.OpenForwardCloseBackward);
            if (animated)
            {
                closeCoroutines.AddRange(closeEffects.Select(effect => effect.Play()));
                closeCoroutines.AddRange(closeBackwardEffects.Select(effect => effect.PlayBackwards()));
            }

            yield return WhenAll(closeCoroutines.ToArray());
        }

        public void PlayAnimationCategories(params string[] categories)
        {
            var animations = GetEffects(categories);
            foreach (var animation in animations) StartCoroutine(animation.Play());
        }

        public void PlayBackwardAnimationCategories(params string[] categories)
        {
            var animations = GetEffects(categories);
            foreach (var animation in animations) StartCoroutine(animation.PlayBackwards());
        }

        protected void RequestClose(bool animated = true)
        {
            OnCloseRequested?.Invoke(this, animated);
        }

        protected TWidget CreateChild<TWidget>(
            string widgetId = null,
            Transform mountingPoint = null) where TWidget : Widget, new()
        {
            // var widget = _reUI.Create<TWidget>(widgetId, mountingPoint);
            // AddChild(widget);
            // return widget;
            return null;
        }

        protected TWidget OpenChild<TWidget>(
            string widgetId = null,
            bool animated = true,
            Transform mountingPoint = null) where TWidget : Widget, new()
        {
            var widget = CreateChild<TWidget>(widgetId, mountingPoint);
            widget.Initialize();
            StartCoroutine(widget.Open(animated));
            return widget;
        }

        protected TWidget Open<TWidget>(string layoutId = null)
            where TWidget : Widget, new()
        {
            var widget = _reUI.Open<TWidget>(layoutId);
            return widget;
        }


        private void AddChild(Widget child)
        {
            _children.Add(child);
            child.OnCloseRequested += OnChildRequestedClose;
        }

        protected void CloseChild(Widget widget, bool animated = true)
        {
            widget.DeInitialize();
            StartCoroutine(WaitClosing());
            widget.OnCloseRequested -= OnChildRequestedClose;
            return;

            IEnumerator WaitClosing()
            {
                yield return widget.Close(animated);
                _reUI.ReturnWidget(widget);
                _children.Remove(widget);
            }
        }

        private void OnChildRequestedClose(Widget widget, bool animated)
        {
            CloseChild(widget, animated);
        }

        private IEnumerator WhenAll(params IEnumerator[] coroutines)
        {
            var remainingCount = coroutines.Length;

            foreach (var coroutine in coroutines) StartCoroutine(WaitForEnd(coroutine));

            yield return new WaitWhile(() => remainingCount > 0);
            yield break;

            IEnumerator WaitForEnd(IEnumerator coroutine)
            {
                yield return coroutine;
                remainingCount--;
            }
        }
    }
}