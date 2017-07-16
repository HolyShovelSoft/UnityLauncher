using System;
using System.Windows;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public class EditorInfo : Notifier, IContext
    {
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

        private string version;
        public string Version
        {
            get => version;
            set
            {
                if (version != value)
                {
                    version = value;
                    NotifyPropertyChanged("Version");
                }
            }
        }

        private bool? isX64;

        public bool? IsX64
        {
            get => isX64;
            set
            {
                if (isX64 != value)
                {
                    isX64 = value;
                    NotifyPropertyChanged("IsX64");
                }
            }
        }

        public string ContextKey => "EditorInfo";

        public EditorInfo(string path)
        {
            Path = path;
            ThrededJob.RunJob(this, () =>
            {
                var newVersion = BinnaryHelper.GetUnityVersion(path);
                var newIs64 = BinnaryHelper.IsX64(path);
                Application.Current.Dispatcher.BeginInvoke((Action<string, bool?>) ((ver, is64) =>
                {
                    Version = ver;
                    IsX64 = is64;
                }), 
                newVersion, newIs64);
            });
        }
    }
}