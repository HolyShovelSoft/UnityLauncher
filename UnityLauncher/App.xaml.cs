using UnityLauncher.Core;
using UnityLauncher.Core.Common;
using UnityLauncher.Interfaces;

namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            Settings.Init();
            var mainWindow = new MainWindowView();
            var mainViewModel = new MainWindowModelView(mainWindow);
            Behaviors.Init(mainViewModel, (IBehavior) Settings.Instance);
            mainWindow.Init(mainViewModel);
            mainWindow.Show();
        }
    }
}
