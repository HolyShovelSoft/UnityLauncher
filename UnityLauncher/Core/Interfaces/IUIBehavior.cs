using System.Windows;

namespace UnityLauncher.Interfaces
{
    public interface IUIBehavior : IBehavior
    {
        FrameworkElement GetControl();
    }
}