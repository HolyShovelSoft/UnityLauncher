using System;
using Newtonsoft.Json;
using UnityLauncher.Core.Attributes;

namespace UnityLauncher.Core.LaunchSettings
{
    public class UserInfo : Notifier
    {
        public event Action OnDirtyChanged;

        private bool isDirty;
        [JsonIgnore]
        public bool IsDirty
        {
            get => isDirty;
            set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnDirtyChanged?.Invoke();
                }
            }
        }

        private bool passwordIsShowed;
        [JsonIgnore]
        public bool PasswordIsShowed
        {
            get => passwordIsShowed;
            set
            {
                if (passwordIsShowed != value)
                {
                    passwordIsShowed = value;
                    NotifyPropertyChanged("PasswordIsShowed");
                }
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    IsDirty = true;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string password;

        [JsonEncrypt]
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    IsDirty = true;
                    NotifyPropertyChanged("Password");
                }
            }
        }
    }
}