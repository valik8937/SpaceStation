using Arch.Core;

namespace SpaceStation.Core.Systems;

/// <summary>
/// Base interface for all systems in the Entity Component System.
/// Systems contain logic and operate on entities with specific components.
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Priority for system execution order. Lower values run first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether this system is currently enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Called when the system is initialized.
    /// </summary>
    void Initialize(World world);

    /// <summary>
    /// Called every game tick.
    /// </summary>
    /// <param name="deltaTime">Time since last tick in seconds.</param>
    /// <param name="world">The ECS world to query.</param>
    void Update(float deltaTime, World world);

    /// <summary>
    /// Called when the system is shut down.
    /// </summary>
    void Shutdown();
}
