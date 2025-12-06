namespace SpaceStation.Shared.Enums;

/// <summary>
/// Cardinal and diagonal directions.
/// Matches SS13's direction flags.
/// </summary>
[Flags]
public enum Direction : byte
{
    None = 0,
    North = 1,
    South = 2,
    East = 4,
    West = 8,
    NorthEast = North | East,
    NorthWest = North | West,
    SouthEast = South | East,
    SouthWest = South | West
}
