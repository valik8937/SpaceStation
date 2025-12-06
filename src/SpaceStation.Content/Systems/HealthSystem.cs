using Arch.Core;
using Arch.Core.Extensions;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Components;
using SpaceStation.Shared.Enums;
using SpaceStation.Shared.Constants;

namespace SpaceStation.Content.Systems;

/// <summary>
/// System managing entity health and state transitions.
/// </summary>
public sealed class HealthSystem : BaseSystem
{
    public override int Priority => 20;

    private static readonly QueryDescription Query = new QueryDescription()
        .WithAll<Health, Damageable>();

    public override void Update(float deltaTime, World world)
    {
        world.Query(in Query, (ref Health health, ref Damageable damageable) =>
        {
            // Calculate health from damage
            health.CurrentHealth = health.MaxHealth - damageable.TotalDamage;

            // Update mob state based on health
            UpdateMobState(ref health);
        });
    }

    private static void UpdateMobState(ref Health health)
    {
        if (health.State == MobState.Gibbed)
            return; // Gibbed is permanent

        if (health.CurrentHealth <= GameConstants.DeathThreshold)
        {
            health.State = MobState.Dead;
        }
        else if (health.CurrentHealth <= GameConstants.CriticalHealthThreshold)
        {
            health.State = MobState.Critical;
        }
        else
        {
            health.State = MobState.Alive;
        }
    }
}
