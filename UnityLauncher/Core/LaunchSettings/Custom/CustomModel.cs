using System.Collections.ObjectModel;
using System.Linq;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core.LaunchSettings
{
    public class CustomModel : EnableableModel, ILaunchCommandSource
    {
        public readonly ObservableCollection<StringInfo> commands = new ObservableCollection<StringInfo>();

        public override void Init(IContext context)
        {
            base.Init(context);
            LoadFromSettings();
        }

        private void LoadFromSettings()
        {
            var confCommands = Settings.Instance.GetSetting<string[]>(Context, "Commands");
            foreach (var command in commands)
            {
                command.OnValueChange -= OnStringChanged;
            }
            commands.Clear();
            if (confCommands != null)
            {
                foreach (var confCommand in confCommands)
                {
                    if (!string.IsNullOrEmpty(confCommand) && commands.All(info => info.Value != confCommand))
                    {
                        var info = new StringInfo {Value = confCommand, Parent = Context};
                        info.OnValueChange += OnStringChanged;
                        commands.Add(info);
                    }
                }
            }
        }

        private void OnStringChanged()
        {
            Settings.Instance.SaveSetting(Context, "Commands", commands.Select(info => info.Value).ToArray());
        }

        public string CommandLineValue
        {
            get
            {
                if (!IsEnabled) return "";
                var forAgr = commands.Where(info => info!= null).Select(info => info.Value).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                return forAgr.Length == 0 ? "" : forAgr.Aggregate((s1, s2) => s1 + " " + s2);
            }
        }

        public bool AddCommand(string command)
        {
            if(string.IsNullOrEmpty(command) || commands.Any(info => info.Value == command)) return false;
            var stringInfo = new StringInfo {Value = command, Parent = Context};
            stringInfo.OnValueChange += OnStringChanged;
            commands.Add(stringInfo);
            Settings.Instance.SaveSetting(Context, "Commands", commands.Select(info => info.Value).ToArray());
            return true;
        }

        public void RemoveCommand(int index)
        {
            if(index < 0 || index>= commands.Count) return;
            var info = commands[index];
            info.OnValueChange -= OnStringChanged;
            commands.RemoveAt(index);
            Settings.Instance.SaveSetting(Context, "Commands", commands.ToArray());
        }
    }
}