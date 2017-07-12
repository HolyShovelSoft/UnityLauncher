namespace UnityLauncher.Interfaces
{
    public interface IMessageReceiver<in T>
    {
        void OnMessage(T message);
    }
}