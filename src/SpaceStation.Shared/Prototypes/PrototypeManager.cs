using System.Text.Json;

namespace SpaceStation.Shared.Prototypes;

/// <summary>
/// Manages loading and retrieval of prototypes.
/// </summary>
public sealed class PrototypeManager
{
    private readonly Dictionary<Type, Dictionary<string, IPrototype>> _prototypes = new();

    /// <summary>
    /// Registers a prototype.
    /// </summary>
    public void Register<T>(T prototype) where T : IPrototype
    {
        var type = typeof(T);
        if (!_prototypes.ContainsKey(type))
        {
            _prototypes[type] = new Dictionary<string, IPrototype>();
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
    /// Loads prototypes from a JSON file.
    /// </summary>
    public void LoadFromJson<T>(string json) where T : IPrototype
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var prototypes = JsonSerializer.Deserialize<List<T>>(json, options);
        if (prototypes == null) return;

        foreach (var prototype in prototypes)
        {
            Register(prototype);
        }
    }
}
