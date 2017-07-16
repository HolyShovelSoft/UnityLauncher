namespace UnityLauncher.Core.LaunchSettings
{
    public class RenderModeInfo : Notifier
    {
        public string Value { get; }
        public string Command { get; }

        public RenderModeInfo(string value, string command, object parent)
        {
            Value = value;
            Command = command;
            Parent = parent;
        }
    }
}