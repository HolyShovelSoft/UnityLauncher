using System.Collections.ObjectModel;
using UnityLauncher.Core.Attributes;
using UnityLauncher.Core.LaunchSettings;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.Options
{
    [NonInstanciatedBehavior]
    public class UsersModelView : BaseBehaviorModelView<UsersModel, UsersView>, IOptionsBehavior
    {
        public override string ContextKey => "Users";
        public override IMessageReceiver MessageReceiver => null;
        public override int UiOrder => 1;

        public bool IsDirty
        {
            get => Model.IsDirty;
            set => Model.IsDirty = value;
        }

        public string NewUserName
        {
            get => Model.NewUserName;
            set => Model.NewUserName = value;
        }

        public string NewUserPassword
        {
            get => Model.NewUserPassword;
            set => Model.NewUserPassword = value;
        }


        private bool passwordIsShowed;
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

        public ObservableCollection<UserInfo> Users => Model.users;
        public Command ShowPasswordCommand { get; }
        public Command AddNewUserCommand { get; }
        public Command RemoveUserCommand { get; }
        public Command SaveCommand { get; }
        public Command RevertCommand { get; }
        private bool CanAddNewUser => Model.NewUserCanBeAdded;

        private void CheckAddCommandAvailible()
        {
            AddNewUserCommand.CanExecute = CanAddNewUser;
        }

        public UsersModelView()
        {
            Model.OnNewUserDataChange += OnNewUserDataChange;
            Model.OnDirtyChanged += OnDirtyChanged;

            ShowPasswordCommand = new Command(OnShowPassword);
            AddNewUserCommand = new Command(OnAddNewUser);
            RemoveUserCommand = new Command(RemoveUser);
            SaveCommand = new Command(Model.SaveUsers, IsDirty);
            RevertCommand = new Command(Model.RestoreUsers, IsDirty);

            NotifyPropertyChanged("ShowPasswordCommand");
            NotifyPropertyChanged("RemoveUserCommand");
            NotifyPropertyChanged("AddNewUserCommand");
            NotifyPropertyChanged("PasswordIsShowed");
            NotifyPropertyChanged("NewUserName");
            NotifyPropertyChanged("SaveCommand");
            NotifyPropertyChanged("RevertCommand");

            CheckAddCommandAvailible();
        }

        private void OnDirtyChanged()
        {
            SaveCommand.CanExecute = IsDirty;
            RevertCommand.CanExecute = IsDirty;
        }

        private void OnNewUserDataChange()
        {
            NotifyPropertyChanged("NewUserPassword");
            NotifyPropertyChanged("NewUserName");
            CheckAddCommandAvailible();
        }

        private void OnAddNewUser()
        {
            if(!CanAddNewUser) return;
            Model.AddNewUser();
            PasswordIsShowed = false;
        }

        private void RemoveUser(object param)
        {
            Model.RemoveUser(param as UserInfo);
        }

        private void OnShowPassword(object param)
        {
            if (param == this)
            {
                PasswordIsShowed = !PasswordIsShowed;
                return;    
            }
            var info = param as UserInfo;
            if (info != null)
            {
                info.PasswordIsShowed = !info.PasswordIsShowed;
            }
        }
    }
}