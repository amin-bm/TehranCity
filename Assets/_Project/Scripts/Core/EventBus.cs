using System;
using System.Collections.Generic;

public interface IEventBus
{
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
    void Publish<T>(T evt);
}

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, Delegate> _table = new();

    public void Subscribe<T>(Action<T> handler)
    {
        _table.TryGetValue(typeof(T), out var d);
        _table[typeof(T)] = Delegate.Combine(d, handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (_table.TryGetValue(typeof(T), out var d))
            _table[typeof(T)] = Delegate.Remove(d as Action<T>, handler);
    }

    public void Publish<T>(T evt)
    {
        if (_table.TryGetValue(typeof(T), out var d))
            (d as Action<T>)?.Invoke(evt);
    }
}