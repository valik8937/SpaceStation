namespace SpaceStation.Shared.Enums;

/// <summary>
/// Types of damage that can be inflicted.
/// </summary>
public enum DamageType
{
    Brute,
    Burn,
    Toxin,
    Oxygen,
    Clone,
    Stamina,
    Brain,
    Cellular
}

/// <summary>
/// Damage groups for resistance/weakness calculations.
/// </summary>
public enum DamageGroup
{
    Physical,  // Brute + Burn
    Chemical,  // Toxin + Clone
    Biological // Oxygen + Clone + Cellular
}
