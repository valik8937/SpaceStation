using LiteNetLib;
using LiteNetLib.Utils;
using SpaceStation.Shared.Network;
using SpaceStation.Shared.Network.Packets;

namespace SpaceStation.Client.Network;

/// <summary>
/// Client-side network manager for connecting to server and receiving game state.
/// </summary>
public sealed class ClientNetworkManager : INetEventListener, IDisposable
{
    private readonly NetManager _netManager;
    private NetPeer? _serverPeer;
    private int _clientId;
    
    private readonly Queue<WorldSnapshotPacket> _snapshotQueue = new();
    private readonly object _queueLock = new();
    
    public bool IsConnected => _serverPeer?.ConnectionState == ConnectionState.Connected;
    public int ClientId => _clientId;
    public int Ping => _serverPeer?.Ping ?? 0;
    
    /// <summary>Event when connected to server.</summary>
    public event Action<int>? OnConnected;
    
    /// <summary>Event when disconnected from server.</summary>
    public event Action<string>? OnDisconnected;
    
    /// <summary>Event when world snapshot received.</summary>
    public event Action<WorldSnapshotPacket>? OnSnapshotReceived;
    
    public ClientNetworkManager()
    {
        _netManager = new NetManager(this)
        {
            AutoRecycle = true,
            DisconnectTimeout = NetworkConstants.DisconnectTimeout,
            PingInterval = NetworkConstants.PingInterval,
            UnconnectedMessagesEnabled = false
        };
    }
    
    /// <summary>
    /// Starts the client network manager.
    /// </summary>
    public void Start()
    {
        _netManager.Start();
        Console.WriteLine($"[Network] Client started");
    }
    
    /// <summary>
    /// Connects to a server.
    /// </summary>
    public void Connect(string host = "localhost", int port = NetworkConstants.DefaultPort)
    {
        Console.WriteLine($"[Network] Connecting to {host}:{port}...");
        
        var writer = new NetDataWriter();
        writer.Put(NetworkConstants.ConnectionKey);
        
        _netManager.Connect(host, port, writer);
    }
    
    /// <summary>
    /// Disconnects from the server.
    /// </summary>
    public void Disconnect()
    {
        _serverPeer?.Disconnect();
    }
    
    /// <summary>
    /// Stops the client.
    /// </summary>
    public void Stop()
    {
        _netManager.Stop();
        _serverPeer = null;
        Console.WriteLine("[Network] Client stopped");
    }
    
    /// <summary>
    /// Polls network events. Should be called every frame.
    /// </summary>
    public void PollEvents()
    {
        _netManager.PollEvents();
    }
    
    /// <summary>
    /// Gets the next snapshot from the queue.
    /// </summary>
    public WorldSnapshotPacket? DequeueSnapshot()
    {
        lock (_queueLock)
        {
            return _snapshotQueue.Count > 0 ? _snapshotQueue.Dequeue() : null;
        }
    }
    
    /// <summary>
    /// Sends data to the server.
    /// </summary>
    public void Send(byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        _serverPeer?.Send(data, deliveryMethod);
    }
    
    // INetEventListener implementation
    
    public void OnConnectionRequest(ConnectionRequest request)
    {
        // Client doesn't accept incoming connections
        request.Reject();
    }
    
    public void OnPeerConnected(NetPeer peer)
    {
        _serverPeer = peer;
        Console.WriteLine($"[Network] Connected to server at {peer.Address}:{peer.Port}");
    }
    
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _serverPeer = null;
        Console.WriteLine($"[Network] Disconnected from server: {disconnectInfo.Reason}");
        OnDisconnected?.Invoke(disconnectInfo.Reason.ToString());
    }
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var data = new byte[reader.AvailableBytes];
        reader.GetBytes(data, data.Length);
        
        ProcessPacket(data);
    }
    
    private void ProcessPacket(byte[] data)
    {
        try
        {
            // Try to deserialize as ConnectionAccepted first
            var connectionAccepted = PacketSerializer.DeserializeConnectionAccepted(data);
            if (connectionAccepted != null && connectionAccepted.Header.Type == PacketType.ConnectionAccepted)
            {
                _clientId = connectionAccepted.ClientId;
                Console.WriteLine($"[Network] Connection accepted! Client ID: {_clientId}");
                OnConnected?.Invoke(_clientId);
                return;
            }
            
            // Try to deserialize as WorldSnapshot
            var snapshot = PacketSerializer.DeserializeSnapshot(data);
            if (snapshot != null && snapshot.Header.Type == PacketType.WorldSnapshot)
            {
                lock (_queueLock)
                {
                    // Keep only latest few snapshots to prevent lag buildup
                    while (_snapshotQueue.Count > 3)
                    {
                        _snapshotQueue.Dequeue();
                    }
                    _snapshotQueue.Enqueue(snapshot);
                }
                OnSnapshotReceived?.Invoke(snapshot);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network] Error processing packet: {ex.Message}");
        }
    }
    
    public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
    {
        Console.WriteLine($"[Network] Error: {socketError}");
    }
    
    public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // Not used
    }
    
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        // Could be used for ping display
    }
    
    public void Dispose()
    {
        Stop();
    }
}
