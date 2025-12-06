namespace SpaceStation.Shared.Network;

/// <summary>
/// Types of network packets.
/// </summary>
public enum PacketType : byte
{
    // Connection (0-19)
    ConnectionRequest = 0,
    ConnectionAccepted = 1,
    ConnectionDenied = 2,
    Disconnect = 3,
    
    // Game State (20-39)
    WorldSnapshot = 20,
    EntitySpawn = 21,
    EntityDespawn = 22,
    EntityUpdate = 23,
    
    // Player Input (40-59)
    PlayerInput = 40,
    PlayerCommand = 41,
    PlayerMove = 42,
    PlayerInteract = 43,
    PlayerSpawned = 44,
    
    // Chat (60-79)
    ChatMessage = 60,
    
    // Admin (80-99)
    AdminCommand = 80
}
