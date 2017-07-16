using UnityLauncher.Core;
using UnityLauncher.Core.Common;

namespace UnityLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var mainWindow = new MainWindowView();
            var mainViewModel = new MainWindowModelView(mainWindow);
            Behaviors.Init(mainViewModel);
            mainWindow.Show();
        }
    }
}
