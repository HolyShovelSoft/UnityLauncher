using System.Collections.ObjectModel;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class RenderModeModelView : EnableableModelView<RenderModeModel, RenderModeView>, ILaunchCommandBehavior
    {
        public override string ContextKey => "RenderMode";
        public override IMessageReceiver MessageReceiver => null;
        public override int UiOrder => 2;

        public RenderModeInfo Selected
        {
            get => Model.Selected;
            set => Model.Selected = value;
        }

        public ObservableCollection<RenderModeInfo> Modes => Model.modes;

        public RenderModeModelView()
        {
            Model.OnSelectedChange += OnSelectedChange;
        }

        private void OnSelectedChange()
        {
            NotifyPropertyChanged("Selected");
        }

        public ILaunchCommandSource LaunchCommandSource => Model;
    }
}