namespace SpaceStation.Shared.Prototypes;

/// <summary>
/// Prototype for entity definitions.
/// These are loaded from YAML/JSON files.
/// </summary>
[Prototype("entity")]
public sealed class EntityPrototype : IPrototype
{
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the entity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description shown when examining.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Parent prototype to inherit from.
    /// </summary>
    public string? Parent { get; set; }

    /// <summary>
    /// Path to the sprite/icon.
    /// </summary>
    public string? Sprite { get; set; }

    /// <summary>
    /// Components to add to this entity.
    /// </summary>
    public List<ComponentData> Components { get; set; } = new();
}

/// <summary>
/// Component data from prototype definition.
/// </summary>
public sealed class ComponentData
{
    /// <summary>
    /// Type name of the component.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Component-specific data.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}
