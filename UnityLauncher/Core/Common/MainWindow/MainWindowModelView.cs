using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Common
{
    public class MainWindowModelView : 
        BaseModelView<MainWindowModel>
    {
        public override string ContextKey => "Default";
        public override IMessageReceiver MessageReceiver => Model;

        private readonly Command launchCommand;
        public ICommand LaunchCommand => launchCommand;

        public MainWindowModelView(FrameworkElement uiElement) : base(uiElement)
        {
            Model.OnSelectedEditorChanged += UpdateEditorProperties;
            launchCommand = new Command(Model.LaunchUnity);
            launchCommand.CanExecute = Model.SelectedEditorInfo != null;
        }

        private void UpdateEditorProperties()
        {
            NotifyPropertyChanged("SelectedEditorIndex");
            launchCommand.CanExecute = Model.SelectedEditorInfo != null;
        }

        public ObservableCollection<EditorInfo> AvailibleEditors => Model.availibleEditors;

        public int SelectedEditorIndex
        {
            get
            {
                var idx = -1;
                var currentInfo = Model.SelectedEditorInfo;
                if (currentInfo != null)
                {
                    var findedInfo = AvailibleEditors.FirstOrDefault(info => info.Path == currentInfo.Path);
                    if (findedInfo != null)
                    {
                        idx = AvailibleEditors.IndexOf(findedInfo);
                    }
                }
                return idx;
            }
            set
            {
                var idx = value;
                if (idx < 0 || idx >= AvailibleEditors.Count)
                {
                    Model.SelectedEditorInfo = null;
                }
                else
                {
                    Model.SelectedEditorInfo = AvailibleEditors[idx];
                }
                launchCommand.CanExecute = Model.SelectedEditorInfo != null;
            }
        }

        public List<FrameworkElement> launchElements;
        public List<FrameworkElement> LaunchElements
        {
            get
            {
                if (launchElements == null)
                {
                    launchElements = new List<FrameworkElement>();
                    var launchBehaviors = Behaviors.GetBehaviourList<ILaunchBehavior>();
                    if (launchBehaviors != null)
                    {
                        foreach (var beh in launchBehaviors)
                        {
                            var ui = beh?.GetControl();
                            if (ui != null)
                            {
                                launchElements.Add(ui);
                            }
                            var launchCommandSource = (beh as ILaunchCommandBehavior)?.LaunchCommandSource;
                            Model.AddLaunchCommandSource(launchCommandSource);
                        }
                    }
                }
                return launchElements;
            }
        }

        public List<FrameworkElement> optionElements;
        public List<FrameworkElement> OptionElements
        {
            get
            {
                if (optionElements == null)
                {
                    optionElements = new List<FrameworkElement>();
                    var optionBehaviors = Behaviors.GetBehaviourList<IOptionsBehavior>();
                    if (optionBehaviors != null)
                    {
                        foreach (var beh in optionBehaviors)
                        {
                            var ui = beh?.GetControl();
                            if (ui != null)
                            {
                                optionElements.Add(ui);
                            }
                        }
                    }
                }
                return optionElements;
            }
        }
    }
}