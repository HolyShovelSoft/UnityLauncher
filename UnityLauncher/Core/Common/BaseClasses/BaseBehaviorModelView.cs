using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class BaseBehaviorModelView<TModel, TView> : BaseModelView<TModel>, IUIBehavior where TModel : BaseModel, new() where TView : FrameworkElement, IInitializableView, new()
    {
        protected TView View { get; }

        protected BaseBehaviorModelView()
        {
            View = new TView();
            Init(View);
            View.Init(this);
        }

        public abstract int UiOrder { get; }

        public FrameworkElement GetControl()
        {
            return UiElement;
        }
    }
}