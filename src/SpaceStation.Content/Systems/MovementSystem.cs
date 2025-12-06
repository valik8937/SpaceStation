using Arch.Core;
using Arch.Core.Extensions;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Components;

namespace SpaceStation.Content.Systems;

/// <summary>
/// System handling entity movement and physics.
/// </summary>
public sealed class MovementSystem : BaseSystem
{
    public override int Priority => 10;

    private static readonly QueryDescription Query = new QueryDescription()
        .WithAll<Transform, Physics>();

    public override void Update(float deltaTime, World world)
    {
        world.Query(in Query, (ref Transform transform, ref Physics physics) =>
        {
            if (physics.Anchored)
                return;

            // Apply velocity
            if (physics.Velocity != System.Numerics.Vector2.Zero)
            {
                transform.Position += physics.Velocity * deltaTime;

                // Apply friction
                physics.Velocity *= (1f - physics.Friction * deltaTime);

                // Stop if very slow
                if (physics.Velocity.LengthSquared() < 0.001f)
                {
                    physics.Velocity = System.Numerics.Vector2.Zero;
                }
            }
        });
    }
}
