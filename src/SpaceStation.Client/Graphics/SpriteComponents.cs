using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceStation.Client.Graphics;

/// <summary>
/// Sprite component for rendering entities.
/// </summary>
public record struct Sprite(
    Texture2D? Texture = null,
    Rectangle SourceRect = default,
    Color Tint = default,
    float Scale = 1f,
    float LayerDepth = 0.5f
)
{
    public Sprite() : this(null, Rectangle.Empty, Color.White, 1f, 0.5f) { }
}

/// <summary>
/// Animated sprite component.
/// </summary>
public struct AnimatedSprite
{
    public Texture2D? Texture;
    public int FrameWidth;
    public int FrameHeight;
    public int CurrentFrame;
    public int TotalFrames;
    public float FrameTime;
    public float TimeAccumulator;
    public bool Loop;

    public Rectangle CurrentSourceRect => new(
        CurrentFrame * FrameWidth,
        0,
        FrameWidth,
        FrameHeight
    );
}

/// <summary>
/// Camera component for view transformation.
/// </summary>
public record struct Camera(
    Vector2 Position = default,
    float Zoom = 1f,
    float Rotation = 0f
)
{
    public readonly Matrix GetTransform(Viewport viewport)
    {
        return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(Zoom, Zoom, 1) *
               Matrix.CreateTranslation(new Vector3(viewport.Width / 2f, viewport.Height / 2f, 0));
    }
}
