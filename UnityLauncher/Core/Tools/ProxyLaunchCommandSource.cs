using System;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public class ProxyLaunchCommandSource : ILaunchCommandSource
    {
        private readonly ILaunchCommandSource wrappedSource;
        private readonly Action beforeProxy;
        private readonly Func<string, string> proxyProcess;

        public ProxyLaunchCommandSource(ILaunchCommandSource wrappedSource, Action beforeProxy, Func<string, string> proxyProcess)
        {
            this.wrappedSource = wrappedSource;
            this.beforeProxy = beforeProxy;
            this.proxyProcess = proxyProcess;
        }

        public string CommandLineValue
        {
            get
            {
                beforeProxy?.Invoke();

                var result = wrappedSource?.CommandLineValue ?? "";
                if (proxyProcess != null)
                {
                    result = proxyProcess(result);
                }

                return result;
            }
        }

    }
}