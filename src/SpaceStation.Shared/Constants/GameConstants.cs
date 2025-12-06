namespace SpaceStation.Shared.Constants;

/// <summary>
/// Core game constants.
/// </summary>
public static class GameConstants
{
    // World settings
    public const int TileSize = 32;
    public const int DefaultMapWidth = 255;
    public const int DefaultMapHeight = 255;
    public const float TickRate = 20f; // ticks per second
    public const float TickDuration = 1f / TickRate;
    
    // Movement
    public const float DefaultMoveSpeed = 1.0f;
    public const float RunMultiplier = 1.5f;
    public const float CrawlMultiplier = 0.3f;
    
    // Combat
    public const float DefaultMeleeDamage = 5f;
    public const float DefaultMeleeRange = 1.5f;
    public const float DefaultProjectileSpeed = 25f;
    
    // Interaction
    public const float DefaultInteractRange = 1.5f;
    public const float DefaultViewRange = 7f;
    
    // Health
    public const float DefaultMaxHealth = 100f;
    public const float CriticalHealthThreshold = 0f;
    public const float DeathThreshold = -100f;
}
