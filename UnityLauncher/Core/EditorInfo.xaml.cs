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

        public string UIPath => string.IsNullOrEmpty(Path)||Path=="---"?"---":$"path:\"{Path}\"";

        public EditorInfo(string path)
        {
            Path = path;
            Version = Versions.Version.GetUnityVersion(path);
            InitializeComponent();
        }
    }
}
