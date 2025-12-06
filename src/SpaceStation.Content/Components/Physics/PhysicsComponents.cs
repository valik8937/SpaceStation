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
