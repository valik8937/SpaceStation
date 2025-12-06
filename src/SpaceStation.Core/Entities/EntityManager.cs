using Arch.Core;

namespace SpaceStation.Core.Entities;

/// <summary>
/// Wrapper around Arch.Core.World for entity management.
/// Provides a simplified API for common operations.
/// </summary>
public sealed class EntityManager : IDisposable
{
    /// <summary>
    /// The underlying Arch ECS World.
    /// </summary>
    public World World { get; }
    
    /// <summary>
    /// Event raised when an entity is created.
    /// </summary>
    public event Action<Entity>? EntityCreated;
    
    /// <summary>
    /// Event raised when an entity is destroyed.
    /// </summary>
    public event Action<Entity>? EntityDestroyed;
    
    public EntityManager()
    {
        World = World.Create();
    }
    
    /// <summary>
    /// Creates an entity with specified components.
    /// </summary>
    public Entity Create<T1>(T1 c1)
    {
        var entity = World.Create(c1);
        EntityCreated?.Invoke(entity);
        return entity;
    }
    
    /// <summary>
    /// Creates an entity with specified components.
    /// </summary>
    public Entity Create<T1, T2>(T1 c1, T2 c2)
    {
        var entity = World.Create(c1, c2);
        EntityCreated?.Invoke(entity);
        return entity;
    }
    
    /// <summary>
    /// Creates an entity with specified components.
    /// </summary>
    public Entity Create<T1, T2, T3>(T1 c1, T2 c2, T3 c3)
    {
        var entity = World.Create(c1, c2, c3);
        EntityCreated?.Invoke(entity);
        return entity;
    }
    
    /// <summary>
    /// Creates an entity with specified components.
    /// </summary>
    public Entity Create<T1, T2, T3, T4>(T1 c1, T2 c2, T3 c3, T4 c4)
    {
        var entity = World.Create(c1, c2, c3, c4);
        EntityCreated?.Invoke(entity);
        return entity;
    }
    
    /// <summary>
    /// Creates an entity with specified components.
    /// </summary>
    public Entity Create<T1, T2, T3, T4, T5>(T1 c1, T2 c2, T3 c3, T4 c4, T5 c5)
    {
        var entity = World.Create(c1, c2, c3, c4, c5);
        EntityCreated?.Invoke(entity);
        return entity;
    }
    
    /// <summary>
    /// Destroys an entity.
    /// </summary>
    public void Destroy(Entity entity)
    {
        if (World.IsAlive(entity))
        {
            EntityDestroyed?.Invoke(entity);
            World.Destroy(entity);
        }
    }
    
    /// <summary>
    /// Checks if an entity is alive.
    /// </summary>
    public bool IsAlive(Entity entity) => World.IsAlive(entity);
    
    /// <summary>
    /// Gets a component from an entity.
    /// </summary>
    public ref T Get<T>(Entity entity) => ref World.Get<T>(entity);
    
    /// <summary>
    /// Tries to get a component from an entity.
    /// </summary>
    public bool TryGet<T>(Entity entity, out T component)
    {
        if (World.Has<T>(entity))
        {
            component = World.Get<T>(entity);
            return true;
        }
        component = default!;
        return false;
    }
    
    /// <summary>
    /// Checks if entity has a component.
    /// </summary>
    public bool Has<T>(Entity entity) => World.Has<T>(entity);
    
    /// <summary>
    /// Adds a component to an entity.
    /// </summary>
    public void Add<T>(Entity entity, T component) => World.Add(entity, component);
    
    /// <summary>
    /// Removes a component from an entity.
    /// </summary>
    public void Remove<T>(Entity entity) => World.Remove<T>(entity);
    
    /// <summary>
    /// Gets the count of entities in the world.
    /// </summary>
    public int EntityCount => World.Size;
    
    public void Dispose()
    {
        World.Dispose();
    }
}
