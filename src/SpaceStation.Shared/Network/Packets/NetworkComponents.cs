using MemoryPack;

namespace SpaceStation.Shared.Network.Packets;

/// <summary>
/// Network-serializable transform data.
/// </summary>
[MemoryPackable]
public partial struct NetworkTransform
{
    public float X;
    public float Y;
    public float Rotation;
    public int ZLevel;
    
    public NetworkTransform(float x, float y, float rotation = 0f, int zLevel = 0)
    {
        X = x;
        Y = y;
        Rotation = rotation;
        ZLevel = zLevel;
    }
}

/// <summary>
/// Network-serializable physics data.
/// </summary>
[MemoryPackable]
public partial struct NetworkPhysics
{
    public float VelocityX;
    public float VelocityY;
    public float MoveSpeed;
    public bool Dense;
    public bool Anchored;
}

/// <summary>
/// Network-serializable sprite data.
/// </summary>
[MemoryPackable]
public partial struct NetworkSprite
{
    public string TextureId;
    public int SourceX;
    public int SourceY;
    public int SourceWidth;
    public int SourceHeight;
    public byte TintR;
    public byte TintG;
    public byte TintB;
    public byte TintA;
    public float Scale;
}

/// <summary>
/// Network-serializable health data.
/// </summary>
[MemoryPackable]
public partial struct NetworkHealth
{
    public float MaxHealth;
    public float CurrentHealth;
    public byte State; // MobState as byte
}

/// <summary>
/// A complete entity state for network transmission.
/// </summary>
[MemoryPackable]
public partial class NetworkEntity
{
    /// <summary>Server-side entity ID for tracking.</summary>
    public int EntityId { get; set; }
    
    /// <summary>Prototype ID for spawning.</summary>
    public string? PrototypeId { get; set; }
    
    /// <summary>Transform component (always present).</summary>
    public NetworkTransform Transform { get; set; }
    
    /// <summary>Optional physics data.</summary>
    public NetworkPhysics? Physics { get; set; }
    
    /// <summary>Optional sprite data.</summary>
    public NetworkSprite? Sprite { get; set; }
    
    /// <summary>Optional health data.</summary>
    public NetworkHealth? Health { get; set; }
}
