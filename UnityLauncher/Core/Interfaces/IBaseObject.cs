namespace UnityLauncher.Interfaces
{
    public interface IBaseObject
    {
        bool IsValid { get; }
        string SettingsStoreKey { get; }
    }
}