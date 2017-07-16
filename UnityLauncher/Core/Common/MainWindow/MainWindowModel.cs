using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Common
{
    public class MainWindowModel : BaseModel, IMessageReceiver<EditorLocationsChanged>
    {
        private struct PathWithVer
        {
            public string path;
            public int first;
            public int second;
            public int third;
            public long fourth;
        }

        public event Action OnSelectedEditorChanged;

        private readonly List<ILaunchCommandSource> launchCommandSources = new List<ILaunchCommandSource>();
        public readonly ObservableCollection<EditorInfo> availibleEditors = new ObservableCollection<EditorInfo>();

        private EditorInfo selectedEditorInfo;
        public EditorInfo SelectedEditorInfo
        {
            get => selectedEditorInfo;
            set
            {
                if (Equals(selectedEditorInfo, value)) return;
                selectedEditorInfo = value;
                OnSelectedEditorChanged?.Invoke();
                Behaviors.SendMessage(new SelectedEditorChanged{ selectEditorInfo = value });
                Settings.Instance.SaveSetting(Context, "LastSelectedEditor", selectedEditorInfo?.Path);
            }
        }

        public MainWindowModel()
        {
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            LoadPathes();
            Behaviors.SendMessage(new SelectedEditorChanged { selectEditorInfo = selectedEditorInfo });
        }

        private void LoadPathes()
        {
            var searchPathes = Settings.Instance.GetSetting<List<string>>(Context, "SearchPathes");
            if (searchPathes == null)
            {
                searchPathes = Settings.DefaultUnityDirectoryPathes.Distinct().ToList();
                Settings.Instance.SaveSetting(Context, "SearchPathes", searchPathes);
            }
            var searchMasks = Settings.Instance.GetSetting<List<string>>(Context, "SearchMasks");
            if (searchMasks == null)
            {
                searchMasks = Settings.DefaultUnityDirectoryMasks.Distinct().ToList();
                Settings.Instance.SaveSetting(Context, "SearchMasks", searchMasks);
            }

            FillEditors(searchPathes.ToArray(), searchMasks.ToArray());
        }

        private void FillEditors(string[] pathes, string[] masks)
        {
            ThrededJob.RunJob(Context, () =>
            {
                if (Application.Current != null)
                {
                    List<string> tmp = new List<string>();

                    string lastPath ="";

                    Application.Current.Dispatcher.BeginInvoke((Action) (() =>
                    {
                        lastPath = SelectedEditorInfo != null? SelectedEditorInfo.Path : Settings.Instance.GetSetting<string>(Context, "LastSelectedEditor");
                        availibleEditors.Clear();
                    }));

                    if(pathes == null || masks == null) return;
                    foreach (var path in pathes)
                    {
                        foreach (var mask in masks)
                        {
                            var subPathes = Directory.GetDirectories(path, mask)
                                .Where(s => File.Exists(s + "\\Editor\\Unity.exe"));
                            tmp.AddRange(subPathes);
                        }
                    }
                    var locs = tmp.Distinct().Select(s =>
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
                        .Select(ver => ver.path).ToArray();

                    Application.Current.Dispatcher.BeginInvoke((Action<string[]>)(locations =>
                    {
                        foreach (var loc in locations)
                        {
                            EditorInfo editorInfo = new EditorInfo(loc);
                            availibleEditors.Add(editorInfo);
                            if (loc == lastPath && selectedEditorInfo == null)
                            {
                                if (lastPath == loc)
                                {
                                    SelectedEditorInfo = editorInfo;
                                }
                            }
                        }
                    }), new object[]{locs});
                    
                }
            });
        }
        
        public void OnMessage(EditorLocationsChanged message)
        {
            FillEditors(message.pathes, message.masks);
        }

        public void LaunchUnity()
        {
            if (SelectedEditorInfo == null) return;
            if (File.Exists(SelectedEditorInfo.Path + "\\Editor\\Unity.exe"))
            {
                var path = SelectedEditorInfo.Path + "\\Editor\\Unity.exe";
                var commands = GetCommands();
                Process.Start(path, commands);
                MainWindowView.Instance.Close();
            }
        }

        private string GetCommands()
        {
            var commands = launchCommandSources
                .Select(source => source?.CommandLineValue)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            return commands.Length == 0 ? "" : commands.Aggregate((s1, s2) => s1 + " " + s2);
        }

        public void AddLaunchCommandSource(ILaunchCommandSource source)
        {
            if (source != null && launchCommandSources.IndexOf(source) < 0)
            {
                launchCommandSources.Add(source);
            }
        }
    }
}