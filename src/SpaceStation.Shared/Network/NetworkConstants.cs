namespace SpaceStation.Shared.Network;

/// <summary>
/// Network configuration constants.
/// </summary>
public static class NetworkConstants
{
    /// <summary>Default server port.</summary>
    public const int DefaultPort = 7777;
    
    /// <summary>Connection key for authentication.</summary>
    public const string ConnectionKey = "SS13_REMAKE_v1";
    
    /// <summary>Maximum number of connected clients.</summary>
    public const int MaxPlayers = 100;
    
    /// <summary>Server tick rate (updates per second).</summary>
    public const int TickRate = 20;
    
    /// <summary>Tick duration in seconds.</summary>
    public const float TickDuration = 1f / TickRate;
    
    /// <summary>Connection timeout in milliseconds.</summary>
    public const int ConnectionTimeout = 5000;
    
    /// <summary>Disconnect timeout in milliseconds.</summary>
    public const int DisconnectTimeout = 5000;
    
    /// <summary>Ping interval in milliseconds.</summary>
    public const int PingInterval = 1000;
}
