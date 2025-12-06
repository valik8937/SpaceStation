using MemoryPack;

namespace SpaceStation.Shared.Network.Packets;

/// <summary>
/// Packet header containing type and metadata.
/// </summary>
[MemoryPackable]
public partial struct PacketHeader
{
    public PacketType Type;
    public uint Tick;
    public long Timestamp;
}

/// <summary>
/// World snapshot containing all entity states.
/// Sent from server to clients every tick.
/// </summary>
[MemoryPackable]
public partial class WorldSnapshotPacket
{
    public PacketHeader Header { get; set; }
    public NetworkEntity[] Entities { get; set; } = Array.Empty<NetworkEntity>();
    public int PlayerCount { get; set; }
    
    public WorldSnapshotPacket()
    {
        Header = new PacketHeader
        {
            Type = PacketType.WorldSnapshot,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}

/// <summary>
/// Single entity spawn packet.
/// </summary>
[MemoryPackable]
public partial class EntitySpawnPacket
{
    public PacketHeader Header { get; set; }
    public NetworkEntity Entity { get; set; } = null!;
    
    public EntitySpawnPacket()
    {
        Header = new PacketHeader { Type = PacketType.EntitySpawn };
    }
}

/// <summary>
/// Entity despawn packet.
/// </summary>
[MemoryPackable]
public partial class EntityDespawnPacket
{
    public PacketHeader Header { get; set; }
    public int EntityId { get; set; }
    
    public EntityDespawnPacket()
    {
        Header = new PacketHeader { Type = PacketType.EntityDespawn };
    }
}

/// <summary>
/// Connection accepted response.
/// </summary>
[MemoryPackable]
public partial class ConnectionAcceptedPacket
{
    public PacketHeader Header { get; set; }
    public int ClientId { get; set; }
    public string ServerName { get; set; } = "Space Station 13";
    public int TickRate { get; set; } = NetworkConstants.TickRate;
    
    public ConnectionAcceptedPacket()
    {
        Header = new PacketHeader { Type = PacketType.ConnectionAccepted };
    }
}

/// <summary>
/// Connection denied response.
/// </summary>
[MemoryPackable]
public partial class ConnectionDeniedPacket
{
    public PacketHeader Header { get; set; }
    public string Reason { get; set; } = "Connection denied";
    
    public ConnectionDeniedPacket()
    {
        Header = new PacketHeader { Type = PacketType.ConnectionDenied };
    }
}
