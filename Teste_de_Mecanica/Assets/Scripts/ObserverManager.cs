using System;
using System.Collections.Generic;

public static class ObserverManager
{
    private static Dictionary<string, Action> events =
        new Dictionary<string, Action>();

    public static void Subscribe(string eventName, Action listener)
    {
        if (!events.ContainsKey(eventName))
            events.Add(eventName, listener);
        else
            events[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action listener)
    {
        if (events.ContainsKey(eventName))
            events[eventName] -= listener;
    }

    public static void Notify(string eventName)
    {
        if (events.ContainsKey(eventName))
            events[eventName]?.Invoke();
    }
}