using System.ComponentModel;
using Newtonsoft.Json;

namespace UnityLauncher.Core
{
    public abstract class Notifier: INotifyPropertyChanged
    {
        [JsonIgnore]
        public object This => this;

        private object parent;
        [JsonIgnore]
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