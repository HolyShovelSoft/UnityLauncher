namespace UnityLauncher.Interfaces
{
    public interface IBehavior : IContext
    {
        IMessageReceiver MessageReceiver { get; }
    }
}