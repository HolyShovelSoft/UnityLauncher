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
        private static readonly List<IBehavour> WorkedBehavours = new List<IBehavour>();
        private static bool isInited;
        
        public static void Init()
        {
            if(isInited) return;
            isInited = true;

            var behavioursType = typeof(Behaviours).Assembly.GetTypes()
                .Where(type => typeof(IBehavour).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

            foreach (var type in behavioursType)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as ICommand;
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

        public static void OnSelectedEditorChange(EditorInfo newEditor)
        {
            for (int i = 0; i <= WorkedBehavours.Count - 1; i++)
            {
                WorkedBehavours[i].OnSelectedEditorUpdate(newEditor);
            }
        }

        public static IList<T> GetBehaviourList<T>() where T: IBehavour
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
    }
}