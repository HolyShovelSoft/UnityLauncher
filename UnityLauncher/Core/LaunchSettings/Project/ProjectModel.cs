using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using UnityLauncher.Core.Common;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class ProjectModel : EnableableModel, ILaunchCommandSource, IMessageReceiver<SelectedEditorChanged>
    {
        public event Action OnSelectedProjectChange;
        public event Action<EditorInfo> OnSelectedEditorChange;

        public string CommandLineValue
        {
            get
            {
                if (!IsEnabled) return "";
                if(SelectedProject == null || !Directory.Exists(SelectedProject.Path)) return "";
                if (!Directory.Exists(SelectedProject.Path + "\\Assets"))
                {
                    Directory.CreateDirectory(SelectedProject.Path + "\\Assets");
                }
                return $"-projectPath \"{SelectedProject.Path}\"";
            }
        }

        public readonly ObservableCollection<ProjectInfo> recentlyUsedProjects = new ObservableCollection<ProjectInfo>();

        public override void Init(IContext context)
        {
            base.Init(context);
            foreach (var project in GetRecentlyUsedProjects())
            {
                recentlyUsedProjects.Add(project);
            }

            var lastSelectedProject = Settings.Instance.GetSetting<string>(Context, "LastSelected");

            TrySetProjectByPath(lastSelectedProject);
        }

        public void OnMessage(SelectedEditorChanged message)
        {
            OnSelectedEditorChange?.Invoke(message.selectEditorInfo);
        }

        private ProjectInfo[] GetRecentlyUsedProjects()
        {
            var tmp = new List<ProjectInfo>();

            var registry = Registry.CurrentUser.OpenSubKey("Software\\Unity Technologies\\Unity Editor 5.x");
            if (registry != null)
            {
                var names = registry.GetValueNames().Where(s => s.StartsWith("RecentlyUsedProjectPaths-") && registry.GetValueKind(s) == RegistryValueKind.Binary).ToArray();
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
                        var proj = ProjectInfo.TryGetProjectInfo(path);
                        if (proj != null)
                        {
                            proj.Idx = idx;
                            proj.Parent = Context;
                            tmp.Add(proj);
                        }
                    }
                    catch
                    {
                        //
                    }
                }
            }

            return tmp.OrderBy(project => project.Idx).ToArray();
        }

        private ProjectInfo selectedProject;
        public ProjectInfo SelectedProject
        {
            get => selectedProject;
            set
            {
                if (value != selectedProject)
                {
                    if(selectedProject != null) selectedProject.PropertyChanged -= SelectedProjectOnPropertyChanged;
                    selectedProject = value;
                    if (selectedProject != null) selectedProject.PropertyChanged += SelectedProjectOnPropertyChanged;
                    OnSelectedProjectChange?.Invoke();
                    Settings.Instance.SaveSetting(Context, "LastSelected", selectedProject?.Path);
                }
            }
        }

        private void SelectedProjectOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Version")
            {
                OnSelectedProjectChange?.Invoke();
            }
        }

        public bool TrySetProjectByPath(string path)
        {
            if(string.IsNullOrEmpty(path)) return false;

            var proj = recentlyUsedProjects.FirstOrDefault(info => info.Path == path);

            SelectedProject = proj;

            if (proj == null)
            {
                proj = ProjectInfo.TryGetProjectInfo(path);
                SelectedProject = proj;
            }

            return proj != null;
        }
    }
}