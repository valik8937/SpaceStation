using System.Numerics;

namespace SpaceStation.Content.Components;

/// <summary>
/// Transform component for position and rotation.
/// </summary>
public record struct Transform(
    Vector2 Position = default,
    float Rotation = 0f,
    int ZLevel = 0
);

/// <summary>
/// Physics component for movement and collision.
/// </summary>
public record struct Physics(
    Vector2 Velocity = default,
    float MoveSpeed = 1.0f,
    float Mass = 1.0f,
    float Friction = 0.5f,
    bool Dense = true,
    bool Anchored = false
);

/// <summary>
/// Collision component for collision detection.
/// </summary>
public record struct Collision(
    int Layer = 1,
    int Mask = -1,
    Vector2 Size = default
)
{
    public Collision() : this(1, -1, new Vector2(1f, 1f)) { }
}
