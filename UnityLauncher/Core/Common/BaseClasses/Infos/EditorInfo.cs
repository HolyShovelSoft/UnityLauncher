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

        protected EditorInfo(EditorInfo info)
        {
            Path = info.Path;
            Version = info.Version;
            IsX64 = info.IsX64;
        }

        protected void FillVersion()
        {
            Version = BinnaryHelper.GetUnityVersion(path);
            IsX64 = BinnaryHelper.IsX64(path);
        }

        public EditorInfo(string path)
        {
            Path = path;
            FillVersion();
        }
    }
}