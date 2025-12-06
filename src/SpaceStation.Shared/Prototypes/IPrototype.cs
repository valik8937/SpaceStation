namespace SpaceStation.Shared.Prototypes;

/// <summary>
/// Interface for prototype definitions.
/// Prototypes define templates for creating entities via data (YAML/JSON).
/// </summary>
public interface IPrototype
{
    /// <summary>
    /// Unique identifier for this prototype.
    /// </summary>
    string ID { get; }
}

/// <summary>
/// Attribute to mark a class as a prototype type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PrototypeAttribute : Attribute
{
    public string TypeName { get; }

    public PrototypeAttribute(string typeName)
    {
        TypeName = typeName;
    }
}
