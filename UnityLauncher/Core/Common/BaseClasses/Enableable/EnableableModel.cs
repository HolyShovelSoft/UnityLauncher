using System;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class EnableableModel : BaseModel
    {
        public event Action OnEnableChange;

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnEnableChange?.Invoke();
                    Settings.Instance.SaveSetting(Context, "IsEnabled", value);
                }
            }
        }

        public override void Init(IContext context)
        {
            base.Init(context);
            isEnabled = Settings.Instance.GetSetting<bool>(Context, "IsEnabled");
        }
    }
}