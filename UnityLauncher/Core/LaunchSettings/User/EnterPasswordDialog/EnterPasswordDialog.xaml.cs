using System.Windows.Controls;

namespace UnityLauncher.Core.LaunchSettings
{
    /// <summary>
    /// Interaction logic for EnterPasswordDialog.xaml
    /// </summary>
    public partial class EnterPasswordDialog
    {
        public string UserNameValue
        {
            get => UserName.Text;
            set => UserName.Text = value;
        }

        public string PasswordValue
        {
            get => PassworBox.Password;
            set => PassworBox.Password = value;
        }

        public EnterPasswordDialog()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button != null)
            {
                PassworBox.ShowPassword = !PassworBox.ShowPassword;
                button.Content = PassworBox.ShowPassword? "Hide Password": "Show Password";
            }
        }
    }
}
