using System.Windows;

namespace UnityLauncher.Interfaces
{
    public interface IBehaviour : IBaseObject
    {
        FrameworkElement GetControl();
    }
}