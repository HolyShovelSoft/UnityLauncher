using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityLauncher.Core.LaunchSettings;
using System.Threading;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Common
{
    public class MainWindowModel : BaseModel, 
        IMessageReceiver<EditorLocationsChanged>, 
        IMessageReceiver<SelectedProjectChanged>
    {
        private struct PathWithVer
        {
            public string path;
            public int first;
            public int second;
            public int third;
            public int fourth;
            public string letters;
        }

        private Guid lastGuid;
        private string lastSelectedProjectVersion;

        public event Action OnSelectedEditorChanged;

        private readonly List<ILaunchCommandSource> launchCommandSources = new List<ILaunchCommandSource>();
        public readonly ObservableCollection<EditorInfo> availibleEditors = new ObservableCollection<EditorInfo>();

        public event Action OnSelecteNeededVersionChanged;
        private bool selectNeededVersion = false;

        public bool SelectNeededVersion
        {
            get => selectNeededVersion;
            set
            {
                if(selectNeededVersion == value) return;
                selectNeededVersion = value;
                OnSelecteNeededVersionChanged?.Invoke();
                Settings.Instance.SaveSetting(Context, "MustSelectNeededVersion", SelectNeededVersion);
                if (value)
                {
                    TrySelectCorrectVersion(lastSelectedProjectVersion);
                }
            }
        }

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
            SelectNeededVersion = Settings.Instance.GetSetting<bool>(Context, "MustSelectNeededVersion");
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
            lastGuid = Guid.NewGuid();
            var tmpGuid = lastGuid;
            var t = new Thread(() =>
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
                            var version = BinnaryHelper.GetUnityVersion(s);
                            var splitVersion = version.Split('.');
                            var pathVer = new PathWithVer
                            {
                                path = s,
                            };
                            if (splitVersion?.Length >= 3)
                            {
                                Int32.TryParse(splitVersion[0], out pathVer.first);
                                Int32.TryParse(splitVersion[1], out pathVer.second);
                                var splitMinor = Regex.Split(splitVersion[2], "[\\D]");
                                pathVer.letters = Regex.Match(splitVersion[2], "[\\D]").ToString().ToLower();
                                if (splitMinor?.Length >= 2)
                                {
                                    Int32.TryParse(splitMinor[0], out pathVer.third);
                                    Int32.TryParse(splitMinor[1], out pathVer.fourth);
                                }
                            }
                            return pathVer;
                        })
                        .OrderBy(ver => ver.first)
                        .ThenBy(ver => ver.second)
                        .ThenBy(ver => ver.third)
                        .ThenBy(ver => ver.letters)
                        .ThenBy(ver => ver.fourth)
                        .ThenBy(ver => ver.path)
                        .Select(ver => ver.path).ToArray();

                    Application.Current.Dispatcher.BeginInvoke((Action<string[]>)(locations =>
                    {
                        if (lastGuid != tmpGuid)
                        {
                            return;
                        }
                        EditorInfo selected = null;
                        foreach (var loc in locations)
                        {
                            EditorInfo editorInfo = new EditorInfo(loc);
                            availibleEditors.Add(editorInfo);
                            if (lastPath == loc)
                            {
                                selected = editorInfo;
                            }
                        }
                        SelectedEditorInfo = selected;
                        if (selectNeededVersion)
                        {
                            TrySelectCorrectVersion(lastSelectedProjectVersion);
                        }
                        Behaviors.SendMessage(new EditorsFilled{infos = availibleEditors.ToArray()});
                    }), new object[]{locs});
                }
            });
            t.Start();
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
                using (FileHelper.LockForFileOperation(path))
                {
                    var commands = GetCommands();
                    var pid = Process.Start(path, commands);
                    var t = pid.MainWindowTitle;

                }
                MainWindowView.Instance.Close();
                Application.Current.Shutdown();
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

        private void TrySelectCorrectVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return;
            if (SelectedEditorInfo?.Version == version) return;
            var neededEditor = availibleEditors.FirstOrDefault(info => info != null && info.Version == version);
            if (neededEditor != null)
            {
                SelectedEditorInfo = neededEditor;
            }
        }

        public void OnMessage(SelectedProjectChanged message)
        {
            lastSelectedProjectVersion = message.selectedProject?.Version;
            if (!SelectNeededVersion) return;
            TrySelectCorrectVersion(lastSelectedProjectVersion);
        }
    }
}