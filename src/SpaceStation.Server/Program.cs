using Arch.Core;
using Arch.Core.Extensions;
using LiteNetLib;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Systems;
using SpaceStation.Content.Components;
using SpaceStation.Server.Network;
using SpaceStation.Shared.Network;
using SpaceStation.Shared.Network.Packets;
using SpaceStation.Shared.Prototypes;

namespace SpaceStation.Server;

/// <summary>
/// Main server class for Space Station 13 remake.
/// </summary>
public sealed class GameServer : IDisposable
{
    private readonly World _world;
    private readonly SystemManager _systems;
    private readonly ServerNetworkManager _network;

    // Data-driven content
    private readonly PrototypeManager _prototypes;
    private readonly EntityFactory _entityFactory;

    private bool _running;
    private uint _currentTick;

    // Entity ID and sprite tracking for network sync
    private readonly Dictionary<Entity, (int NetId, string SpriteId)> _entityRegistry = new();
    private readonly Dictionary<int, Entity> _netIdToEntity = new();
    private int _nextNetworkId = 1;

    // Query for entities with Transform (all syncable entities)
    private static readonly QueryDescription SyncableQuery = new QueryDescription()
        .WithAll<Transform>();

    public GameServer()
    {
        _world = World.Create();
        _systems = new SystemManager(_world);
        _network = new ServerNetworkManager();

        // Initialize prototype system
        _prototypes = new PrototypeManager();
        _entityFactory = new EntityFactory(_prototypes);

        // Hook entity spawning to network registration
        _entityFactory.OnEntitySpawned += RegisterNetworkEntity;

        // Register core systems
        _systems.RegisterSystem<MovementSystem>();
        _systems.RegisterSystem<HealthSystem>();
        _systems.RegisterSystem<PowerSystem>();
        _systems.RegisterSystem<AtmosSystem>();

        // Subscribe to network events
        _network.OnClientConnected += OnClientConnected;
        _network.OnClientDisconnected += OnClientDisconnected;
    }

    /// <summary>
    /// Initializes and starts the server.
    /// </summary>
    public void Start()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     Space Station 13 - C# Remake       ║");
        Console.WriteLine("║          SERVER v0.4.0-DATA             ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();

        // Load prototypes
        LoadPrototypes();

        _systems.Initialize();

        // Create initial game entities
        CreateGameEntities();

        // Start network
        if (!_network.Start())
        {
            Console.WriteLine("[Server] Failed to start network, exiting.");
            return;
        }

        Console.WriteLine("[Server] Systems initialized");
        Console.WriteLine($"[Server] Tick rate: {NetworkConstants.TickRate} ticks/second");
        Console.WriteLine($"[Server] Entities: {_world.Size}");
        Console.WriteLine("[Server] Starting game loop...");
        Console.WriteLine("[Server] Press Ctrl+C to stop.");
        Console.WriteLine();

        _running = true;
        RunGameLoop();
    }

    private void LoadPrototypes()
    {
        // Try multiple paths to find prototypes
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Resources", "Prototypes"),
            "Resources/Prototypes",
            "../Resources/Prototypes"
        };

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                _prototypes.LoadDirectory(path);
                return;
            }
        }

        Console.WriteLine("[Server] WARNING: Prototypes directory not found!");
    }

    private void CreateGameEntities()
    {
        var pos = System.Numerics.Vector2.Zero;

        // Create floor tiles using prototype
        for (int x = 0; x < 15; x++)
        {
            for (int y = 0; y < 15; y++)
            {
                pos = new System.Numerics.Vector2(x, y);
                _entityFactory.Spawn(_world, "Floor", pos);
            }
        }

        // Create walls around the edge
        for (int i = 0; i < 15; i++)
        {
            // Top and bottom walls
            _entityFactory.Spawn(_world, "Wall", new System.Numerics.Vector2(i, -1));
            _entityFactory.Spawn(_world, "Wall", new System.Numerics.Vector2(i, 15));
        }

        // Create some crew members
        for (int i = 0; i < 5; i++)
        {
            pos = new System.Numerics.Vector2(5 + i * 2, 7);
            _entityFactory.Spawn(_world, "Human", pos);
        }

        // Create power infrastructure
        _entityFactory.Spawn(_world, "Generator", new System.Numerics.Vector2(2, 2));

        // Create some items
        _entityFactory.Spawn(_world, "Toolbox", new System.Numerics.Vector2(8, 3));
        _entityFactory.Spawn(_world, "Wrench", new System.Numerics.Vector2(9, 3));

        Console.WriteLine($"[Server] Created {_entityRegistry.Count} network entities from prototypes");
    }

    private void RegisterNetworkEntity(Entity entity, string spriteId)
    {
        var netId = _nextNetworkId++;
        _entityRegistry[entity] = (netId, spriteId);
        _netIdToEntity[netId] = entity;
    }

    private void OnClientConnected(int clientId, NetPeer peer)
    {
        Console.WriteLine($"[Server] Sending initial world state to client {clientId}");

        // Send full world snapshot to newly connected client
        var snapshot = BuildWorldSnapshot();
        var data = PacketSerializer.Serialize(snapshot);
        _network.SendTo(clientId, data, DeliveryMethod.ReliableOrdered);
    }

    private void OnClientDisconnected(int clientId)
    {
        Console.WriteLine($"[Server] Client {clientId} left the game");
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        _running = false;
        Console.WriteLine("[Server] Shutting down...");
    }

    private void RunGameLoop()
    {
        var tickDuration = TimeSpan.FromSeconds(1.0 / NetworkConstants.TickRate);
        var lastTick = DateTime.UtcNow;
        var lastStatusUpdate = DateTime.UtcNow;

        while (_running)
        {
            var now = DateTime.UtcNow;
            var deltaTime = (float)(now - lastTick).TotalSeconds;
            lastTick = now;

            // Poll network events
            _network.PollEvents();

            // Update all systems
            _systems.Update(deltaTime);

            _currentTick++;

            // Broadcast world state to all clients
            if (_network.ClientCount > 0)
            {
                BroadcastWorldState();
            }

            // Print status every 5 seconds
            if ((now - lastStatusUpdate).TotalSeconds >= 5)
            {
                Console.WriteLine($"[Server] Tick {_currentTick}, Entities: {_world.Size}, Clients: {_network.ClientCount}");
                lastStatusUpdate = now;
            }

            // Sleep until next tick
            var elapsed = DateTime.UtcNow - now;
            var sleepTime = tickDuration - elapsed;
            if (sleepTime > TimeSpan.Zero)
            {
                Thread.Sleep(sleepTime);
            }
        }
    }

    private WorldSnapshotPacket BuildWorldSnapshot()
    {
        var entities = new List<NetworkEntity>();

        _world.Query(in SyncableQuery, (Entity entity, ref Transform transform) =>
        {
            if (!_entityRegistry.TryGetValue(entity, out var registration))
                return;

            var netEntity = new NetworkEntity
            {
                EntityId = registration.NetId,
                Transform = new NetworkTransform(
                    transform.Position.X,
                    transform.Position.Y,
                    transform.Rotation,
                    transform.ZLevel
                ),
                // Include sprite reference
                Sprite = new NetworkSprite
                {
                    TextureId = registration.SpriteId,
                    TintR = 255,
                    TintG = 255,
                    TintB = 255,
                    TintA = 255,
                    Scale = 1f
                }
            };

            // Add optional components
            if (_world.Has<Physics>(entity))
            {
                var physics = _world.Get<Physics>(entity);
                netEntity.Physics = new NetworkPhysics
                {
                    VelocityX = physics.Velocity.X,
                    VelocityY = physics.Velocity.Y,
                    MoveSpeed = physics.MoveSpeed,
                    Dense = physics.Dense,
                    Anchored = physics.Anchored
                };
            }

            if (_world.Has<Health>(entity))
            {
                var health = _world.Get<Health>(entity);
                netEntity.Health = new NetworkHealth
                {
                    MaxHealth = health.MaxHealth,
                    CurrentHealth = health.CurrentHealth,
                    State = (byte)health.State
                };
            }

            entities.Add(netEntity);
        });

        return new WorldSnapshotPacket
        {
            Header = new PacketHeader
            {
                Type = PacketType.WorldSnapshot,
                Tick = _currentTick,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            },
            Entities = entities.ToArray(),
            PlayerCount = _network.ClientCount
        };
    }

    private void BroadcastWorldState()
    {
        var snapshot = BuildWorldSnapshot();
        var data = PacketSerializer.Serialize(snapshot);
        // Use ReliableUnordered - supports fragmentation for large packets
        _network.Broadcast(data, DeliveryMethod.ReliableUnordered);
    }

    public void Dispose()
    {
        _running = false;
        _network.Dispose();
        _systems.Dispose();
        _world.Dispose();
    }
}

/// <summary>
/// Server entry point.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        using var server = new GameServer();

        // Handle Ctrl+C for graceful shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            server.Stop();
        };

        server.Start();

        Console.WriteLine("[Server] Shutdown complete.");
    }
}
