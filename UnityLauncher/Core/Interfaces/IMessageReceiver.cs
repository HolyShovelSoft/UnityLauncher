namespace UnityLauncher.Interfaces
{
    public interface IMessageReceiver{ }

    public interface IMessageReceiver<in T> : IMessageReceiver
    {
        void OnMessage(T message);
    }
}