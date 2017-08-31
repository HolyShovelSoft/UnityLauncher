using System.Collections.ObjectModel;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class CustomModelView : EnableableModelView<CustomModel, CustomView>, ILaunchCommandBehavior
    {
        public override string ContextKey => "Custom";
        public override IMessageReceiver MessageReceiver => null;
        public ILaunchCommandSource LaunchCommandSource => Model;
        public ObservableCollection<StringInfo> Commands => Model.commands;
        private StringInfo newCommand;
        public override int UiOrder => 3;

        public StringInfo NewCommand => newCommand ?? (newCommand = new StringInfo {Parent = this, Value = ""});
        public Command AddNewCommand { get; }
        public Command RemoveCommand { get; }

        public CustomModelView()
        {
            AddNewCommand = new Command(AddNewValue);
            RemoveCommand = new Command(RemoveValue);
            NotifyPropertyChanged("AddNewCommand");
            NotifyPropertyChanged("RemoveCommand");
        }

        private void AddNewValue(object param)
        {
            var info = param as StringInfo;
            if (info != null)
            {
                if (Model.AddCommand(info.Value))
                {
                    NewCommand.Value = "";
                }
            }
        }

        private void RemoveValue(object param)
        {
            var info = param as StringInfo;
            if (info != null)
            {
                Model.RemoveCommand(Commands.IndexOf(info));
            }
        }
    }
}