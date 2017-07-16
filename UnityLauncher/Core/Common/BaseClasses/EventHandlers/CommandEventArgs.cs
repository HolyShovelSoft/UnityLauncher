using System;

namespace UnityLauncher.Core
{
    public class CommandEventArgs : EventArgs
    {
        public object Parameter { get; set; }
    }
}