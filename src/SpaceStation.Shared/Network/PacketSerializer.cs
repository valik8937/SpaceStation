using LiteNetLib;
using MemoryPack;
using SpaceStation.Shared.Network.Packets;

namespace SpaceStation.Shared.Network;

/// <summary>
/// Helper class for serializing and deserializing network packets.
/// </summary>
public static class PacketSerializer
{
    /// <summary>
    /// Serializes a packet to bytes.
    /// </summary>
    public static byte[] Serialize<T>(T packet) where T : class
    {
        return MemoryPackSerializer.Serialize(packet);
    }
    
    /// <summary>
    /// Generic deserialize method for any MemoryPackable packet.
    /// </summary>
    public static T? Deserialize<T>(byte[] data) where T : class
    {
        try
        {
            return MemoryPackSerializer.Deserialize<T>(data);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Deserializes a world snapshot packet from bytes.
    /// </summary>
    public static WorldSnapshotPacket? DeserializeSnapshot(byte[] data)
    {
        try
        {
            return MemoryPackSerializer.Deserialize<WorldSnapshotPacket>(data);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Deserializes a connection accepted packet from bytes.
    /// </summary>
    public static ConnectionAcceptedPacket? DeserializeConnectionAccepted(byte[] data)
    {
        try
        {
            return MemoryPackSerializer.Deserialize<ConnectionAcceptedPacket>(data);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Deserializes a connection denied packet from bytes.
    /// </summary>
    public static ConnectionDeniedPacket? DeserializeConnectionDenied(byte[] data)
    {
        try
        {
            return MemoryPackSerializer.Deserialize<ConnectionDeniedPacket>(data);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Gets the packet type from raw data (first byte after header).
    /// </summary>
    public static PacketType GetPacketType(byte[] data)
    {
        if (data.Length < 1)
            return (PacketType)255; // Invalid
            
        return (PacketType)data[0];
    }
}
