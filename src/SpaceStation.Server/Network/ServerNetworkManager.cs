using LiteNetLib;
using LiteNetLib.Utils;
using SpaceStation.Shared.Network;
using SpaceStation.Shared.Network.Packets;

namespace SpaceStation.Server.Network;

/// <summary>
/// Server-side network manager handling client connections and packet broadcasting.
/// </summary>
public sealed class ServerNetworkManager : INetEventListener, IDisposable
{
    private readonly NetManager _netManager;
    private readonly Dictionary<int, NetPeer> _clients = new();
    private int _nextClientId = 1;

    public bool IsRunning => _netManager.IsRunning;
    public int ClientCount => _clients.Count;

    /// <summary>Event when a client connects.</summary>
    public event Action<int, NetPeer>? OnClientConnected;

    /// <summary>Event when a client disconnects.</summary>
    public event Action<int>? OnClientDisconnected;

    /// <summary>Event when data is received from a client.</summary>
    public event Action<int, byte[]>? OnDataReceived;

    public ServerNetworkManager()
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
    /// Starts the server on the specified port.
    /// </summary>
    public bool Start(int port = NetworkConstants.DefaultPort)
    {
        var result = _netManager.Start(port);
        if (result)
        {
            Console.WriteLine($"[Network] Server started on port {port}");
        }
        else
        {
            Console.WriteLine($"[Network] Failed to start server on port {port}");
        }
        return result;
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        _netManager.Stop();
        _clients.Clear();
        Console.WriteLine("[Network] Server stopped");
    }

    /// <summary>
    /// Polls network events. Should be called every frame/tick.
    /// </summary>
    public void PollEvents()
    {
        _netManager.PollEvents();
    }

    /// <summary>
    /// Broadcasts data to all connected clients.
    /// </summary>
    public void Broadcast(byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        foreach (var client in _clients.Values)
        {
            client.Send(data, deliveryMethod);
        }
    }

    /// <summary>
    /// Sends data to a specific client.
    /// </summary>
    public void SendTo(int clientId, byte[] data, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        if (_clients.TryGetValue(clientId, out var peer))
        {
            peer.Send(data, deliveryMethod);
        }
    }

    /// <summary>
    /// Disconnects a specific client.
    /// </summary>
    public void DisconnectClient(int clientId)
    {
        if (_clients.TryGetValue(clientId, out var peer))
        {
            peer.Disconnect();
        }
    }

    // INetEventListener implementation

    public void OnConnectionRequest(ConnectionRequest request)
    {
        var key = request.Data.GetString();
        if (key == NetworkConstants.ConnectionKey && _clients.Count < NetworkConstants.MaxPlayers)
        {
            request.Accept();
            Console.WriteLine($"[Network] Connection request accepted from {request.RemoteEndPoint}");
        }
        else
        {
            request.Reject();
            Console.WriteLine($"[Network] Connection request rejected from {request.RemoteEndPoint}");
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        var clientId = _nextClientId++;
        _clients[clientId] = peer;

        Console.WriteLine($"[Network] Client {clientId} connected from {peer.Address}:{peer.Port}");

        // Send connection accepted
        var acceptPacket = new ConnectionAcceptedPacket
        {
            ClientId = clientId,
            TickRate = NetworkConstants.TickRate
        };
        var data = PacketSerializer.Serialize(acceptPacket);
        peer.Send(data, DeliveryMethod.ReliableOrdered);

        OnClientConnected?.Invoke(clientId, peer);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var clientId = _clients.FirstOrDefault(x => x.Value == peer).Key;
        if (clientId != 0)
        {
            _clients.Remove(clientId);
            Console.WriteLine($"[Network] Client {clientId} disconnected: {disconnectInfo.Reason}");
            OnClientDisconnected?.Invoke(clientId);
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var clientId = _clients.FirstOrDefault(x => x.Value == peer).Key;
        if (clientId != 0)
        {
            var data = new byte[reader.AvailableBytes];
            reader.GetBytes(data, data.Length);
            OnDataReceived?.Invoke(clientId, data);
        }
    }

    public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
    {
        Console.WriteLine($"[Network] Error from {endPoint}: {socketError}");
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
