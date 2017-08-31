using System;
using System.IO;
using System.Threading;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class ProjectInfo : Notifier, IContext
    {
        public static readonly ProjectInfo InvalidProjectInfo = new ProjectInfo
        {
            Idx = -1,
            Author = "",
            Path = "",
            Name = "---",
            Version = "",
        };

        public static ProjectInfo TryGetProjectInfo(string pathCandidate)
        {
            if (string.IsNullOrEmpty(pathCandidate)) return null;
            if (Directory.Exists(pathCandidate) && (Directory.GetFiles(pathCandidate).Length == 0 &&
                                                    Directory.GetDirectories(pathCandidate).Length == 0 ||
                                                    Directory.Exists(pathCandidate + "\\Assets")))
            {
                var normalizedPath = pathCandidate.TrimEnd('\\', '/');

                var project = new ProjectInfo
                {
                    Path = pathCandidate,
                    Name = System.IO.Path.GetFileName(normalizedPath),
                    Author = "unknown",
                    Version = "unknown"
                };
                var t = new Thread(() =>
                {
                    var projectVersionPath = normalizedPath + "\\ProjectSettings\\ProjectVersion.txt";
                    using (FileHelper.LockForFileOperation(projectVersionPath))
                    {
                        if (File.Exists(projectVersionPath))
                        {
                            foreach (var line in File.ReadLines(projectVersionPath))
                            {
                                if (string.IsNullOrEmpty(line)) continue;

                                if (line.Trim().StartsWith("m_EditorVersion"))
                                {
                                    var tmp = line.Replace("m_EditorVersion", "").Replace(":", "").Trim();
                                    if (!string.IsNullOrEmpty(tmp) && Application.Current != null)
                                    {
                                        Application.Current.Dispatcher.BeginInvoke((Action<string>) (ver =>
                                        {
                                            project.Version = ver;
                                        }), tmp);
                                    }
                                }
                            }
                        }
                    }

                    var projectSettingsAsset = normalizedPath + "\\ProjectSettings\\ProjectSettings.asset";
                    using (FileHelper.LockForFileOperation(projectSettingsAsset))
                    {
                        if (File.Exists(projectSettingsAsset))
                        {
                            foreach (var line in File.ReadLines(projectSettingsAsset))
                            {
                                if (string.IsNullOrEmpty(line)) continue;
                                if (line.Trim().StartsWith("organizationId"))
                                {
                                    var tmp = line.Replace("organizationId", "").Replace(":", "").Trim();
                                    if (!string.IsNullOrEmpty(tmp) && Application.Current != null)
                                    {
                                        Application.Current.Dispatcher.BeginInvoke((Action<string>)(auth =>
                                        {
                                            project.Author = auth;
                                        }), tmp);
                                    }
                                }
                            }
                        }
                    }
                    
                });
                t.Start();
                return project;
            }
            return null;
        }

        public int Idx { get; set; }

        private string path;
        public string Path
        {
            get => path;
            private set
            {
                if (path != value)
                {
                    path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        private string name;
        public string Name
        {
            get => name;
            private set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string version;
        public string Version
        {
            get => version;
            private set
            {
                if (version != value)
                {
                    version = value;
                    NotifyPropertyChanged("Version");
                }
            }
        }

        private string author;
        public string Author
        {
            get => author;
            private set
            {
                if (author != value)
                {
                    author = value;
                    NotifyPropertyChanged("Author");
                }
            }
        }

        private ProjectInfo() { }

        public string ContextKey => "ProjectInfo";
    }
}