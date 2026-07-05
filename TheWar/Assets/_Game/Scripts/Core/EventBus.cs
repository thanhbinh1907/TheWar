using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }
            _subscribers[eventType].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                // Create a copy to prevent modified collection exceptions if subscribers unsubscribe during iteration
                var handlers = new List<Delegate>(_subscribers[eventType]);
                foreach (var handler in handlers)
                {
                    if (handler is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}
