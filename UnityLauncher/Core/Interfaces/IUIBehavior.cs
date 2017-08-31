using System.Windows;

namespace UnityLauncher.Interfaces
{
    public interface IUIBehavior : IBehavior
    {
        int UiOrder { get; }
        FrameworkElement GetControl();
    }
}