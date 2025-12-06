using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceStation.Client.Resources;

/// <summary>
/// Sprite data returned by ResourceManager.
/// </summary>
public readonly record struct SpriteData(
    Texture2D Texture,
    Rectangle SourceRect,
    Vector2 Origin
)
{
    public static readonly SpriteData Empty = default;

    public bool IsValid => Texture != null;
}

/// <summary>
/// Animation data for animated sprites.
/// </summary>
public class AnimationData
{
    public Texture2D Texture { get; init; } = null!;
    public AnimationFrame[] Frames { get; init; } = Array.Empty<AnimationFrame>();
    public bool Loop { get; init; } = true;
    public float TotalDuration { get; init; }

    public static readonly AnimationData Empty = new();

    public bool IsValid => Texture != null && Frames.Length > 0;
}

/// <summary>
/// A single frame of animation with timing.
/// </summary>
public readonly record struct AnimationFrame(
    Rectangle SourceRect,
    float Duration,
    float StartTime
);

/// <summary>
/// Direction for directional sprites.
/// </summary>
public enum SpriteDirection
{
    South = 0,
    North = 1,
    East = 2,
    West = 3
}

/// <summary>
/// Extension methods for SpriteDirection.
/// </summary>
public static class SpriteDirectionExtensions
{
    public static string ToJsonKey(this SpriteDirection dir) => dir switch
    {
        SpriteDirection.South => "south",
        SpriteDirection.North => "north",
        SpriteDirection.East => "east",
        SpriteDirection.West => "west",
        _ => "south"
    };

    public static SpriteDirection FromAngle(float radians)
    {
        var degrees = MathHelper.ToDegrees(radians);
        degrees = (degrees % 360 + 360) % 360;

        return degrees switch
        {
            >= 315 or < 45 => SpriteDirection.East,
            >= 45 and < 135 => SpriteDirection.South,
            >= 135 and < 225 => SpriteDirection.West,
            _ => SpriteDirection.North
        };
    }
}
