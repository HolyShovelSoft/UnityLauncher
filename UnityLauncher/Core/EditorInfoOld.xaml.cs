namespace UnityLauncher.Core
{
    /// <summary>
    /// Interaction logic for EditorInfoOld.xaml
    /// </summary>
    public partial class EditorInfoOld
    {
        public string Path { get; }

        public string Version { get; }

        public string UIVersion => string.IsNullOrEmpty(Version)?"":$"{Version} ({(IsX64 ? "x64" : "x86")})";

        public string UIPath => string.IsNullOrEmpty(Path)||Path=="---"?"---":$"path:\"{Path}\"";

        public bool IsX64 { get; }

        public EditorInfoOld(string path)
        {
            Path = path;
            Version = Core.BinnaryHelper.GetUnityVersion(path);
            IsX64 = Core.BinnaryHelper.IsX64(path);
            InitializeComponent();
        }
    }
}
