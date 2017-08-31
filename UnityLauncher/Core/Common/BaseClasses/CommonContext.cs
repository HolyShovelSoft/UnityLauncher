using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public class CommonContext : IContext
    {
        public CommonContext(string contextKey)
        {
            ContextKey = contextKey;
        }

        public string ContextKey { get; }
    }
}