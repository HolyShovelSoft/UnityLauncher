using System.Windows;
using System.Windows.Forms.VisualStyles;

namespace UnityLauncher.Core.Commands.Projects
{
    /// <summary>
    /// Interaction logic for Project.xaml
    /// </summary>
    public partial class Project
    {
        public ProjectInfo Info { get; set; }
        
        public Visibility AdditionalVisibility { get; set; }

        public EditorInfo SelectEditorInfo { get; }

        public string AdditionalInfo => Info !=null && Info.Name != "---"? $"(Unity ver: {Info.Version}, author: {Info.Author})" : "";

        public string VersionInfo => Info != null && Info.Version != "---" && !string.IsNullOrEmpty(SelectEditorInfo?.Version)
            ? (SelectEditorInfo.Version != Info.Version?"Selected another version of editor!":"") : "";

        public Thickness ProjectNameMargin => AdditionalVisibility == Visibility.Visible
            ? new Thickness(0, 0, 0, 0)
            : new Thickness(0, -2, 0, 0);


        public Project(ProjectInfo info, bool showAdditional = false, EditorInfo editorInfo = null)
        {
            AdditionalVisibility = showAdditional? Visibility.Visible : Visibility.Collapsed;
            Info = info;
            SelectEditorInfo = editorInfo;
            InitializeComponent();
        }
    }
}
