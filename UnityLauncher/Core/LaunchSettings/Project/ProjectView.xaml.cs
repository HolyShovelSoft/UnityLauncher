using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : IInitializableView
    {
        public ProjectView() { }

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
