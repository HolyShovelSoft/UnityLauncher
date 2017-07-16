using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public static class Behaviors
    {
        private static readonly Dictionary<Type, IList> BehaviourLists = new Dictionary<Type, IList>();
        private static readonly List<IBehavior> WorkedBehavours = new List<IBehavior>();
        private static bool _isInited;
        
        public static void Init(params IBehavior[] predefinedBehaviors)
        {
            if(_isInited) return;
            _isInited = true;

            if (predefinedBehaviors != null)
            {
                WorkedBehavours.AddRange(predefinedBehaviors);
            }

            var behavioursType = typeof(Behaviors).Assembly.GetTypes()
                .Where(type => typeof(IUIBehavior).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).ToArray();

            foreach (var type in behavioursType)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as IUIBehavior;
                    if (instance != null)
                    {
                        WorkedBehavours.Add(instance);
                    }
                }
                catch
                {
                    //
                }
            }
        }

        public static IList<T> GetBehaviourList<T>() where T: IBehavior
        {
            IList tmp;
            if (!BehaviourLists.TryGetValue(typeof(T), out tmp) || !(tmp is List<T>))
            {
                tmp = new List<T>();

                BehaviourLists[typeof(T)] = tmp;

                for (int i = 0; i <= WorkedBehavours.Count - 1; i++)
                {
                    if (WorkedBehavours[i] is T)
                    {
                        ((List<T>)tmp).Add((T) WorkedBehavours[i]);
                    }
                }
            }
            return ((List<T>) tmp).AsReadOnly();
        }

        public static void SendMessage<T>(T message)
        {
            foreach (var behavour in WorkedBehavours)
            {
                var receiver = behavour?.MessageReceiver as IMessageReceiver<T>;
                receiver?.OnMessage(message);
            }
        }
    }
}