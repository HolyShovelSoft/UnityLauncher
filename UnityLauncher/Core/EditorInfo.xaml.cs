using System.Windows;
using System.Windows.Controls;

namespace UnityLauncher.Core
{
    /// <summary>
    /// Interaction logic for EditorInfo.xaml
    /// </summary>
    public partial class EditorInfo
    {
        public string Path { get; }

        public string Version { get; }

        public string UIVersion => string.IsNullOrEmpty(Version)?"":$"{Version} ({(IsX64 ? "x64" : "x86")})";

        public string UIPath => string.IsNullOrEmpty(Path)||Path=="---"?"---":$"path:\"{Path}\"";

        public bool IsX64 { get; }

        public EditorInfo(string path)
        {
            Path = path;
            Version = Versions.Version.GetUnityVersion(path);
            IsX64 = Versions.Version.IsX64(path);
            InitializeComponent();
        }
    }
}
