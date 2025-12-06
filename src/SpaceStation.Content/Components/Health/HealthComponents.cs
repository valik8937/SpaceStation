using SpaceStation.Shared.Enums;

namespace SpaceStation.Content.Components;

/// <summary>
/// Health component for entity health and state.
/// </summary>
public record struct Health(
    float MaxHealth = 100f,
    float CurrentHealth = 100f,
    MobState State = MobState.Alive
)
{
    public readonly float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
    public readonly bool IsDead => State == MobState.Dead || State == MobState.Gibbed;
    public readonly bool IsCritical => State == MobState.Critical;
}

/// <summary>
/// Damageable component tracking damage by type.
/// </summary>
public struct Damageable
{
    public float BruteDamage;
    public float BurnDamage;
    public float ToxinDamage;
    public float OxygenDamage;
    public float StaminaDamage;

    public readonly float TotalDamage =>
        BruteDamage + BurnDamage + ToxinDamage + OxygenDamage + StaminaDamage;

    public void ApplyDamage(DamageType type, float amount)
    {
        switch (type)
        {
            case DamageType.Brute: BruteDamage = MathF.Max(0, BruteDamage + amount); break;
            case DamageType.Burn: BurnDamage = MathF.Max(0, BurnDamage + amount); break;
            case DamageType.Toxin: ToxinDamage = MathF.Max(0, ToxinDamage + amount); break;
            case DamageType.Oxygen: OxygenDamage = MathF.Max(0, OxygenDamage + amount); break;
            case DamageType.Stamina: StaminaDamage = MathF.Max(0, StaminaDamage + amount); break;
        }
    }

    public void HealDamage(DamageType type, float amount)
    {
        ApplyDamage(type, -amount);
    }
}
