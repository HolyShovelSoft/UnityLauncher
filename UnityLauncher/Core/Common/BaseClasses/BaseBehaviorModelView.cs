using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class BaseBehaviorModelView<TModel, TView> : BaseModelView<TModel>, IUIBehavior where TModel : BaseModel, new() where TView : FrameworkElement, new()
    {
        protected TView View { get; }

        protected BaseBehaviorModelView()
        {
            View = new TView();
            Init(View);
        }

        public FrameworkElement GetControl()
        {
            return UiElement;
        }
    }
}