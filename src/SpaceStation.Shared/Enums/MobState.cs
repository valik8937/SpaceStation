namespace SpaceStation.Shared.Enums;

/// <summary>
/// States a living mob can be in.
/// </summary>
public enum MobState
{
    /// <summary>Normal functioning state.</summary>
    Alive,
    
    /// <summary>Critically injured, near death.</summary>
    Critical,
    
    /// <summary>Dead but potentially revivable.</summary>
    Dead,
    
    /// <summary>Unconscious but stable.</summary>
    Unconscious,
    
    /// <summary>Unable to be revived.</summary>
    Gibbed
}

/// <summary>
/// Types of consciousness for mobs.
/// </summary>
public enum Consciousness
{
    /// <summary>Fully conscious and aware.</summary>
    Conscious,
    
    /// <summary>Sleeping or knocked out.</summary>
    Unconscious,
    
    /// <summary>In a coma.</summary>
    Comatose
}
