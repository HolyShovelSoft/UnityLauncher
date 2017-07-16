using System;
using System.Collections.Generic;
using System.Threading;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public class ThrededJob
    {
        private static readonly Dictionary<string, ThrededJob> Jobs = new Dictionary<string, ThrededJob>();

        private readonly Thread thread;
        private readonly string key;

        private static ThrededJob GetJob(IContext target)
        {
            var context = target?.ContextKey;
            if (string.IsNullOrEmpty(context)) return null;
            ThrededJob job;
            lock (DictLockObject)
            {
                if (!Jobs.TryGetValue(context, out job) || job == null)
                {
                    job = new ThrededJob(context);
                    Jobs[context] = job;
                }
            }
            return job;
        }

        private readonly object queueLockObject = new object();
        private static readonly object DictLockObject = new object();
        private readonly Queue<Action> queuedActions = new Queue<Action>();

        private ThrededJob(string key)
        {
            thread = new Thread(Start);
            this.key = key;
        }

        private bool isRunning;

        public void Run()
        {
            lock (queueLockObject)
            {
                if(isRunning) return;
                isRunning = true;
            }
            thread.Start();
        }

        private void Start()
        {
            bool run;
            do
            {
                Action act = null;
                lock (queueLockObject)
                {
                    if (queuedActions.Count > 0)
                    {
                        act = queuedActions.Dequeue();
                        run = true;
                    }
                    else
                    {
                        run = false;
                    }
                }
                act?.Invoke();
            } while (run);
            lock (queueLockObject)
            {
                isRunning = false;
            }
            lock (DictLockObject)
            {
                Jobs.Remove(key);
            }
        }

        public static void RunJob(IContext target, Action action)
        {
            var job = GetJob(target);
            if (job != null && action != null)
            {
                lock (job.queueLockObject)
                {
                    job.queuedActions.Enqueue(action);
                    if (!job.isRunning)
                    {
                        job.Start();
                    }
                }
            }
        }
    }
}