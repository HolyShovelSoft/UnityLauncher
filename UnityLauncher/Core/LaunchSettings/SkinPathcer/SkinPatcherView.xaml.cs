using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for SkinPatcherView.xaml
    /// </summary>
    public partial class SkinPatcherView : IInitializableView
    {
        public SkinPatcherView() { }

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
