using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityLauncher.Core.Attributes;
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
                .Where(type => typeof(IUIBehavior).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .Where(type =>
                {
                    var attrs = type.GetCustomAttributes(typeof(NonInstanciatedBehaviorAttribute), false);
                    for (var i = 0; i <= attrs.Length - 1; i++)
                    {
                        var attr = attrs[i] as NonInstanciatedBehaviorAttribute;
                        if (attr != null)
                        {
                            if (string.IsNullOrEmpty(attr.flagPropertyName))
                            {
                                return false;
                            }
                            else
                            {
                                var prop = type.GetProperty(attr.flagPropertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                if (prop != null && prop.PropertyType == typeof(bool))
                                {
                                    return (bool) prop.GetValue(null);
                                }
                            }
                        }
                    }
                    return true;
                }).ToArray();

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