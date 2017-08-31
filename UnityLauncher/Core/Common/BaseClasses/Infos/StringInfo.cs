using System;

namespace UnityLauncher.Core
{
    public class StringInfo : Notifier
    {
        public event Action OnValueChange; 

        public bool NotEmpty => !string.IsNullOrEmpty(value);

        private string value;
        public string Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnValueChange?.Invoke();
                    NotifyPropertyChanged("Value");
                    NotifyPropertyChanged("NotEmpty");
                }
            }
        }
    }
}