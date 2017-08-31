using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for CustomView.xaml
    /// </summary>
    public partial class CustomView : IInitializableView
    {
        public CustomView() {}

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
