namespace SpaceStation.Shared.Map;

/// <summary>
/// Grid and tile size constants.
/// </summary>
public static class GridConstants
{
    /// <summary>Size of a single tile in pixels.</summary>
    public const int TileSize = 32;

    /// <summary>Default map width in tiles.</summary>
    public const int DefaultMapWidth = 255;

    /// <summary>Default map height in tiles.</summary>
    public const int DefaultMapHeight = 255;

    /// <summary>Maximum Z-levels supported.</summary>
    public const int MaxZLevels = 10;

    /// <summary>
    /// Converts tile coordinates to pixel coordinates (center of tile).
    /// </summary>
    public static (float X, float Y) TileToPixel(int tileX, int tileY)
    {
        return (tileX * TileSize + TileSize / 2f, tileY * TileSize + TileSize / 2f);
    }

    /// <summary>
    /// Converts pixel coordinates to tile coordinates.
    /// </summary>
    public static (int X, int Y) PixelToTile(float pixelX, float pixelY)
    {
        return ((int)(pixelX / TileSize), (int)(pixelY / TileSize));
    }

    /// <summary>
    /// Gets the flat array index for tile coordinates.
    /// </summary>
    public static int GetIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    /// <summary>
    /// Gets tile coordinates from flat array index.
    /// </summary>
    public static (int X, int Y) GetCoords(int index, int width)
    {
        return (index % width, index / width);
    }
}
