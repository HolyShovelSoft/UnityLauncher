using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using UnityLauncher.Interfaces;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;
using Microsoft.WindowsAPICodePack.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace UnityLauncher.Core.Commands.Projects
{
    /// <summary>
    /// Interaction logic for ProjectSelect.xaml
    /// </summary>
    public partial class ProjectSelect : ICommandBehaviour, IMessageReceiver<EditorSelectionChangedMessage>
    {
        private const string IncorrectFolderWarningMessage = "Folder \"{0}\" not empty and not contain \"Assets\" subfolder. Do you want select another folder?";

        private static readonly Project NotSelectedPopup = new Project(new ProjectInfo
        {
            Author = "---",
            Idx = -1,
            Name = "---",
            Path = "---",
            Version = "---"
        });

        private static readonly Project NotSelectedFull = new Project(new ProjectInfo
        {
            Author = "---",
            Idx = -1,
            Name = "---",
            Path = "---",
            Version = "---"
        }, true);

        private bool isFilled;
        private bool handUpdate;
        private ComboBox comboBox;
        private TextBox textBox;
        private StackPanel stackPanel;
        private Project selectedProject;

        public ProjectSelect()
        {
            InitializeComponent();
        }

        public FrameworkElement GetControl()
        {
            Fill();
            return this;
        }

        private ProjectInfo GetProjectInfo(string path, int index)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    var name = Path.GetFileName(path);
                    var author = "unknown";
                    var version = "unknown";

                    if (File.Exists(path + "\\ProjectSettings\\ProjectVersion.txt"))
                    {
                        foreach (var line in File.ReadLines(path + "\\ProjectSettings\\ProjectVersion.txt"))
                        {
                            if(string.IsNullOrEmpty(line)) continue;
                            if (line.Trim().StartsWith("m_EditorVersion"))
                            {
                                var tmp = line.Replace("m_EditorVersion", "").Replace(":", "").Trim();
                                if (!string.IsNullOrEmpty(tmp))
                                {
                                    version = tmp;
                                }
                            }
                        }
                    }
                    

                    if (File.Exists(path + "\\ProjectSettings\\ProjectSettings.asset"))
                    {
                        foreach (var line in File.ReadLines(path + "\\ProjectSettings\\ProjectSettings.asset"))
                        {
                            if (string.IsNullOrEmpty(line)) continue;
                            if (line.Trim().StartsWith("organizationId"))
                            {
                                var tmp = line.Replace("organizationId", "").Replace(":", "").Trim();
                                if (!string.IsNullOrEmpty(tmp))
                                {
                                    author = tmp;
                                }
                            }
                        }
                    }

                    var result = new ProjectInfo
                    {
                        Idx = index,
                        Path = path,
                        Name = name,
                        Author = author,
                        Version = version
                    };
                    return result;
                }
            }
            return null;
        }

        private Project[] GetProjects()
        {
            var tmp = new List<Project>();

            var registry = Registry.CurrentUser.OpenSubKey("Software\\Unity Technologies\\Unity Editor 5.x");
            if (registry != null)
            {
                var names = registry.GetValueNames().Where(s => s.StartsWith("RecentlyUsedProjectPaths-") && registry.GetValueKind(s) == RegistryValueKind.Binary);
                foreach (var name in names)
                {
                    try
                    {
                        var path = System.Text.Encoding.UTF8.GetString((byte[])registry.GetValue(name))
                            .TrimEnd('\0');
                        var idxStr = name.Remove(0, "RecentlyUsedProjectPaths-".Length);
                        var numLen = idxStr.IndexOf("_", StringComparison.Ordinal);
                        if (numLen >= 0)
                        {
                            idxStr = idxStr.Substring(0, numLen);
                        }
                        int idx;
                        if (!Int32.TryParse(idxStr, out idx))
                        {
                            idx = 0;
                        }
                        var proj = GetProjectInfo(path, idx);
                        if (proj != null)
                        {
                            tmp.Add(new Project(proj));
                        }
                    }
                    catch 
                    {
                        //
                    }
                }
            }

            return tmp.OrderBy(project => project.Info.Idx).ToArray();
        }

        private void Fill()
        {
            if(isFilled) return;
            isFilled = true;

            comboBox = FindName("RecentlyProjects") as ComboBox;
            if(comboBox == null) return;

            comboBox.Items.Add(NotSelectedPopup);
            handUpdate = true;
            comboBox.SelectedIndex = 0;
            handUpdate = false;

            var projects = GetProjects();
            foreach (var project in projects)
            {
                comboBox.Items.Add(project);
            }

            stackPanel = FindName("InfoPanel") as StackPanel;

            selectedProject = NotSelectedPopup;

            textBox = FindName("PathView") as TextBox;

            UpdateSelection();
        }

        public string GetCommandLineArguments()
        {
            if (!IsValid) return "";
            if (!Directory.Exists(selectedProject.Info.Path + "\\Assets"))
            {
                Directory.CreateDirectory(selectedProject.Info.Path + "\\Assets");
            }
            return $"-projectPath \"{selectedProject.Info.Path}\"";
        }

        public bool IsValid => selectedProject?.Info != null && selectedProject.Info.Path != "---" && Directory.Exists(selectedProject.Info.Path) && 
            (Directory.GetFiles(selectedProject.Info.Path).Length == 0 && Directory.GetDirectories(selectedProject.Info.Path).Length == 0 
            || Directory.Exists(selectedProject.Info.Path + "\\Assets"));

        public string SettingsStoreKey => "Projects";

        private void RecentlyProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(handUpdate) return;
            selectedProject = comboBox.Items.GetItemAt(comboBox.SelectedIndex) as Project;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            stackPanel.Children.Clear();

            if (selectedProject != null)
            {
                var idx = -1;
                for (int i = 0; i <= comboBox.Items.Count - 1; i++)
                {
                    var project = comboBox.Items[i] as Project;
                    if (project != null && project.Info.Path == selectedProject.Info.Path)
                    {
                        idx = i;
                        break;
                    }
                }
                handUpdate = true;
                comboBox.SelectedIndex = idx >= 0 ? idx : 0;
                handUpdate = false;
                stackPanel.Children.Add(new Project(selectedProject.Info, true, MainWindow.Instance.SelectedEditor));
                textBox.Text = selectedProject.Info.Path != "---" ? selectedProject.Info.Path : "";
            }
            else
            {
                handUpdate = true;
                comboBox.SelectedIndex = 0;
                handUpdate = false;
                stackPanel.Children.Add(NotSelectedFull);
                textBox.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            using (var dlg = new CommonOpenFileDialog
            {
                Title = "Select project folder",
                IsFolderPicker = true,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true,
                NavigateToShortcut = true,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false
                })
            {
                if (selectedProject != null && selectedProject.Info.Path != "---")
                {
                    dlg.InitialDirectory = selectedProject.Info.Path.Replace("/", "\\");
                    dlg.DefaultDirectory = selectedProject.Info.Path.Replace("/", "\\");
                }
                else
                {
                    dlg.InitialDirectory = @"C:\";
                    dlg.DefaultDirectory = @"C:\";
                }

                dlg.FolderChanging += (o, args) =>
                {
                    ((CommonOpenFileDialog) o).InitialDirectory = args.Folder;
                    ((CommonOpenFileDialog)o).DefaultDirectory = args.Folder;
                };

                var dialogResult = dlg.ShowDialog(MainWindow.Instance);

                while (dialogResult == CommonFileDialogResult.Ok)
                {
                    var folder = dlg.FileName.Replace("\\", "/");
                    if (Directory.GetFiles(folder).Length == 0 && Directory.GetDirectories(folder).Length == 0 || Directory.Exists(folder + "\\Assets"))
                    {
                        var projInfo = GetProjectInfo(folder, -1);
                        selectedProject = new Project(projInfo);
                        UpdateSelection();
                        break;
                    }
                    var messageResult = MessageBox.Show(MainWindow.Instance, string.Format(IncorrectFolderWarningMessage, folder), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (messageResult == MessageBoxResult.Yes)
                    {
                        dialogResult = dlg.ShowDialog(MainWindow.Instance);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void OnMessage(EditorSelectionChangedMessage message)
        {
            UpdateSelection();
        }
        
    }
}
