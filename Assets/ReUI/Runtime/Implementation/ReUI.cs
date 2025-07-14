using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Abyse.ReUI
{
    //TODO : automatic open/close from canvas/prefab 
    //TODO : dialog example
    //TODO : tooltip example
    //TODO : Hero animation example
    //TODO : minimal MVP example
    //TODO : TabBar example
    //TODO : infinite scroll / recycle view with pagination example
    //TODO : carousel example
    //TODO : unitask support ?? nahuya + zachem ?? 
    //TODO : pool clear methods 
    //TODO : documentation for public methods in code 
    //TODO : documentation in readme

    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CanvasScaler))]
    public class ReUI : MonoBehaviour
    {
        [HideInInspector] [SerializeField] private GameObject stash;
        [SerializeField] private WidgetIdPair[] idBoundedWidgets;
        [SerializeField] private Widget[] typeBoundedWidgets;
        [SerializeField] private bool startManually = true;

        private bool _isInitialized;

        private WidgetPool _pool;
        private ServiceLocator _serviceLocator;

        private void Awake()
        {
            if (!startManually)
                Initialize();
        }

        private void Reset()
        {
            EnsureStashCreated();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;
            EnsureStashCreated();
            EnsureDependencies();
            RegisterWidgets();

            _isInitialized = true;
        }

        private void EnsureStashCreated()
        {
            if (stash == null)
                stash = new GameObject("Stash", typeof(RectTransform), typeof(Canvas));
            SetUpStash();
        }

        private void EnsureDependencies()
        {
            _pool ??= new WidgetPool(stash);
            _serviceLocator ??= new ServiceLocator();
        }

        private void RegisterWidgets()
        {
            if (idBoundedWidgets != null)
                foreach (var pair in idBoundedWidgets)
                {
                    if (pair.widget == null)
                        throw new ReUIException($"Widget prefab with id {pair.id} is not assigned.");
                    RegisterWidget(pair.widget, pair.id);
                }

            if (typeBoundedWidgets != null)
                foreach (var widget in typeBoundedWidgets)
                {
                    if (widget == null)
                        throw new ReUIException("Widget in widgets array is not assigned.");
                    RegisterWidget(widget.GetType(), widget);
                }
        }

        private void SetUpStash()
        {
            stash.transform.SetParent(transform, false);
            stash.SetActive(false);

            var stashRect = stash.GetComponent<RectTransform>();
            stashRect.anchorMin = Vector2.zero;
            stashRect.anchorMax = Vector2.one;
            stashRect.offsetMin = Vector2.zero;
            stashRect.offsetMax = Vector2.zero;
        }

        public TWidget Open<TWidget>(string id = null, Transform mountingPoint = null, bool animated = true)
            where TWidget : Widget
        {
            var widget = GetWidget<TWidget>(id, mountingPoint);

            widget.Initialize();

            StartCoroutine(widget.Open(animated));

            widget.OnCloseRequested += Close;

            return widget;
        }

        public void Close(Widget widget, bool animated = true)
        {
            widget.DeInitialize();

            StartCoroutine(WaitClosing());
            return;

            IEnumerator WaitClosing()
            {
                yield return widget.Close(animated);
                ReturnWidget(widget);
            }
        }

        public void RegisterType<T>(T instance, string id = null)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            _serviceLocator.Register(instance, id);
        }

        public void RegisterType<T>(Func<object> factory, string id = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");
            _serviceLocator.Register<T>(factory, id);
        }

        public void RegisterType(Type type, object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            if (type == null || !type.IsAssignableFrom(instance.GetType()))
                throw new ArgumentException("Type must be assignable from the instance type.", nameof(type));

            _serviceLocator.Register(type, instance);
        }

        public void RegisterType(Type type, Func<object> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");
            if (type == null || !typeof(object).IsAssignableFrom(type))
                throw new ArgumentException("Type must be a valid type.", nameof(type));

            _serviceLocator.Register(type, factory);
        }

        public void RegisterWidget<TWidget>(TWidget widget, string id = null) where TWidget : Widget
        {
            if (widget == null)
                throw new ArgumentNullException(nameof(widget), "Widget cannot be null.");

            _pool.Register<TWidget>(widget, id);
        }

        public void RegisterWidget<TWidget>(Func<Widget> factory, string id = null) where TWidget : Widget
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");

            _pool.Register<TWidget>(factory, id);
        }


        public void RegisterWidget(Type widgetType, Widget widget)
        {
            if (widget == null)
                throw new ArgumentNullException(nameof(widget), "Widget cannot be null.");
            if (widgetType == null || !typeof(Widget).IsAssignableFrom(widgetType))
                throw new ArgumentException("Widget type must be a subclass of Widget.", nameof(widgetType));

            _pool.Register(widgetType, widget);
        }


        public void RegisterWidget(Type widgetType, Func<Widget> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");
            if (widgetType == null || !typeof(Widget).IsAssignableFrom(widgetType))
                throw new ArgumentException("Widget type must be a subclass of Widget.", nameof(widgetType));

            _pool.Register(widgetType, factory);
        }

        internal T GetInstance<T>(string id = null)
        {
            return _serviceLocator.Get<T>(id);
        }

        internal TWidget GetWidget<TWidget>(string widgetId = null, Transform mountingPoint = null)
            where TWidget : Widget
        {
            mountingPoint ??= transform;
            var success = _pool.TryGet<TWidget>(out var widget, widgetId);

            if (!success || widget == null)
                throw new ReUIException(
                    $"Widget with id {widgetId} and type {typeof(TWidget)} is not registered in the pool.");

            widget.transform.SetParent(mountingPoint);
            return widget;
        }

        internal void ReturnWidget(Widget widget)
        {
            _pool.TryReturn(widget);
        }

        [Serializable]
        private class WidgetIdPair
        {
            public string id;
            public Widget widget;
        }
    }
}