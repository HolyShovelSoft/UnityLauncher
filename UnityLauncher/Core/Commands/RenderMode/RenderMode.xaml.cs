using System.Windows.Controls;

namespace UnityLauncher.Core.Commands.RenderMode
{
    /// <summary>
    /// Interaction logic for RenderMode.xaml
    /// </summary>
    public partial class RenderMode 
    {
        public string Value { get; }
        public string Command { get; }

        public RenderMode(string value, string command)
        {
            Value = value;
            Command = command;
            InitializeComponent();
        }
    }
}
