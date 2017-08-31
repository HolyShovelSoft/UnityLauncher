using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityLauncher.Core.LaunchSettings;
using UnityLauncher.Core.LaunchSettings.Messages;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    public class UsersModel : BaseModel
    {
        public event Action OnNewUserDataChange;
        public event Action OnDirtyChanged; 

        private bool isDirtySelf;

        public bool IsDirty
        {
            get => isDirtySelf || users.Any(info => info.IsDirty);
            set
            {
                if (isDirtySelf != value)
                {
                    isDirtySelf = value;
                    OnDirtyChanged?.Invoke();
                }
            }
        }


        public ObservableCollection<UserInfo> users = new ObservableCollection<UserInfo>();

        private string newUserName;
        public string NewUserName
        {
            get => newUserName;
            set
            {
                if (newUserName != value)
                {
                    newUserName = value;
                    OnNewUserDataChange?.Invoke();
                }
            }
        }

        private string newUserPassword;
        public string NewUserPassword
        {
            get => newUserPassword;
            set
            {
                if (newUserPassword != value)
                {
                    newUserPassword = value;
                    OnNewUserDataChange?.Invoke();
                }
            }
        }

        public bool NewUserCanBeAdded => !(string.IsNullOrEmpty(NewUserName) || users.Any(info => info.Name == NewUserName));

        public void AddNewUser()
        {
            if(!NewUserCanBeAdded) return;
            var newUser = new UserInfo {Name = NewUserName, Password = NewUserPassword, Parent = this};
            newUser.OnDirtyChanged += OnUserDirtyChanged;
            users.Add(newUser);
            NewUserName = "";
            NewUserPassword = "";
            IsDirty = true;
        }

        public void RemoveUser(UserInfo info)
        {
            if (info != null)
            {
                var idx = users.IndexOf(info);
                if (idx >= 0)
                {
                    users.RemoveAt(idx);
                    info.OnDirtyChanged -= OnUserDirtyChanged;
                    IsDirty = true;
                }
            }
        }

        private void OnUserDirtyChanged()
        {
            OnDirtyChanged?.Invoke();
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            LoadConfigUsers();
            IsDirty = false;
        }

        private void LoadConfigUsers()
        {
            var confUsers = Settings.Instance.GetSetting<UserInfo[]>(Context, "StoredUsers");
            foreach (var user in users)
            {
                user.OnDirtyChanged -= OnUserDirtyChanged;
            }
            users.Clear();
            if (confUsers != null)
            {
                foreach (var user in confUsers)
                {
                    if (!string.IsNullOrEmpty(user?.Name))
                    {
                        user.OnDirtyChanged += OnUserDirtyChanged;
                        user.Parent = Context;
                        users.Add(user);
                    }
                }
            }
        }

        public void SaveUsers()
        {
            var toSaveUsers = users.ToArray();
            Settings.Instance.SaveSetting(Context, "StoredUsers", toSaveUsers);
            foreach (var user in users)
            {
                user.IsDirty = false;
            }
            Behaviors.SendMessage(new UserListChanged{users = toSaveUsers});
            IsDirty = false;
        }

        public void RestoreUsers()
        {
            LoadConfigUsers();
            foreach (var user in users)
            {
                user.IsDirty = false;
            }
            IsDirty = false;
        }
    }
}