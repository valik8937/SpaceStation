namespace SpaceStation.Core.Utilities;

/// <summary>
/// Math helper utilities.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps a value between min and max.
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
    
    /// <summary>
    /// Clamps a value between min and max.
    /// </summary>
    public static int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
    
    /// <summary>
    /// Linear interpolation between two values.
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Clamp(t, 0f, 1f);
    }
    
    /// <summary>
    /// Returns the Manhattan distance between two points.
    /// </summary>
    public static float ManhattanDistance(float x1, float y1, float x2, float y2)
    {
        return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
    }
    
    /// <summary>
    /// Returns the Chebyshev distance (chess king distance) between two points.
    /// </summary>
    public static float ChebyshevDistance(float x1, float y1, float x2, float y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }
    
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static float ToRadians(float degrees) => degrees * MathF.PI / 180f;
    
    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    public static float ToDegrees(float radians) => radians * 180f / MathF.PI;
}
