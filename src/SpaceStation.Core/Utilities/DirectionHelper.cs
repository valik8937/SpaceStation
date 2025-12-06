using System.Numerics;

namespace SpaceStation.Core.Utilities;

/// <summary>
/// Helper methods for direction calculations.
/// </summary>
public static class DirectionHelper
{
    /// <summary>
    /// All cardinal and diagonal directions.
    /// </summary>
    public static readonly Vector2[] AllDirections =
    {
        new(0, 1),   // North
        new(1, 1),   // NorthEast
        new(1, 0),   // East
        new(1, -1),  // SouthEast
        new(0, -1),  // South
        new(-1, -1), // SouthWest
        new(-1, 0),  // West
        new(-1, 1)   // NorthWest
    };
    
    /// <summary>
    /// Cardinal directions only.
    /// </summary>
    public static readonly Vector2[] CardinalDirections =
    {
        new(0, 1),  // North
        new(1, 0),  // East
        new(0, -1), // South
        new(-1, 0)  // West
    };
    
    /// <summary>
    /// Gets the opposite direction.
    /// </summary>
    public static Vector2 GetOpposite(Vector2 direction)
    {
        return -direction;
    }
    
    /// <summary>
    /// Rotates a direction 90 degrees clockwise.
    /// </summary>
    public static Vector2 RotateClockwise(Vector2 direction)
    {
        return new Vector2(direction.Y, -direction.X);
    }
    
    /// <summary>
    /// Rotates a direction 90 degrees counter-clockwise.
    /// </summary>
    public static Vector2 RotateCounterClockwise(Vector2 direction)
    {
        return new Vector2(-direction.Y, direction.X);
    }
    
    /// <summary>
    /// Gets the direction from one point to another.
    /// </summary>
    public static Vector2 GetDirection(Vector2 from, Vector2 to)
    {
        var diff = to - from;
        if (diff == Vector2.Zero)
            return Vector2.Zero;
            
        return Vector2.Normalize(diff);
    }
    
    /// <summary>
    /// Snaps a direction to the nearest cardinal or diagonal direction.
    /// </summary>
    public static Vector2 SnapToDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero)
            return Vector2.Zero;
            
        var normalized = Vector2.Normalize(direction);
        Vector2 best = AllDirections[0];
        float bestDot = Vector2.Dot(normalized, best);
        
        for (int i = 1; i < AllDirections.Length; i++)
        {
            var dot = Vector2.Dot(normalized, AllDirections[i]);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = AllDirections[i];
            }
        }
        
        return best;
    }
}
