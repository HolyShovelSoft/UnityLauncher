using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityLauncher.Core
{
    public static class FileHelper
    {
        private class LockerImpl : IDisposable
        {
            private object locker;

            public LockerImpl(object locker)
            {
                this.locker = locker;

                if (locker != null)
                {
                    Monitor.Enter(locker);
                }
            }

            public void Dispose()
            {
                if (locker != null)
                {
                    Monitor.Exit(locker);
                }
            }
        }

        private static readonly Dictionary<string, object> Lockers = new Dictionary<string, object>();

        private static object GetLocker(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            object locker;
            if (!Lockers.TryGetValue(path, out locker) || locker == null)
            {
                locker = new object();
                Lockers[path] = locker;
            }
            return locker;
        }

        public static IDisposable LockForFileOperation(string filePath)
        {
            return new LockerImpl(GetLocker(filePath));
        }
    }
}