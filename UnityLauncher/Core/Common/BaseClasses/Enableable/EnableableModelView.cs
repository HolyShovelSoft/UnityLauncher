using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class EnableableModelView<TModel, TView> : BaseBehaviorModelView<TModel, TView> where TModel : EnableableModel, new() where TView : FrameworkElement, IInitializableView , new()
    {
        public virtual bool IsEnabled
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