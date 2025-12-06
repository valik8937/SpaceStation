namespace SpaceStation.Content.Components;

/// <summary>
/// Turf-specific data from turf.dm.
/// Represents floor/wall tile properties.
/// </summary>
public record struct Turf(
    /// <summary>
    /// Whether the turf's floor plating is intact.
    /// Affects what objects beneath are visible.
    /// </summary>
    bool Intact = true,
    
    /// <summary>
    /// Whether this turf blocks air flow.
    /// Used by atmos simulation.
    /// </summary>
    bool BlocksAir = false,
    
    /// <summary>
    /// Current temperature in Kelvin.
    /// T20C = 293.15K (room temperature)
    /// </summary>
    float Temperature = 293.15f,
    
    /// <summary>
    /// Maximum fire temperature this turf has been exposed to.
    /// Used for melting/destruction logic.
    /// </summary>
    float MaxFireTemperature = 0f,
    
    /// <summary>
    /// Whether this turf is holodeck-compatible.
    /// </summary>
    bool HolodeckCompatible = false
);

/// <summary>
/// Icon smoothing data from atoms.dm (smoothing_flags, smoothing_junction).
/// Used for auto-tiling walls and floors.
/// </summary>
public record struct Smoothing(
    /// <summary>Smoothing mode flags.</summary>
    SmoothingFlags Flags = SmoothingFlags.None,
    
    /// <summary>
    /// Current junction bitmask (0-255).
    /// Determines which neighbor connections exist.
    /// </summary>
    byte Junction = 0,
    
    /// <summary>Smoothing group ID this tile belongs to.</summary>
    ushort GroupId = 0
);

/// <summary>
/// Smoothing behavior flags.
/// </summary>
[Flags]
public enum SmoothingFlags : byte
{
    None = 0,
    
    /// <summary>Corner-based smoothing (RPGmaker style).</summary>
    Corners = 1 << 0,
    
    /// <summary>Bitmask-based smoothing (blob style).</summary>
    Bitmask = 1 << 1,
    
    /// <summary>Smooths with objects on the turf, not just turfs.</summary>
    SmoothObjects = 1 << 2,
    
    /// <summary>Currently queued for smoothing update.</summary>
    Queued = 1 << 3
}

/// <summary>
/// Space turf marker - turfs with this are open space.
/// </summary>
public record struct SpaceTurf;

/// <summary>
/// Simulated turf marker - turfs with atmos simulation.
/// </summary>
public record struct SimulatedTurf;

/// <summary>
/// Wall turf marker.
/// </summary>
public record struct WallTurf(
    /// <summary>Wall hardness for damage calculations.</summary>
    float Hardness = 50f,
    
    /// <summary>Whether this wall can be deconstructed.</summary>
    bool Deconstructable = true
);

/// <summary>
/// Floor turf marker.
/// </summary>
public record struct FloorTurf(
    /// <summary>Floor tile overlay ID (if tiled).</summary>
    ushort TileId = 0,
    
    /// <summary>Whether the floor uses burnt overlay.</summary>
    bool Burnt = false
);
