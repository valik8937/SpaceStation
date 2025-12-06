using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Systems;
using SpaceStation.Content.Components;
using SpaceStation.Client.Graphics;
using SpaceStation.Client.Network;
using SpaceStation.Client.Resources;
using SpaceStation.Shared.Network;

namespace SpaceStation.Client;

/// <summary>
/// Main MonoGame game class for Space Station 13 client.
/// </summary>
public class SpaceStationGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    // ECS
    private World _world = null!;
    private SystemManager _systems = null!;

    // Rendering
    private RenderSystem _renderSystem = null!;
    private Camera _camera;

    // Network
    private ClientNetworkManager _network = null!;
    private EntitySyncSystem _entitySync = null!;
    private bool _connected;
    private uint _lastReceivedTick;
    private int _playerCount;

    // Resources
    private ResourceManager _resourceManager = null!;

    // Debug
    private Texture2D? _pixel;
    private int _entityCount;
    private double _fps;

    public SpaceStationGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            SynchronizeWithVerticalRetrace = true
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Space Station 13 - C# Remake (Connecting...)";
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     Space Station 13 - C# Remake       ║");
        Console.WriteLine("║          CLIENT v0.3.0-NET              ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();

        // Initialize ECS World
        _world = World.Create();
        _systems = new SystemManager(_world);

        // Register game systems (for client-side prediction in the future)
        _systems.RegisterSystem<MovementSystem>();
        _systems.Initialize();

        // Initialize render system
        _renderSystem = new RenderSystem();

        // Initialize entity sync
        _entitySync = new EntitySyncSystem();

        // Initialize network
        _network = new ClientNetworkManager();
        _network.OnConnected += OnConnected;
        _network.OnDisconnected += OnDisconnected;
        _network.Start();

        // Connect to localhost by default
        _network.Connect("localhost", NetworkConstants.DefaultPort);

        // Initialize camera
        _camera = new Camera(new Vector2(7 * 32, 7 * 32), 1f, 0f); // Center on map

        Console.WriteLine("[Client] Attempting to connect to server...");

        base.Initialize();
    }

    private void OnConnected(int clientId)
    {
        _connected = true;
        Window.Title = $"Space Station 13 - Client #{clientId}";
        Console.WriteLine($"[Client] Connected! Client ID: {clientId}");
    }

    private void OnDisconnected(string reason)
    {
        _connected = false;
        Window.Title = "Space Station 13 - Disconnected";
        Console.WriteLine($"[Client] Disconnected: {reason}");

        // Clear synced entities
        _entitySync.Clear(_world);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Initialize resource manager and load textures
        _resourceManager = new ResourceManager(GraphicsDevice);
        
        // Try to load textures from Resources/Textures
        var texturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Resources", "Textures");
        if (Directory.Exists(texturePath))
        {
            _resourceManager.LoadDirectory(texturePath);
        }
        else
        {
            // Try relative to working directory
            texturePath = "Resources/Textures";
            if (Directory.Exists(texturePath))
            {
                _resourceManager.LoadDirectory(texturePath);
            }
            else
            {
                Console.WriteLine($"[Client] WARNING: Texture directory not found!");
            }
        }

        // Set resources for entity sync
        _entitySync.DefaultTexture = _pixel;
        _entitySync.ResourceManager = _resourceManager;

        Console.WriteLine($"[Client] Content loaded - {_resourceManager.TotalStates} sprite states available");
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;

        // Poll network
        _network.PollEvents();

        // Process network snapshots
        ProcessNetworkSnapshots();

        // Handle input
        HandleInput(deltaTime);

        // Update all ECS systems (for client-side prediction)
        // _systems.Update(deltaTime);

        // Update animations
        _renderSystem.UpdateAnimations(_world, deltaTime);

        // Track entity count
        _entityCount = _world.Size;

        base.Update(gameTime);
    }

    private void ProcessNetworkSnapshots()
    {
        // Process all available snapshots
        while (true)
        {
            var snapshot = _network.DequeueSnapshot();
            if (snapshot == null)
                break;

            _lastReceivedTick = snapshot.Header.Tick;
            _playerCount = snapshot.PlayerCount;

            // Apply snapshot to local world
            _entitySync.ApplySnapshot(_world, snapshot);
        }
    }

    private void HandleInput(float deltaTime)
    {
        var keyboard = Keyboard.GetState();

        // Exit on Escape
        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // Reconnect on R
        if (keyboard.IsKeyDown(Keys.R) && !_connected)
        {
            _network.Connect("localhost", NetworkConstants.DefaultPort);
        }

        // Camera movement
        const float cameraSpeed = 300f;
        var cameraMove = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            cameraMove.Y -= cameraSpeed * deltaTime;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            cameraMove.Y += cameraSpeed * deltaTime;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            cameraMove.X -= cameraSpeed * deltaTime;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            cameraMove.X += cameraSpeed * deltaTime;

        _camera.Position += cameraMove;

        // Camera zoom
        if (keyboard.IsKeyDown(Keys.OemPlus) || keyboard.IsKeyDown(Keys.Add))
            _camera.Zoom = MathF.Min(4f, _camera.Zoom + deltaTime * 2);
        if (keyboard.IsKeyDown(Keys.OemMinus) || keyboard.IsKeyDown(Keys.Subtract))
            _camera.Zoom = MathF.Max(0.25f, _camera.Zoom - deltaTime * 2);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 10, 20)); // Dark space background

        // Render all entities
        _renderSystem.Draw(_world, _spriteBatch, _camera, GraphicsDevice.Viewport);

        // Draw debug info
        DrawDebugInfo();

        base.Draw(gameTime);
    }

    private void DrawDebugInfo()
    {
        _spriteBatch.Begin();

        // Background panel
        var panelWidth = 220;
        var panelHeight = 100;
        _spriteBatch.Draw(_pixel, new Rectangle(10, 10, panelWidth, panelHeight), new Color(0, 0, 0, 200));

        // Connection status indicator
        var statusColor = _connected ? Color.LimeGreen : Color.Red;
        _spriteBatch.Draw(_pixel, new Rectangle(20, 20, 20, 20), statusColor);

        // FPS bar
        var fpsColor = _fps >= 55 ? Color.LimeGreen : (_fps >= 30 ? Color.Yellow : Color.Red);
        _spriteBatch.Draw(_pixel, new Rectangle(50, 20, (int)MathF.Min((float)_fps * 2, 160), 8), fpsColor);

        // Entity count bar
        _spriteBatch.Draw(_pixel, new Rectangle(50, 35, MathF.Min(_entityCount / 2, 160).ToInt(), 8), Color.Cyan);

        // Tick indicator
        var tickColor = Color.Magenta;
        _spriteBatch.Draw(_pixel, new Rectangle(50, 50, (int)(_lastReceivedTick % 100) * 2, 8), tickColor);

        // Ping bar
        var ping = _network.Ping;
        var pingColor = ping < 50 ? Color.LimeGreen : (ping < 100 ? Color.Yellow : Color.Red);
        _spriteBatch.Draw(_pixel, new Rectangle(50, 65, MathF.Min(ping / 2, 160).ToInt(), 8), pingColor);

        // Player count indicator
        _spriteBatch.Draw(_pixel, new Rectangle(50, 80, _playerCount * 20, 8), Color.Orange);

        _spriteBatch.End();
    }

    protected override void UnloadContent()
    {
        _network?.Dispose();
        _world?.Dispose();
        _systems?.Dispose();
        _resourceManager?.Dispose();
        _pixel?.Dispose();

        Console.WriteLine("[Client] Shutdown complete.");
    }
}

internal static class MathExtensions
{
    public static int ToInt(this float value) => (int)value;
}

/// <summary>
/// Client entry point.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        using var game = new SpaceStationGame();
        game.Run();
    }
}
