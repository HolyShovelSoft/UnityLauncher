using System.ComponentModel;

namespace UnityLauncher.Core
{
    public abstract class Notifier : INotifyPropertyChanged
    {
        public object parent;

        public object Parent
        {
            get => parent;
            set
            {
                if (value != parent)
                {
                    parent = value;
                    NotifyPropertyChanged("Parent");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}