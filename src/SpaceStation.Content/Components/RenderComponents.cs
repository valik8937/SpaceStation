namespace SpaceStation.Content.Components;

/// <summary>
/// Rendering layer and plane from atoms.dm (layer, plane).
/// Controls the order in which entities are rendered.
/// </summary>
public record struct RenderLayer(
    /// <summary>
    /// Draw order within the same plane. Higher = drawn on top.
    /// Default layers: Turf=2.0, Mob=4.0, Item=3.0, Effect=5.0
    /// </summary>
    float Layer = 2.0f,
    
    /// <summary>
    /// Rendering plane. Higher planes are drawn on top regardless of layer.
    /// Default: 0 = Game plane, -100 = Background, 100 = HUD
    /// </summary>
    int Plane = 0
)
{
    // Common layer constants (from BYOND)
    public const float TurfLayer = 2.0f;
    public const float ObjLayer = 3.0f;
    public const float MobLayer = 4.0f;
    public const float FlyLayer = 5.0f;
    public const float EffectLayer = 5.0f;
    public const float AreaLayer = 1.0f;
    
    // Common plane constants
    public const int BackgroundPlane = -100;
    public const int GamePlane = 0;
    public const int LightingPlane = 50;
    public const int HudPlane = 100;
}

/// <summary>
/// Visibility and opacity from atoms.dm (opacity).
/// Controls whether this entity blocks line of sight.
/// </summary>
public record struct Visibility(
    /// <summary>Whether this entity blocks vision.</summary>
    bool Opaque = false,
    
    /// <summary>Visual transparency (0.0 = invisible, 1.0 = fully visible).</summary>
    float Alpha = 1.0f,
    
    /// <summary>See-invisible threshold for this entity.</summary>
    byte Invisibility = 0
);

/// <summary>
/// Light source from atoms.dm (light_range, light_power, light_color).
/// </summary>
public record struct LightSource(
    /// <summary>Range of light in tiles.</summary>
    float Range = 0f,
    
    /// <summary>Intensity of the light (0.0 to 1.0+).</summary>
    float Power = 1.0f,
    
    /// <summary>Light color as RGB bytes.</summary>
    byte R = 255,
    byte G = 255,
    byte B = 255,
    
    /// <summary>Whether the light is currently on.</summary>
    bool On = true
)
{
    public static readonly LightSource None = new(0f);
    
    public LightSource WithColor(byte r, byte g, byte b) => this with { R = r, G = g, B = b };
    public LightSource WithRange(float range) => this with { Range = range };
}
