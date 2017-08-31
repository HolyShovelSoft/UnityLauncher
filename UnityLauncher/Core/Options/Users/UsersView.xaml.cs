using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : IInitializableView
    {
        public UsersView() { }

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
