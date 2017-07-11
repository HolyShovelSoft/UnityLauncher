namespace UnityLauncher.Interfaces
{
    public interface ICommand : IBehavour
    {
        string GetCommandLineArguments();
    }
}