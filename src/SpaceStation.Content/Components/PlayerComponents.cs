namespace SpaceStation.Content.Components;

/// <summary>
/// Marks an entity as controlled by a player.
/// </summary>
public record struct Player(
    /// <summary>Client ID owning this entity.</summary>
    int ClientId = 0,

    /// <summary>Player's display name.</summary>
    string Name = "Unknown"
);

/// <summary>
/// Input state for a controllable entity.
/// Updated each tick from player input.
/// </summary>
public record struct InputState(
    float MoveX = 0f,
    float MoveY = 0f,
    bool Running = false,
    bool Interacting = false
);
