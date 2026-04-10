using System;
using System.Collections.Generic;

public static class EventBus<T> where T : struct
{
    private static readonly List<Action<T>> subscribers = new List<Action<T>>();

    public static void Subscribe(Action<T> callback)
    {
        subscribers.Add(callback);
    }

    public static void Unsubscribe(Action<T> callback)
    {
        subscribers.Remove(callback);
    }

    public static void Publish(T eventArgs)
    {
        for (int i = subscribers.Count - 1; i >= 0; i--)
        {
            subscribers[i]?.Invoke(eventArgs);
        }
    }

    public static void Clear()
    {
        subscribers.Clear();
    }
}