using System.Numerics;

namespace SpaceStation.Content.Components;

/// <summary>
/// Target tile for grid-based movement.
/// When present, entity is moving towards target tile.
/// </summary>
public record struct MoveTarget(
    /// <summary>Target tile X coordinate (integer)</summary>
    int TargetX,
    
    /// <summary>Target tile Y coordinate (integer)</summary>
    int TargetY,
    
    /// <summary>Movement progress from 0.0 to 1.0</summary>
    float Progress = 0f,
    
    /// <summary>Movement speed (tiles per second)</summary>
    float Speed = 4f
)
{
    /// <summary>Starting position when movement began.</summary>
    public Vector2 StartPosition { get; init; } = Vector2.Zero;
    
    /// <summary>Target position as Vector2.</summary>
    public readonly Vector2 TargetPosition => new(TargetX, TargetY);
    
    /// <summary>Is movement complete?</summary>
    public readonly bool IsComplete => Progress >= 1f;
}
