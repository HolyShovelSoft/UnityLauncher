namespace UnityLauncher.Interfaces
{
    public interface ILaunchCommandBehavior : ILaunchBehavior
    {
        ILaunchCommandSource LaunchCommandSource { get; }
    }
}