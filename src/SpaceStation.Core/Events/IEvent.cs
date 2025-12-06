namespace SpaceStation.Core.Events;

/// <summary>
/// Base interface for all game events.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Whether this event has been handled and should stop propagating.
    /// </summary>
    bool Handled { get; set; }
}
