using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using UnityLauncher.Core;
using UnityLauncher.Interfaces;

namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly string[] UnityDirectoryPathes = {
            "C:\\Program Files\\",
            "C:\\Program Files (x86)\\"
        };

        private StackPanel panel;
        private ComboBox unityVersion;
        private Button launch;
        private static MainWindow _instance;
        public static MainWindow Instance => _instance;
        private EditorInfo selectedEditor;

        private struct PathWithVer
        {
            public string path;
            public int first;
            public int second;
            public int third;
            public long fourth;
        }

        public EditorInfo SelectedEditor
        {
            get => selectedEditor;
            private set
            {
                if (ReferenceEquals(selectedEditor, value)) return;
                selectedEditor = value;
                Behaviours.OnSelectedEditorChange(selectedEditor);
            }
        }

        public MainWindow()
        {
            _instance = this;
            InitializeComponent();
            FillElements();
        }

        private void FillElements()
        {
            FillPanel();
            FillunityVersions();
        }

        private void FillPanel()
        {
            Behaviours.Init();
            panel = FindName("Panel") as StackPanel;
            if(panel == null) return;
            var list = Behaviours.GetBehaviourList<ICommand>();
            for (int i = 0; i <= list.Count - 1; i++)
            {
                var control = list[i].GetControl();
                if (control == null) continue;
                panel.Children.Add(control);
            }
        }

        private void FillunityVersions()
        {
            unityVersion = FindName("UnityVersion") as ComboBox;
            launch = FindName("LaunchButton") as Button;
            unityVersion.Items.Add(new EditorInfo("---"));
            
            var idx = 0;

            var lastProject = Settings.GetMainSetting("LastSelectedProject");

            for (int i = 0; i <= UnityDirectoryPathes.Length - 1; i++)
            {
                var parentPath = UnityDirectoryPathes[i];
                if (Directory.Exists(parentPath))
                {
                    var pathes = Directory.GetDirectories(parentPath, "Unity*")
                        .Where(s => File.Exists(s + "\\Editor\\Unity.exe"))
                        .Select(s =>
                        {
                            var verInfo = FileVersionInfo.GetVersionInfo(s + "\\Editor\\Unity.exe");
                            var pathVer = new PathWithVer
                            {
                                path = s,
                                first = verInfo.FileMajorPart,
                                second = verInfo.FileMinorPart,
                                third = verInfo.FileBuildPart
                            };
                            var lastPartStr =
                                verInfo.FileVersion.Replace($"{pathVer.first}.{pathVer.second}.{pathVer.third}.", "");
                            Int64.TryParse(lastPartStr, out pathVer.fourth);
                            return pathVer;
                        })
                        .OrderBy(ver => ver.first)
                        .ThenBy(ver => ver.second)
                        .ThenBy(ver => ver.third)
                        .ThenBy(ver => ver.fourth)
                        .Select(ver => ver.path)
                        .ToArray();

                    for (int j = 0; j <= pathes.Length - 1; j++)
                    {
                        var currentPath = pathes[j] + "\\Editor\\Unity.exe";
                        if (File.Exists(currentPath)){
                            
                            unityVersion.Items.Add(new EditorInfo(pathes[j]));
                            if (pathes[j] == lastProject)
                            {
                                idx = j + 1;
                            }
                        }
                    }
                }
            }

            if (idx == 0)
            {
                Settings.RemoveMainSetting("LastSelectedProject");
            }
            unityVersion.SelectedIndex = idx;
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
                Settings.RemoveMainSetting("LastSelectedProject");
            }
            else
            {
                Settings.SaveMainSetting("LastSelectedProject", SelectedEditor.Path);
            }
        }

        private string GetCommands()
        {
            string result = "";
            var list = Behaviours.GetBehaviourList<ICommand>();
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
    }
}
