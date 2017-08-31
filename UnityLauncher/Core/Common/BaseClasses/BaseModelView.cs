using System.Collections.Generic;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class BaseModelView : Notifier, IBehavior
    {
        protected static readonly Dictionary<FrameworkElement, BaseModelView> ViewToModelView = new Dictionary<FrameworkElement, BaseModelView>();

        public static BaseModelView GetModelView(FrameworkElement uiElement)
        {
            if (uiElement == null) return null;
            BaseModelView result;
            ViewToModelView.TryGetValue(uiElement, out result);
            return result;
        }

        public abstract FrameworkElement UiElement { get; protected set; }
        public abstract string ContextKey { get; }
        public abstract IMessageReceiver MessageReceiver { get; }
    }

    public abstract class BaseModelView<T> : BaseModelView where T: BaseModel, new()
    {
        public T Model { get; private set; }

        public sealed override FrameworkElement UiElement { get; protected set; }

        protected void Init(FrameworkElement uiElement)
        {
            UiElement = uiElement;
            if (uiElement != null)
            {
                ViewToModelView[uiElement] = this;
                uiElement.DataContext = this;
            }
            Model = new T();
            Model.Init(this);
        }

        protected BaseModelView(FrameworkElement uiElement)
        {
            Init(uiElement);
        }

        protected BaseModelView() { }
    }
}