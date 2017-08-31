using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityLauncher.Core.LaunchSettings.Messages;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class UserModel : EnableableModel, IMessageReceiver<UserListChanged>, ILaunchCommandSource
    {

        public event Action OnUserChange;

        public readonly ObservableCollection<UserInfo> availibleUsers = new ObservableCollection<UserInfo>();


        private string enteredPassword;
        public void SetEnteredPassword(string password)
        {
            enteredPassword = password;
        }

        private UserInfo selectedUser;
        public UserInfo SelectedUser
        {
            get => selectedUser;
            set
            {
                if (selectedUser != value)
                {
                    selectedUser = value;
                    if (selectedUser != null)
                    {
                        SelectedUserName = selectedUser.Name;
                    }
                    OnUserChange?.Invoke();
                }
            }
        }

        private string selectedUserName;

        public string SelectedUserName
        {
            get => selectedUserName;
            set 
            {
                if (selectedUserName != value)
                {
                    selectedUserName = value;
                    Settings.Instance.SaveSetting(Context, "LastUser", selectedUserName);
                    TrySetUserByName(selectedUserName);
                    OnUserChange?.Invoke();
                }
            }
        }

        public void OnMessage(UserListChanged message)
        {
            LoadUsersFromArray(message.users);
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            LoadConfigUsers();
        }

        private void TrySetUserByName(string userName)
        {
            SelectedUser = availibleUsers.FirstOrDefault(info => info.Name == userName);
        }

        private void LoadUsersFromArray(UserInfo[] users)
        {
            var lastUser = !string.IsNullOrEmpty(selectedUserName)?selectedUserName:Settings.Instance.GetSetting<string>(Context, "LastUser");
            availibleUsers.Clear();
            if (users != null)
            {
                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user?.Name))
                    {
                        user.Parent = Context;
                        availibleUsers.Add(user);
                    }
                }
            }
            TrySetUserByName(lastUser);
            SelectedUserName = lastUser;
        }

        private void LoadConfigUsers()
        {
            var confUsers = Settings.Instance.GetSetting<UserInfo[]>(Context, "StoredUsers");
            LoadUsersFromArray(confUsers);
        }

        public string CommandLineValue
        {
            get
            {
                if (!IsEnabled || string.IsNullOrEmpty(SelectedUserName))
                {
                    enteredPassword = "";
                    return "";
                }
                var password = string.IsNullOrEmpty(enteredPassword) ? SelectedUser?.Password : enteredPassword;
                if (!string.IsNullOrEmpty(password))
                {
                    return $"-username '{SelectedUserName}' -password '{password}'";
                }
                return "";
            }
        }
    }
}