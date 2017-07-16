using System.Windows;

namespace UnityLauncher.Core
{
    public abstract class EnableableModelView<TModel, TView> : BaseBehaviorModelView<TModel, TView> where TModel : EnableableModel, new() where TView : FrameworkElement, new()
    {
        public bool IsEnabled
        {
            get => Model.IsEnabled;
            set => Model.IsEnabled = value;
        }

        protected EnableableModelView()
        {
            Model.OnEnableChange += () => NotifyPropertyChanged("IsEnabled");
            NotifyPropertyChanged("IsEnabled");
        }
    }
}