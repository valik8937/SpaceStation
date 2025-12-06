namespace SpaceStation.Core.Utilities;

/// <summary>
/// Thread-safe random number generator with game-specific utilities.
/// </summary>
public static class RandomHelper
{
    private static readonly ThreadLocal<Random> _random = new(() => new Random());
    
    /// <summary>
    /// Gets a random integer between min (inclusive) and max (exclusive).
    /// </summary>
    public static int Next(int min, int max) => _random.Value!.Next(min, max);
    
    /// <summary>
    /// Gets a random integer between 0 and max (exclusive).
    /// </summary>
    public static int Next(int max) => _random.Value!.Next(max);
    
    /// <summary>
    /// Gets a random float between 0 and 1.
    /// </summary>
    public static float NextFloat() => (float)_random.Value!.NextDouble();
    
    /// <summary>
    /// Gets a random float between min and max.
    /// </summary>
    public static float NextFloat(float min, float max) => min + (max - min) * NextFloat();
    
    /// <summary>
    /// Returns true with the given probability (0-1).
    /// </summary>
    public static bool Prob(float probability) => NextFloat() < probability;
    
    /// <summary>
    /// Returns true with the given percentage (0-100).
    /// Similar to DM's prob() function.
    /// </summary>
    public static bool Prob(int percentage) => Next(100) < percentage;
    
    /// <summary>
    /// Picks a random element from a list.
    /// Similar to DM's pick().
    /// </summary>
    public static T Pick<T>(IList<T> list) => list[Next(list.Count)];
    
    /// <summary>
    /// Picks a random element from params.
    /// Similar to DM's pick().
    /// </summary>
    public static T Pick<T>(params T[] items) => items[Next(items.Length)];
    
    /// <summary>
    /// Shuffles a list in place.
    /// </summary>
    public static void Shuffle<T>(IList<T> list)
    {
        var random = _random.Value!;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
