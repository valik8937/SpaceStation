using Arch.Core;

namespace SpaceStation.Core.Events;

/// <summary>
/// Base class for events that involve an entity.
/// </summary>
public abstract class EntityEventArgs : IEvent
{
    public Entity Entity { get; }
    public bool Handled { get; set; }

    protected EntityEventArgs(Entity entity)
    {
        Entity = entity;
    }
}

/// <summary>
/// Event raised when an entity takes damage.
/// </summary>
public sealed class DamageEvent : EntityEventArgs
{
    public float Amount { get; set; }
    public string DamageType { get; init; }
    public Entity? Source { get; init; }

    public DamageEvent(Entity entity, float amount, string damageType, Entity? source = null)
        : base(entity)
    {
        Amount = amount;
        DamageType = damageType;
        Source = source;
    }
}

/// <summary>
/// Event raised when an entity moves.
/// </summary>
public sealed class MoveEvent : EntityEventArgs
{
    public System.Numerics.Vector2 OldPosition { get; }
    public System.Numerics.Vector2 NewPosition { get; }

    public MoveEvent(Entity entity, System.Numerics.Vector2 oldPos, System.Numerics.Vector2 newPos)
        : base(entity)
    {
        OldPosition = oldPos;
        NewPosition = newPos;
    }
}

/// <summary>
/// Event raised when an entity interacts with another.
/// </summary>
public sealed class InteractEvent : EntityEventArgs
{
    public Entity Target { get; }
    public string InteractionType { get; }

    public InteractEvent(Entity source, Entity target, string interactionType)
        : base(source)
    {
        Target = target;
        InteractionType = interactionType;
    }
}
