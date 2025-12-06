using Arch.Core;

namespace SpaceStation.Core.Systems;

/// <summary>
/// Base class for systems with common functionality.
/// </summary>
public abstract class BaseSystem : ISystem
{
    public virtual int Priority => 0;
    public bool Enabled { get; set; } = true;

    protected World World { get; private set; } = null!;

    public virtual void Initialize(World world)
    {
        World = world;
    }

    public abstract void Update(float deltaTime, World world);

    public virtual void Shutdown() { }
}
