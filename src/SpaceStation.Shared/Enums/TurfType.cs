namespace SpaceStation.Shared.Enums;

/// <summary>
/// Types of tiles in the game world.
/// </summary>
public enum TurfType
{
    // Space
    Space,
    
    // Floors
    FloorPlating,
    FloorSteel,
    FloorWhite,
    FloorDark,
    FloorWood,
    FloorCarpet,
    FloorGrass,
    FloorSand,
    FloorLava,
    FloorWater,
    
    // Walls
    WallSteel,
    WallReinforced,
    WallPlasteel,
    WallShuttle,
    
    // Special
    Lattice,
    Catwalk
}

/// <summary>
/// Flags for turf properties.
/// </summary>
[Flags]
public enum TurfFlags
{
    None = 0,
    Airtight = 1,
    Transparent = 2,
    Dense = 4,
    Slippery = 8,
    Burnable = 16,
    Buildable = 32
}
