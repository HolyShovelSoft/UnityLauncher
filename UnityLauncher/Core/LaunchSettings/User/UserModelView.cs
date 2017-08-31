using System.Collections.ObjectModel;
using UnityLauncher.Core.Attributes;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    [NonInstanciatedBehavior]
    public class UserModelView: EnableableModelView<UserModel, UserView>, ILaunchCommandBehavior
    {
        public override string ContextKey => "Users";
        public override IMessageReceiver MessageReceiver => Model;
        private readonly ProxyLaunchCommandSource proxySource;
        public override int UiOrder => 1;

        private readonly EnterPasswordDialog enterPasswordDialog = new EnterPasswordDialog();
        public ObservableCollection<UserInfo> AvailibleUsers => Model.availibleUsers;

        public UserInfo SelectedUser
        {
            get => Model.SelectedUser;
            set => Model.SelectedUser = value;
        }

        public string SelectedUserName
        {
            get => Model.SelectedUserName;
            set => Model.SelectedUserName = value;
        }

        public ILaunchCommandSource LaunchCommandSource => proxySource;

        public UserModelView()
        {
            Model.OnUserChange += OnUserChange;
            proxySource = new ProxyLaunchCommandSource(Model, OnBeforeProxy, ProcessProxy);
        }

        private void OnUserChange()
        {
            NotifyPropertyChanged("SelectedUser");
            NotifyPropertyChanged("SelectedUserName");
        }

        private void OnBeforeProxy()
        {
            if(!IsEnabled) return;
            if (!string.IsNullOrEmpty(SelectedUserName) && string.IsNullOrEmpty(SelectedUser?.Password))
            {
                enterPasswordDialog.Owner = MainWindowView.Instance;
                enterPasswordDialog.UserNameValue = SelectedUserName;
                enterPasswordDialog.PasswordValue = "";
                var result = enterPasswordDialog.ShowDialog();
                if (result.HasValue)
                {
                    var password = enterPasswordDialog.PasswordValue;
                    enterPasswordDialog.PasswordValue = "";
                    enterPasswordDialog.UserNameValue = "";
                    Model.SetEnteredPassword(password);
                }
            }
        }

        private string ProcessProxy(string value)
        {
            return value;
        }
    }
}