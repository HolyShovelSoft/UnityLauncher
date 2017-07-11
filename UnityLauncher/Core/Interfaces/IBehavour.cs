using System.Windows;
using UnityLauncher.Core;

namespace UnityLauncher.Interfaces
{
    public interface IBehavour
    {
        FrameworkElement GetControl();
        void OnSelectedEditorUpdate(EditorInfo targetInfo);
        bool IsValid { get; }
    }
}