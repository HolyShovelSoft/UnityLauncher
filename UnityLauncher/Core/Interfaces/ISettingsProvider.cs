namespace UnityLauncher.Interfaces
{
    public interface ISettingsProvider
    {
        T GetSetting<T>(IContext context, string key);
        void SaveSetting<T>(IContext context, string key, T value);
        void RemoveSetting(IContext context, string key);
    }
}