using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceStation.Client.Resources;

/// <summary>
/// Manages game resources including textures and DMI atlases.
/// Loads and caches textures, provides sprite lookup by state name.
/// </summary>
public sealed class ResourceManager : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, DmiAtlas> _atlases = new();
    private readonly Dictionary<string, string> _stateToAtlas = new(); // state -> atlas id lookup
    
    private bool _isDisposed;
    
    /// <summary>Number of loaded atlases.</summary>
    public int AtlasCount => _atlases.Count;
    
    /// <summary>Number of loaded textures.</summary>
    public int TextureCount => _textures.Count;
    
    /// <summary>Total number of sprite states available.</summary>
    public int TotalStates => _stateToAtlas.Count;
    
    public ResourceManager(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    }
    
    /// <summary>
    /// Loads all DMI JSON atlases from a directory recursively.
    /// </summary>
    /// <param name="rootPath">Root directory to scan.</param>
    public void LoadDirectory(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            Console.WriteLine($"[ResourceManager] Directory not found: {rootPath}");
            return;
        }
        
        var jsonFiles = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
        var loaded = 0;
        var errors = 0;
        
        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                LoadAtlas(jsonFile);
                loaded++;
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"[ResourceManager] Failed to load {jsonFile}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"[ResourceManager] Loaded {loaded} atlases, {errors} errors, {TotalStates} total states");
    }
    
    /// <summary>
    /// Loads a single DMI JSON atlas file.
    /// </summary>
    public void LoadAtlas(string jsonPath)
    {
        var jsonContent = File.ReadAllText(jsonPath);
        var atlas = JsonSerializer.Deserialize<DmiAtlas>(jsonContent);
        
        if (atlas == null || atlas.Meta.Image == null)
        {
            throw new InvalidDataException($"Invalid atlas format: {jsonPath}");
        }
        
        // Generate atlas ID from relative path
        var atlasId = Path.GetFileNameWithoutExtension(jsonPath);
        var directory = Path.GetDirectoryName(jsonPath) ?? "";
        
        // Load associated texture
        var texturePath = Path.Combine(directory, atlas.Meta.Image);
        if (!File.Exists(texturePath))
        {
            throw new FileNotFoundException($"Texture not found: {texturePath}");
        }
        
        // Load texture using MonoGame
        using var stream = File.OpenRead(texturePath);
        var texture = Texture2D.FromStream(_graphicsDevice, stream);
        
        // Store with unique ID
        var uniqueId = GetUniqueAtlasId(atlasId, jsonPath);
        _atlases[uniqueId] = atlas;
        _textures[uniqueId] = texture;
        
        // Index all states for quick lookup
        foreach (var stateName in atlas.States.Keys)
        {
            // Use first occurrence if duplicate
            if (!_stateToAtlas.ContainsKey(stateName))
            {
                _stateToAtlas[stateName] = uniqueId;
            }
        }
    }
    
    private static string GetUniqueAtlasId(string baseName, string fullPath)
    {
        // Create ID based on directory structure
        var parts = fullPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var relevantParts = parts
            .SkipWhile(p => p != "Textures")
            .Skip(1) // Skip "Textures" itself
            .Take(parts.Length)
            .Select(p => Path.GetFileNameWithoutExtension(p));
        
        return string.Join("/", relevantParts);
    }
    
    /// <summary>
    /// Gets a sprite by state name, searching all loaded atlases.
    /// </summary>
    /// <param name="stateName">The icon state name (e.g., "floor_steel").</param>
    /// <param name="direction">Direction for directional sprites.</param>
    /// <returns>SpriteData with texture and source rectangle.</returns>
    public SpriteData GetSprite(string stateName, SpriteDirection direction = SpriteDirection.South)
    {
        if (!_stateToAtlas.TryGetValue(stateName, out var atlasId))
        {
            return SpriteData.Empty;
        }
        
        return GetSprite(atlasId, stateName, direction);
    }
    
    /// <summary>
    /// Gets a sprite from a specific atlas.
    /// </summary>
    /// <param name="atlasId">Atlas identifier.</param>
    /// <param name="stateName">Icon state name.</param>
    /// <param name="direction">Direction for directional sprites.</param>
    public SpriteData GetSprite(string atlasId, string stateName, SpriteDirection direction = SpriteDirection.South)
    {
        if (!_atlases.TryGetValue(atlasId, out var atlas) ||
            !_textures.TryGetValue(atlasId, out var texture))
        {
            return SpriteData.Empty;
        }
        
        if (!atlas.States.TryGetValue(stateName, out var state))
        {
            return SpriteData.Empty;
        }
        
        // Get first frame of the requested direction
        var dirKey = direction.ToJsonKey();
        List<DmiFrame>? frames = null;
        
        if (state.Directions != null)
        {
            if (!state.Directions.TryGetValue(dirKey, out frames) || frames.Count == 0)
            {
                // Fallback to south or first available
                if (!state.Directions.TryGetValue("south", out frames) || frames.Count == 0)
                {
                    frames = state.Directions.Values.FirstOrDefault();
                }
            }
        }
        
        if (frames == null || frames.Count == 0)
        {
            // Use default size from meta
            return new SpriteData(
                texture,
                new Rectangle(0, 0, atlas.Meta.Size.Width, atlas.Meta.Size.Height),
                new Vector2(atlas.Meta.Size.Width / 2f, atlas.Meta.Size.Height / 2f)
            );
        }
        
        var frame = frames[0];
        return new SpriteData(
            texture,
            new Rectangle(frame.X, frame.Y, frame.W, frame.H),
            new Vector2(frame.W / 2f, frame.H / 2f)
        );
    }
    
    /// <summary>
    /// Gets animation data for an animated sprite.
    /// </summary>
    public AnimationData GetAnimation(string stateName, SpriteDirection direction = SpriteDirection.South)
    {
        if (!_stateToAtlas.TryGetValue(stateName, out var atlasId))
        {
            return AnimationData.Empty;
        }
        
        return GetAnimation(atlasId, stateName, direction);
    }
    
    /// <summary>
    /// Gets animation data from a specific atlas.
    /// </summary>
    public AnimationData GetAnimation(string atlasId, string stateName, SpriteDirection direction = SpriteDirection.South)
    {
        if (!_atlases.TryGetValue(atlasId, out var atlas) ||
            !_textures.TryGetValue(atlasId, out var texture))
        {
            return AnimationData.Empty;
        }
        
        if (!atlas.States.TryGetValue(stateName, out var state) || state.Directions == null)
        {
            return AnimationData.Empty;
        }
        
        var dirKey = direction.ToJsonKey();
        if (!state.Directions.TryGetValue(dirKey, out var dmiFrames) || dmiFrames.Count == 0)
        {
            if (!state.Directions.TryGetValue("south", out dmiFrames) || dmiFrames.Count == 0)
            {
                dmiFrames = state.Directions.Values.FirstOrDefault();
            }
        }
        
        if (dmiFrames == null || dmiFrames.Count == 0)
        {
            return AnimationData.Empty;
        }
        
        // Convert to AnimationFrame array
        var frames = new AnimationFrame[dmiFrames.Count];
        float currentTime = 0f;
        
        for (int i = 0; i < dmiFrames.Count; i++)
        {
            var dmi = dmiFrames[i];
            var duration = dmi.DelayMs / 1000f; // Convert ms to seconds
            
            frames[i] = new AnimationFrame(
                new Rectangle(dmi.X, dmi.Y, dmi.W, dmi.H),
                duration,
                currentTime
            );
            
            currentTime += duration;
        }
        
        return new AnimationData
        {
            Texture = texture,
            Frames = frames,
            Loop = state.Loop,
            TotalDuration = currentTime
        };
    }
    
    /// <summary>
    /// Checks if a state exists.
    /// </summary>
    public bool HasState(string stateName) => _stateToAtlas.ContainsKey(stateName);
    
    /// <summary>
    /// Gets the atlas ID for a state.
    /// </summary>
    public string? GetAtlasForState(string stateName) => 
        _stateToAtlas.TryGetValue(stateName, out var id) ? id : null;
    
    /// <summary>
    /// Lists all available states (for debugging).
    /// </summary>
    public IEnumerable<string> GetAllStates() => _stateToAtlas.Keys;
    
    /// <summary>
    /// Searches for states containing a query string.
    /// </summary>
    public IEnumerable<string> SearchStates(string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        return _stateToAtlas.Keys.Where(s => s.ToLowerInvariant().Contains(lowerQuery));
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }
        
        _textures.Clear();
        _atlases.Clear();
        _stateToAtlas.Clear();
    }
}
