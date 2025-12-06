using System.Numerics;
using System.Reflection;
using System.Text.Json;
using Arch.Core;
using SpaceStation.Content.Components;
using SpaceStation.Shared.Prototypes;

namespace SpaceStation.Server;

/// <summary>
/// Factory for creating ECS entities from JSON prototypes.
/// Uses reflection to map component names to C# structs.
/// </summary>
public sealed class EntityFactory
{
    private readonly PrototypeManager _prototypes;
    private readonly Dictionary<string, Type> _componentTypes = new(StringComparer.OrdinalIgnoreCase);

    // Callback for registering spawned entities with network sync
    public Action<Entity, string>? OnEntitySpawned { get; set; }

    public EntityFactory(PrototypeManager prototypes)
    {
        _prototypes = prototypes;
        DiscoverComponents();
    }

    /// <summary>
    /// Discovers all component types from SpaceStation.Content.Components namespace.
    /// </summary>
    private void DiscoverComponents()
    {
        var contentAssembly = typeof(Transform).Assembly;
        var componentTypes = contentAssembly.GetTypes()
            .Where(t => t.IsValueType && t.Namespace?.Contains("Components") == true);

        foreach (var type in componentTypes)
        {
            _componentTypes[type.Name] = type;
        }

        Console.WriteLine($"[EntityFactory] Discovered {_componentTypes.Count} component types");
    }

    /// <summary>
    /// Spawns an entity from a prototype at the given position.
    /// </summary>
    /// <param name="world">The ECS world.</param>
    /// <param name="prototypeId">Prototype ID to spawn.</param>
    /// <param name="position">World position.</param>
    /// <returns>The created entity, or null if prototype not found.</returns>
    public Entity? Spawn(World world, string prototypeId, Vector2 position)
    {
        if (!_prototypes.TryGet<EntityPrototype>(prototypeId, out var prototype) || prototype == null)
        {
            Console.WriteLine($"[EntityFactory] Prototype not found: {prototypeId}");
            return null;
        }

        return SpawnFromPrototype(world, prototype, position);
    }

    /// <summary>
    /// Spawns an entity from a prototype object.
    /// </summary>
    public Entity SpawnFromPrototype(World world, EntityPrototype prototype, Vector2 position)
    {
        // Always create with Transform
        var transform = new Transform(position, 0f, 0);

        // Collect all components to add
        var components = new List<object> { transform };

        foreach (var compData in prototype.Components)
        {
            var component = CreateComponent(compData);
            if (component != null)
            {
                components.Add(component);
            }
        }

        // Create entity with all components
        var entity = CreateEntityWithComponents(world, components);

        // Notify listeners (for network registration)
        OnEntitySpawned?.Invoke(entity, prototype.Sprite ?? prototype.ID);

        return entity;
    }

    /// <summary>
    /// Creates a component instance from ComponentData.
    /// </summary>
    private object? CreateComponent(ComponentData data)
    {
        if (!_componentTypes.TryGetValue(data.Type, out var componentType))
        {
            Console.WriteLine($"[EntityFactory] Unknown component type: {data.Type}");
            return null;
        }

        try
        {
            // Create instance using constructor with matching parameters
            var instance = CreateComponentInstance(componentType, data.Data);
            return instance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EntityFactory] Failed to create component {data.Type}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a component instance, matching JSON data to constructor parameters.
    /// </summary>
    private object CreateComponentInstance(Type type, Dictionary<string, object> data)
    {
        // Get the primary constructor (record structs have one)
        var constructors = type.GetConstructors();

        // Try to find a constructor that matches the data
        foreach (var ctor in constructors.OrderByDescending(c => c.GetParameters().Length))
        {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            var matched = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var value = GetParameterValue(param, data);

                if (value == null && !param.HasDefaultValue)
                {
                    matched = false;
                    break;
                }

                args[i] = value ?? param.DefaultValue;
            }

            if (matched)
            {
                return ctor.Invoke(args)!;
            }
        }

        // Fallback: create default instance and set properties
        var instance = Activator.CreateInstance(type)!;
        SetProperties(instance, data);
        return instance;
    }

    /// <summary>
    /// Gets a parameter value from JSON data.
    /// </summary>
    private object? GetParameterValue(ParameterInfo param, Dictionary<string, object> data)
    {
        // Try exact match first
        if (data.TryGetValue(param.Name!, out var value))
        {
            return ConvertValue(value, param.ParameterType);
        }

        // Try case-insensitive match
        var key = data.Keys.FirstOrDefault(k =>
            string.Equals(k, param.Name, StringComparison.OrdinalIgnoreCase));

        if (key != null)
        {
            return ConvertValue(data[key], param.ParameterType);
        }

        return null;
    }

    /// <summary>
    /// Sets properties on an object from JSON data.
    /// </summary>
    private void SetProperties(object instance, Dictionary<string, object> data)
    {
        var type = instance.GetType();

        foreach (var (key, value) in data)
        {
            var prop = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite)
            {
                var converted = ConvertValue(value, prop.PropertyType);
                prop.SetValue(instance, converted);
            }
        }
    }

    /// <summary>
    /// Converts a JSON value to the target type.
    /// </summary>
    private object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;

        // Handle JsonElement (from System.Text.Json)
        if (value is JsonElement element)
        {
            return ConvertJsonElement(element, targetType);
        }

        // Direct type match
        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        // Numeric conversions
        if (targetType == typeof(float))
            return Convert.ToSingle(value);
        if (targetType == typeof(int))
            return Convert.ToInt32(value);
        if (targetType == typeof(double))
            return Convert.ToDouble(value);
        if (targetType == typeof(bool))
            return Convert.ToBoolean(value);
        if (targetType == typeof(byte))
            return Convert.ToByte(value);
        if (targetType == typeof(ushort))
            return Convert.ToUInt16(value);

        // Vector2
        if (targetType == typeof(Vector2) && value is JsonElement vecElement)
        {
            var x = vecElement.GetProperty("X").GetSingle();
            var y = vecElement.GetProperty("Y").GetSingle();
            return new Vector2(x, y);
        }

        // Enum
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
        }

        return Convert.ChangeType(value, targetType);
    }

    /// <summary>
    /// Converts a JsonElement to the target type.
    /// </summary>
    private object? ConvertJsonElement(JsonElement element, Type targetType)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number when targetType == typeof(float) => element.GetSingle(),
            JsonValueKind.Number when targetType == typeof(int) => element.GetInt32(),
            JsonValueKind.Number when targetType == typeof(double) => element.GetDouble(),
            JsonValueKind.Number when targetType == typeof(byte) => (byte)element.GetInt32(),
            JsonValueKind.Number when targetType == typeof(ushort) => (ushort)element.GetInt32(),
            JsonValueKind.Number => element.GetSingle(), // Default to float
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when targetType.IsEnum => Enum.Parse(targetType, element.GetString()!, true),
            JsonValueKind.String => element.GetString(),
            _ => null
        };
    }

    /// <summary>
    /// Creates an entity with a dynamic list of components.
    /// Uses typed dispatch for common cases.
    /// </summary>
    private Entity CreateEntityWithComponents(World world, List<object> components)
    {
        // First component is always Transform
        var transform = (Transform)components[0];

        if (components.Count == 1)
        {
            return world.Create(transform);
        }

        // Create with transform first, then add other components
        var entity = world.Create(transform);

        for (int i = 1; i < components.Count; i++)
        {
            AddComponentByType(world, entity, components[i]);
        }

        return entity;
    }

    /// <summary>
    /// Adds a component to an entity using type-specific dispatch.
    /// </summary>
    private void AddComponentByType(World world, Entity entity, object component)
    {
        switch (component)
        {
            case Physics p:
                world.Add(entity, p);
                break;
            case Health h:
                world.Add(entity, h);
                break;
            case Damageable d:
                world.Add(entity, d);
                break;
            case Atmosphere a:
                world.Add(entity, a);
                break;
            case PowerProducer pp:
                world.Add(entity, pp);
                break;
            case PowerConsumer pc:
                world.Add(entity, pc);
                break;
            case Collision c:
                world.Add(entity, c);
                break;
            case RenderLayer rl:
                world.Add(entity, rl);
                break;
            case Visibility v:
                world.Add(entity, v);
                break;
            case Turf t:
                world.Add(entity, t);
                break;
            case Player pl:
                world.Add(entity, pl);
                break;
            case InputState inp:
                world.Add(entity, inp);
                break;
            default:
                // Fallback to reflection for unknown types
                AddComponentDynamic(world, entity, component);
                break;
        }
    }

    private void AddComponentDynamic(World world, Entity entity, object component)
    {
        // Use reflection to call Add<T>
        var method = typeof(Arch.Core.Extensions.EntityExtensions)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 2);

        if (method != null)
        {
            var generic = method.MakeGenericMethod(component.GetType());
            generic.Invoke(null, new object[] { entity, component });
        }
    }
}
