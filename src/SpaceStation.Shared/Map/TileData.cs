using MemoryPack;

namespace SpaceStation.Shared.Map;

/// <summary>
/// Tile data stored in the flat grid array.
/// Optimized for cache-friendly access.
/// </summary>
[MemoryPackable]
public partial struct TileData
{
    /// <summary>
    /// Prototype ID for the turf type.
    /// 0 = space, other values index into prototype table.
    /// </summary>
    public ushort TurfPrototypeId;

    /// <summary>
    /// Tile state flags.
    /// </summary>
    public TileFlags Flags;

    /// <summary>
    /// Current temperature in Kelvin (compressed).
    /// Actual = Temperature * 2 (0-510K range with 0.5K precision).
    /// </summary>
    public byte Temperature;

    /// <summary>
    /// Icon state variant (for smoothing junction or tile overlay).
    /// </summary>
    public byte Variant;

    public TileData(ushort prototypeId = 0)
    {
        TurfPrototypeId = prototypeId;
        Flags = TileFlags.None;
        Temperature = 147; // ~293K (room temperature)
        Variant = 0;
    }

    /// <summary>Gets/sets the actual temperature in Kelvin.</summary>
    public float ActualTemperature
    {
        get => Temperature * 2f;
        set => Temperature = (byte)Math.Clamp(value / 2f, 0, 255);
    }

    public bool Intact
    {
        get => (Flags & TileFlags.Intact) != 0;
        set => Flags = value ? Flags | TileFlags.Intact : Flags & ~TileFlags.Intact;
    }

    public bool BlocksAir
    {
        get => (Flags & TileFlags.BlocksAir) != 0;
        set => Flags = value ? Flags | TileFlags.BlocksAir : Flags & ~TileFlags.BlocksAir;
    }

    public bool Dense
    {
        get => (Flags & TileFlags.Dense) != 0;
        set => Flags = value ? Flags | TileFlags.Dense : Flags & ~TileFlags.Dense;
    }

    public bool Opaque
    {
        get => (Flags & TileFlags.Opaque) != 0;
        set => Flags = value ? Flags | TileFlags.Opaque : Flags & ~TileFlags.Opaque;
    }
}

/// <summary>
/// Bit flags for tile state.
/// </summary>
[Flags]
public enum TileFlags : byte
{
    None = 0,

    /// <summary>Floor plating is intact.</summary>
    Intact = 1 << 0,

    /// <summary>Blocks air flow.</summary>
    BlocksAir = 1 << 1,

    /// <summary>Blocks movement (dense).</summary>
    Dense = 1 << 2,

    /// <summary>Blocks vision (opaque).</summary>
    Opaque = 1 << 3,

    /// <summary>Has atmos simulation running.</summary>
    Simulated = 1 << 4,

    /// <summary>Is a space tile.</summary>
    Space = 1 << 5,

    /// <summary>Needs smoothing update.</summary>
    DirtySmoothing = 1 << 6
}
