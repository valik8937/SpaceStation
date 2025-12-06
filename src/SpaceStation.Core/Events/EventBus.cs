namespace SpaceStation.Core.Events;

/// <summary>
/// Central event bus for publishing and subscribing to game events.
/// Replaces DM's RegisterSignal/SendSignal (COMSIG) system.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    /// <summary>
    /// Subscribe to events of a specific type.
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new List<Delegate>();
        }
        _handlers[type].Add(handler);
    }
    
    /// <summary>
    /// Unsubscribe from events of a specific type.
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (_handlers.TryGetValue(type, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
    
    /// <summary>
    /// Raise an event to all subscribers.
    /// </summary>
    public void Raise<T>(T eventData) where T : IEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var handlers))
            return;
            
        foreach (var handler in handlers)
        {
            if (eventData.Handled)
                break;
                
            ((Action<T>)handler)(eventData);
        }
    }
    
    /// <summary>
    /// Raise an event and return whether it was handled.
    /// </summary>
    public bool RaiseAndCheck<T>(T eventData) where T : IEvent
    {
        Raise(eventData);
        return eventData.Handled;
    }
    
    /// <summary>
    /// Clears all event subscriptions.
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
    }
}
