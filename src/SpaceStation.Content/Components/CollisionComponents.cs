namespace SpaceStation.Content.Components;

/// <summary>
/// Collision/density properties from atoms.dm.
/// Controls whether entities block movement.
/// </summary>
public record struct Collision(
    /// <summary>
    /// Whether this entity blocks movement (density).
    /// </summary>
    bool Dense = true,

    /// <summary>
    /// Pass flags for this entity - what can pass through it.
    /// </summary>
    PassFlags PassFlagsSelf = PassFlags.None,

    /// <summary>
    /// Whether thrown items can pass through this.
    /// </summary>
    bool CanPassThrow = false,

    /// <summary>
    /// Whether this is a border object (doors, windows).
    /// Border objects only block from certain directions.
    /// </summary>
    bool OnBorder = false
)
{
    /// <summary>Creates a non-blocking collision.</summary>
    public static Collision NonDense => new(Dense: false);

    /// <summary>Creates a dense collision that blocks everything.</summary>
    public static Collision Solid => new(Dense: true);
}

/// <summary>
/// Flags for what can pass through an entity.
/// From atoms.dm pass_flags_self.
/// </summary>
[Flags]
public enum PassFlags : ushort
{
    None = 0,

    /// <summary>Mobs can pass through.</summary>
    PassMob = 1 << 0,

    /// <summary>Items can pass through.</summary>
    PassItem = 1 << 1,

    /// <summary>Machines can pass through.</summary>
    PassMachine = 1 << 2,

    /// <summary>Thrown objects can pass through.</summary>
    PassThrow = 1 << 3,

    /// <summary>Projectiles can pass through.</summary>
    PassProjectile = 1 << 4,

    /// <summary>Glass-specific flag.</summary>
    PassGlass = 1 << 5,

    /// <summary>Grille-specific flag.</summary>
    PassGrille = 1 << 6,

    /// <summary>Table-specific flag.</summary>
    PassTable = 1 << 7,

    /// <summary>Everything passes.</summary>
    All = 0xFFFF
}

/// <summary>
/// Movement type flags for entities that are moving.
/// From atoms.dm movement_type.
/// </summary>
[Flags]
public enum MovementType : byte
{
    None = 0,

    /// <summary>Normal ground movement.</summary>
    Ground = 1 << 0,

    /// <summary>Flying movement (ignores gravity).</summary>
    Flying = 1 << 1,

    /// <summary>Floating movement (no gravity, no footsteps).</summary>
    Floating = 1 << 2,

    /// <summary>Phasing through walls (ghost mode).</summary>
    Phasing = 1 << 3,

    /// <summary>Ventcrawling movement.</summary>
    VentCrawling = 1 << 4
}
