using System;
using System.Collections;
using UnityEngine;

namespace Abyse.ReUI
{
    //TODO : automatic open/close from canvas/prefab 
    //TODO : unity animation example
    //TODO : dialog example
    //TODO : tooltip example
    //TODO : Hero animation example
    //TODO : minimal MVVM example
    //TODO : TabBar example
    //TODO : infinite scroll / recycle view with pagination example
    //TODO : carousel example
    //TODO : unitask support ?? nahuya + zachem ?? 
    //TODO : documentation for public methods in code 
    //TODO : pool clear methods 
    //TODO : create widget wizard
    //TODO : documentation in readme

    public class ReUI
    {
        private readonly WidgetPool _pool;
        private readonly UIRoot _root;
        private readonly ServiceLocator _serviceLocator = new();

        public ReUI(UIRoot root)
        {
            _root = root;
            _pool = new WidgetPool(root.Stash);
        }

        public TWidget Open<TWidget>(string id = null, bool animated = true) where TWidget : Widget
        {
            var widget = GetWidget<TWidget>(id);

            widget.Initialize();

            _root.StartCoroutine(widget.Open(animated));

            widget.OnCloseRequested += Close;

            return widget;
        }

        public void Close(Widget widget, bool animated = true)
        {
            widget.DeInitialize();

            _root.StartCoroutine(WaitClosing());
            return;

            IEnumerator WaitClosing()
            {
                yield return widget.Close(animated);
                ReturnWidget(widget);
            }
        }

        public void RegisterType<T>(Func<object> factory, string id = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Factory cannot be null.");
            _serviceLocator.Register<T>(factory, id);
        }

        public void RegisterType<T>(T instance, string id = null)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            _serviceLocator.Register(instance, id);
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

        internal T GetInstance<T>(string id = null)
        {
            return _serviceLocator.Get<T>(id);
        }

        internal TWidget GetWidget<TWidget>(string widgetId = null, Transform mountingPoint = null)
            where TWidget : Widget
        {
            mountingPoint ??= _root.transform;
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
    }
}