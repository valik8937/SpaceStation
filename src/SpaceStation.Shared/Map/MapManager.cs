using System.Text.Json;
using System.Numerics; // Keep this for Vector2 in GridConstants, though not directly used in MapManager anymore.
// using Arch.Core; // Removed as per instruction
// using SpaceStation.Content.Components; // Removed as per instruction

namespace SpaceStation.Shared.Map;

/// <summary>
/// Manages the game map grid data.
/// Uses a flat 1D array for cache-friendly access.
/// Entity spawning should be done by server/client using this data.
/// </summary>
public class MapManager
{
    private TileData[] _tiles = Array.Empty<TileData>();
    private int _width;
    private int _height;
    private int _zLevel;

    /// <summary>Map width in tiles.</summary>
    public int Width => _width;

    /// <summary>Map height in tiles.</summary>
    public int Height => _height;

    /// <summary>Z-level of this map.</summary>
    public int ZLevel => _zLevel;

    /// <summary>Total tile count.</summary>
    public int TileCount => _tiles.Length;

    /// <summary>
    /// Gets a reference to tile data at the given coordinates.
    /// </summary>
    public ref TileData GetTile(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            throw new ArgumentOutOfRangeException($"Tile coordinates ({x}, {y}) out of bounds ({_width}x{_height})");
        }

        return ref _tiles[GridConstants.GetIndex(x, y, _width)];
    }

    /// <summary>
    /// Tries to get tile data, returns false if out of bounds.
    /// </summary>
    public bool TryGetTile(int x, int y, out TileData tile)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            tile = default;
            return false;
        }

        tile = _tiles[GridConstants.GetIndex(x, y, _width)];
        return true;
    }

    /// <summary>
    /// Sets tile data at the given coordinates.
    /// </summary>
    public void SetTile(int x, int y, TileData data)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return;

        _tiles[GridConstants.GetIndex(x, y, _width)] = data;
    }

    /// <summary>
    /// Initializes an empty map of the given size.
    /// </summary>
    public void Initialize(int width, int height, int zLevel = 0, ushort defaultPrototype = 0)
    {
        _width = width;
        _height = height;
        _zLevel = zLevel;
        _tiles = new TileData[width * height];

        // Initialize all tiles
        var defaultTile = new TileData(defaultPrototype);
        Array.Fill(_tiles, defaultTile);

        Console.WriteLine($"[MapManager] Initialized {width}x{height} map on Z-level {zLevel}");
    }

    /// <summary>
    /// Loads a map from a simple text format.
    /// Each character represents a tile type.
    /// </summary>
    public void LoadFromText(string mapData, Dictionary<char, ushort>? legend = null)
    {
        legend ??= new Dictionary<char, ushort>
        {
            { ' ', 0 },  // Space
            { '.', 1 },  // Floor
            { '#', 2 },  // Wall
            { 'D', 3 },  // Door
        };

        var lines = mapData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var height = lines.Length;
        var width = lines.Max(l => l.Length);

        Initialize(width, height);

        for (int y = 0; y < lines.Length; y++)
        {
            var line = lines[y];
            for (int x = 0; x < line.Length; x++)
            {
                var c = line[x];
                if (legend.TryGetValue(c, out var prototypeId))
                {
                    var tile = new TileData(prototypeId);

                    // Set common flags based on type
                    if (prototypeId == 0) // Space
                    {
                        tile.Flags |= TileFlags.Space;
                    }
                    else if (prototypeId == 1) // Floor
                    {
                        tile.Flags |= TileFlags.Intact | TileFlags.Simulated;
                    }
                    else if (prototypeId == 2) // Wall
                    {
                        tile.Flags |= TileFlags.Dense | TileFlags.Opaque | TileFlags.BlocksAir;
                    }

                    SetTile(x, y, tile);
                }
            }
        }

        Console.WriteLine($"[MapManager] Loaded text map {width}x{height}");
    }

    /// <summary>
    /// Loads a map from JSON format.
    /// </summary>
    public void LoadFromJson(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var mapJson = JsonSerializer.Deserialize<MapJson>(json);

        if (mapJson == null)
        {
            throw new InvalidDataException($"Invalid map format: {jsonPath}");
        }

        Initialize(mapJson.Width, mapJson.Height, mapJson.ZLevel);

        if (mapJson.Tiles != null)
        {
            // Direct tile array
            for (int i = 0; i < Math.Min(mapJson.Tiles.Length, _tiles.Length); i++)
            {
                _tiles[i] = new TileData((ushort)mapJson.Tiles[i]);
            }
        }
        else if (mapJson.Data != null)
        {
            // Text-based data
            LoadFromText(mapJson.Data, mapJson.Legend);
        }

        Console.WriteLine($"[MapManager] Loaded JSON map from {jsonPath}");
    }


    /// <summary>
    /// Gets the raw tile array (for serialization).
    /// </summary>
    public TileData[] GetTileArray() => _tiles;

    /// <summary>
    /// Checks if a tile is passable for movement.
    /// </summary>
    public bool IsPassable(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return false;

        var tile = _tiles[GridConstants.GetIndex(x, y, _width)];
        return !tile.Dense;
    }

    /// <summary>
    /// Checks if a tile blocks vision.
    /// </summary>
    public bool BlocksVision(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return true;

        var tile = _tiles[GridConstants.GetIndex(x, y, _width)];
        return tile.Opaque;
    }
}

/// <summary>
/// JSON format for map files.
/// </summary>
internal class MapJson
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int ZLevel { get; set; }
    public int[]? Tiles { get; set; }
    public string? Data { get; set; }
    public Dictionary<char, ushort>? Legend { get; set; }
}
