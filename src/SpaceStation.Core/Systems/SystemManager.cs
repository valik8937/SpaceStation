using Arch.Core;

namespace SpaceStation.Core.Systems;

/// <summary>
/// Manages registration and execution of all game systems.
/// Similar to SS13's subsystem controller (SSsomething pattern).
/// </summary>
public sealed class SystemManager : IDisposable
{
    private readonly List<ISystem> _systems = new();
    private readonly World _world;
    private bool _initialized;

    public SystemManager(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Registers a system for execution.
    /// </summary>
    public T RegisterSystem<T>() where T : ISystem, new()
    {
        var system = new T();
        _systems.Add(system);

        if (_initialized)
        {
            system.Initialize(_world);
        }

        // Keep systems sorted by priority
        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        return system;
    }

    /// <summary>
    /// Registers an existing system instance.
    /// </summary>
    public void RegisterSystem(ISystem system)
    {
        _systems.Add(system);

        if (_initialized)
        {
            system.Initialize(_world);
        }

        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    /// <summary>
    /// Gets a registered system of the specified type.
    /// </summary>
    public T? GetSystem<T>() where T : class, ISystem
    {
        return _systems.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Initializes all registered systems.
    /// </summary>
    public void Initialize()
    {
        foreach (var system in _systems)
        {
            system.Initialize(_world);
        }
        _initialized = true;
    }

    /// <summary>
    /// Updates all enabled systems.
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
        {
            if (system.Enabled)
            {
                system.Update(deltaTime, _world);
            }
        }
    }

    /// <summary>
    /// Shuts down all systems.
    /// </summary>
    public void Shutdown()
    {
        foreach (var system in _systems)
        {
            system.Shutdown();
        }
    }

    public void Dispose()
    {
        Shutdown();
        _systems.Clear();
    }
}
