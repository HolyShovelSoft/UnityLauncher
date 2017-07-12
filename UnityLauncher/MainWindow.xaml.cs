using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using UnityLauncher.Core;
using UnityLauncher.Core.Commands.Projects;
using UnityLauncher.Core.Options;
using UnityLauncher.Interfaces;

namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IBaseObject, IMessageReceiver<EditorLocations>
    {
        private StackPanel panel;
        private ComboBox unityVersion;
        private Button launch;
        private static MainWindow _instance;
        public static MainWindow Instance => _instance;
        private EditorInfo selectedEditor;
        private string[] editorLoacations = new string[0];
        
        public EditorInfo SelectedEditor
        {
            get => selectedEditor;
            private set
            {
                if (ReferenceEquals(selectedEditor, value)) return;
                selectedEditor = value;
            }
        }

        public MainWindow()
        {
            Settings.Init();
            _instance = this;
            Behaviours.Init(this);
            InitializeComponent();
            FillElements();
        }

        private void FillElements()
        {
            FillParamsPanel();
            FillOptionsPanel();
            FillEditorSelection();
        }

        private void FillOptionsPanel()
        {
            panel = FindName("Options") as StackPanel;
            if (panel == null) return;
            var list = Behaviours.GetBehaviourList<IOptionsBehaviour>();
            for (int i = 0; i <= list.Count - 1; i++)
            {
                var control = list[i].GetControl();
                if (control == null) continue;
                panel.Children.Add(control);
            }
        }

        private void FillParamsPanel()
        {
            panel = FindName("Params") as StackPanel;
            if(panel == null) return;
            var list = Behaviours.GetBehaviourList<ILaunchBehaviour>();
            for (int i = 0; i <= list.Count - 1; i++)
            {
                var control = list[i].GetControl();
                if (control == null) continue;
                panel.Children.Add(control);
            }
        }

        private void UpdateLocationList()
        {
            if(unityVersion == null) return;

            var lastProject = SelectedEditor != null ? SelectedEditor.Path : Settings.GetSetting<string>(this, "LastSelectedProject");

            unityVersion.Items.Clear();
            unityVersion.Items.Add(new EditorInfo("---"));

            var idx = 0;

            int i = 1;

            if (editorLoacations != null)
            {
                foreach (var location in editorLoacations)
                {
                    unityVersion.Items.Add(new EditorInfo(location));
                    if (location == lastProject)
                    {
                        idx = i;
                    }
                    i++;
                }
            }
            if (idx == 0)
            {
                Settings.RemoveSetting(this, "LastSelectedProject");
            }
            unityVersion.SelectedIndex = idx;
        }

        private void FillEditorSelection()
        {
            unityVersion = FindName("UnityVersion") as ComboBox;
            launch = FindName("LaunchButton") as Button;
            unityVersion.Items.Add(new EditorInfo("---"));
            UpdateLocationList();
        }

        private void UnityVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (unityVersion.SelectedIndex <= 0)
            {
                SelectedEditor = null;
            }
            else
            {
                SelectedEditor = unityVersion.Items[unityVersion.SelectedIndex] as EditorInfo;
            }
            launch.IsEnabled = SelectedEditor != null;
            if (SelectedEditor == null)
            {
                Settings.RemoveSetting(this, "LastSelectedProject");
            }
            else
            {
                Settings.SaveSetting(this, "LastSelectedProject", SelectedEditor.Path);
            }
            Behaviours.SendMessage(new EditorSelectionChangedMessage(SelectedEditor));
        }

        private string GetCommands()
        {
            string result = "";
            var list = Behaviours.GetBehaviourList<ICommandBehaviour>();
            for (int i = 0; i <= list.Count - 1; i++)
            {
                var producer = list[i];
                if (producer.IsValid)
                {
                    var str = producer.GetCommandLineArguments();
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    result += " " + str;
                }
            }
            return result;
        }

        private void LaunchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(SelectedEditor == null) return;
            if (SelectedEditor.Path != "---" && File.Exists(SelectedEditor.Path + "\\Editor\\Unity.exe"))
            {
                var path = SelectedEditor.Path + "\\Editor\\Unity.exe";
                Process.Start(path, GetCommands());
                Close();
            }
        }

        public bool IsValid => true;
        public string SettingsStoreKey => "";

        
        public void OnMessage(EditorLocations message)
        {
            editorLoacations = message.pathes ?? new string[0];
            UpdateLocationList();
        }
    }
}
