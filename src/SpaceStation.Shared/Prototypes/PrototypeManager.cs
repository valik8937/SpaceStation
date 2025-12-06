using System.Text.Json;

namespace SpaceStation.Shared.Prototypes;

/// <summary>
/// Manages loading and retrieval of prototypes.
/// </summary>
public sealed class PrototypeManager
{
    private readonly Dictionary<Type, Dictionary<string, IPrototype>> _prototypes = new();
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Registers a prototype.
    /// </summary>
    public void Register<T>(T prototype) where T : IPrototype
    {
        var type = typeof(T);
        if (!_prototypes.ContainsKey(type))
        {
            _prototypes[type] = new Dictionary<string, IPrototype>(StringComparer.OrdinalIgnoreCase);
        }
        _prototypes[type][prototype.ID] = prototype;
    }

    /// <summary>
    /// Gets a prototype by ID.
    /// </summary>
    public T Get<T>(string id) where T : IPrototype
    {
        return (T)_prototypes[typeof(T)][id];
    }

    /// <summary>
    /// Tries to get a prototype by ID.
    /// </summary>
    public bool TryGet<T>(string id, out T? prototype) where T : class, IPrototype
    {
        if (_prototypes.TryGetValue(typeof(T), out var dict) &&
            dict.TryGetValue(id, out var proto))
        {
            prototype = (T)proto;
            return true;
        }
        prototype = null;
        return false;
    }

    /// <summary>
    /// Gets all prototypes of a type.
    /// </summary>
    public IEnumerable<T> GetAll<T>() where T : IPrototype
    {
        if (_prototypes.TryGetValue(typeof(T), out var dict))
        {
            return dict.Values.Cast<T>();
        }
        return Enumerable.Empty<T>();
    }

    /// <summary>
    /// Checks if a prototype exists.
    /// </summary>
    public bool Has<T>(string id) where T : IPrototype
    {
        return _prototypes.TryGetValue(typeof(T), out var dict) && dict.ContainsKey(id);
    }
    
    /// <summary>
    /// Total prototype count.
    /// </summary>
    public int PrototypeCount => _prototypes.Values.Sum(d => d.Count);

    /// <summary>
    /// Loads prototypes from a JSON file.
    /// </summary>
    public void LoadFromJson<T>(string json) where T : IPrototype
    {
        var prototypes = JsonSerializer.Deserialize<List<T>>(json, JsonOptions);
        if (prototypes == null) return;

        foreach (var prototype in prototypes)
        {
            Register(prototype);
        }
    }
    
    /// <summary>
    /// Loads all JSON prototype files from a directory recursively.
    /// </summary>
    public void LoadDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"[PrototypeManager] Directory not found: {path}");
            return;
        }
        
        var jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        var loaded = 0;
        var errors = 0;
        
        foreach (var file in jsonFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                LoadFromJson<EntityPrototype>(json);
                loaded++;
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine($"[PrototypeManager] Error loading {file}: {ex.Message}");
            }
        }
        
        // Resolve parent inheritance
        ResolveInheritance();
        
        Console.WriteLine($"[PrototypeManager] Loaded {loaded} files, {PrototypeCount} prototypes, {errors} errors");
    }
    
    /// <summary>
    /// Resolves prototype inheritance (Parent field).
    /// </summary>
    private void ResolveInheritance()
    {
        if (!_prototypes.TryGetValue(typeof(EntityPrototype), out var entityDict))
            return;
            
        foreach (var proto in entityDict.Values.Cast<EntityPrototype>())
        {
            if (string.IsNullOrEmpty(proto.Parent))
                continue;
                
            if (!entityDict.TryGetValue(proto.Parent, out var parentProto))
            {
                Console.WriteLine($"[PrototypeManager] Warning: Prototype '{proto.ID}' has unknown parent '{proto.Parent}'");
                continue;
            }
            
            var parent = (EntityPrototype)parentProto;
            
            // Inherit name if not set
            if (string.IsNullOrEmpty(proto.Name))
                proto.Name = parent.Name;
                
            // Inherit description if not set
            if (string.IsNullOrEmpty(proto.Description))
                proto.Description = parent.Description;
                
            // Inherit sprite if not set
            if (string.IsNullOrEmpty(proto.Sprite))
                proto.Sprite = parent.Sprite;
                
            // Merge components (child overrides parent)
            var parentComponents = new Dictionary<string, ComponentData>(StringComparer.OrdinalIgnoreCase);
            foreach (var comp in parent.Components)
            {
                parentComponents[comp.Type] = comp;
            }
            
            foreach (var comp in proto.Components)
            {
                parentComponents[comp.Type] = comp;
            }
            
            proto.Components = parentComponents.Values.ToList();
        }
    }
}

