using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public static class Behaviours
    {
        private static readonly Dictionary<Type, IList> BehaviourLists = new Dictionary<Type, IList>();
        private static readonly List<IBaseObject> WorkedBehavours = new List<IBaseObject>();
        private static bool isInited;
        
        public static void Init(params IBaseObject[] predefinedBaseObjects)
        {
            if(isInited) return;
            isInited = true;

            if (predefinedBaseObjects != null)
            {
                WorkedBehavours.AddRange(predefinedBaseObjects);
            }

            var behavioursType = typeof(Behaviours).Assembly.GetTypes()
                .Where(type => typeof(IBehaviour).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).ToArray();

            foreach (var type in behavioursType)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as IBehaviour;
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

        public static IList<T> GetBehaviourList<T>() where T: IBehaviour
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
                var receiver = behavour as IMessageReceiver<T>;
                receiver?.OnMessage(message);
            }
        }
    }
}