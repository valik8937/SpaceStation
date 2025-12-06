using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using SpaceStation.Content.Components;
using SpaceStation.Client.Graphics;
using SpaceStation.Shared.Network.Packets;
using SpaceStation.Shared.Enums;

namespace SpaceStation.Client.Network;

/// <summary>
/// Synchronizes local ECS entities with server world state.
/// </summary>
public sealed class EntitySyncSystem
{
    private readonly Dictionary<int, Entity> _networkToLocal = new();
    private readonly Dictionary<Entity, int> _localToNetwork = new();

    /// <summary>
    /// Texture to use for entities (must be set before ApplySnapshot).
    /// </summary>
    public Microsoft.Xna.Framework.Graphics.Texture2D? DefaultTexture { get; set; }

    /// <summary>
    /// Applies a world snapshot to the local ECS world.
    /// </summary>
    public void ApplySnapshot(World world, WorldSnapshotPacket snapshot)
    {
        var receivedIds = new HashSet<int>();

        foreach (var netEntity in snapshot.Entities)
        {
            receivedIds.Add(netEntity.EntityId);

            if (_networkToLocal.TryGetValue(netEntity.EntityId, out var localEntity))
            {
                // Update existing entity
                UpdateEntity(world, localEntity, netEntity);
            }
            else
            {
                // Create new entity
                localEntity = CreateEntity(world, netEntity);
                _networkToLocal[netEntity.EntityId] = localEntity;
                _localToNetwork[localEntity] = netEntity.EntityId;
            }
        }

        // Remove entities that no longer exist on server
        var toRemove = _networkToLocal
            .Where(kvp => !receivedIds.Contains(kvp.Key))
            .ToList();

        foreach (var kvp in toRemove)
        {
            if (world.IsAlive(kvp.Value))
            {
                world.Destroy(kvp.Value);
            }
            _networkToLocal.Remove(kvp.Key);
            _localToNetwork.Remove(kvp.Value);
        }
    }

    private Entity CreateEntity(World world, NetworkEntity netEntity)
    {
        // Create base entity with transform
        var transform = new Transform(
            new System.Numerics.Vector2(netEntity.Transform.X, netEntity.Transform.Y),
            netEntity.Transform.Rotation,
            netEntity.Transform.ZLevel
        );

        // Determine sprite color based on entity properties
        var color = DetermineColor(netEntity);

        var sprite = new Sprite(
            DefaultTexture,
            Microsoft.Xna.Framework.Rectangle.Empty,
            color,
            1f,
            0.5f
        );

        Entity entity;

        if (netEntity.Physics.HasValue)
        {
            var physics = netEntity.Physics.Value;
            var physicsComp = new Physics(
                new System.Numerics.Vector2(physics.VelocityX, physics.VelocityY),
                physics.MoveSpeed,
                1f,
                0.5f,
                physics.Dense,
                physics.Anchored
            );

            if (netEntity.Health.HasValue)
            {
                var health = netEntity.Health.Value;
                entity = world.Create(
                    transform,
                    physicsComp,
                    sprite,
                    new Health(health.MaxHealth, health.CurrentHealth, (MobState)health.State),
                    new Damageable()
                );
            }
            else
            {
                entity = world.Create(transform, physicsComp, sprite);
            }
        }
        else
        {
            entity = world.Create(transform, sprite);
        }

        return entity;
    }

    private void UpdateEntity(World world, Entity entity, NetworkEntity netEntity)
    {
        if (!world.IsAlive(entity))
            return;

        // Update transform
        if (world.Has<Transform>(entity))
        {
            ref var transform = ref world.Get<Transform>(entity);
            transform = new Transform(
                new System.Numerics.Vector2(netEntity.Transform.X, netEntity.Transform.Y),
                netEntity.Transform.Rotation,
                netEntity.Transform.ZLevel
            );
        }

        // Update physics
        if (netEntity.Physics.HasValue && world.Has<Physics>(entity))
        {
            ref var physics = ref world.Get<Physics>(entity);
            var net = netEntity.Physics.Value;
            physics = new Physics(
                new System.Numerics.Vector2(net.VelocityX, net.VelocityY),
                net.MoveSpeed,
                physics.Mass,
                physics.Friction,
                net.Dense,
                net.Anchored
            );
        }

        // Update health
        if (netEntity.Health.HasValue && world.Has<Health>(entity))
        {
            ref var health = ref world.Get<Health>(entity);
            var net = netEntity.Health.Value;
            health = new Health(net.MaxHealth, net.CurrentHealth, (MobState)net.State);
        }
    }

    private Color DetermineColor(NetworkEntity netEntity)
    {
        // Color based on entity type
        if (netEntity.Health.HasValue)
        {
            // Mobs - yellow/orange
            return Color.Yellow;
        }
        else if (netEntity.Physics.HasValue)
        {
            if (netEntity.Physics.Value.Anchored)
            {
                if (netEntity.Physics.Value.Dense)
                {
                    // Wall - dark gray
                    return new Color(60, 60, 70);
                }
                else
                {
                    // Floor - varied colors
                    var hue = ((int)(netEntity.Transform.X + netEntity.Transform.Y) * 37) % 360;
                    return HsvToRgb(hue, 0.4f, 0.6f);
                }
            }
        }

        // Default - white
        return Color.White;
    }

    private static Color HsvToRgb(float h, float s, float v)
    {
        float c = v * s;
        float x = c * (1 - MathF.Abs((h / 60f) % 2 - 1));
        float m = v - c;

        float r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return new Color(r + m, g + m, b + m);
    }

    /// <summary>
    /// Clears all tracked entities.
    /// </summary>
    public void Clear(World world)
    {
        foreach (var entity in _networkToLocal.Values)
        {
            if (world.IsAlive(entity))
            {
                world.Destroy(entity);
            }
        }
        _networkToLocal.Clear();
        _localToNetwork.Clear();
    }

    /// <summary>
    /// Gets statistics about synced entities.
    /// </summary>
    public int SyncedEntityCount => _networkToLocal.Count;
}
