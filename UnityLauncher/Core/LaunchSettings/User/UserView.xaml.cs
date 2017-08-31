using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : IInitializableView
    {
        public UserView() { }
        
        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
