namespace UnityLauncher.Interfaces
{
    public interface ICommandBehaviour : ILaunchBehaviour
    {
        string GetCommandLineArguments();
    }
}