using System.Windows.Controls;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for RenderModeView.xaml
    /// </summary>
    public partial class RenderModeView : IInitializableView
    {
        public RenderModeView() { }

        public void Init(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
