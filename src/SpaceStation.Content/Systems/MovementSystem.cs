using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Components;

namespace SpaceStation.Content.Systems;

/// <summary>
/// System handling grid-based tile movement.
/// Uses MoveTarget for discrete tile-to-tile movement (SS13 style).
/// </summary>
public sealed class MovementSystem : BaseSystem
{
    public override int Priority => 10;

    // Entities currently moving (have MoveTarget)
    private static readonly QueryDescription MovingQuery = new QueryDescription()
        .WithAll<Transform, MoveTarget>();
    
    // Entities with input that might want to start moving
    private static readonly QueryDescription InputQuery = new QueryDescription()
        .WithAll<Transform, Physics, InputState>()
        .WithNone<MoveTarget>();

    /// <summary>
    /// Optional collision checker callback.
    /// Returns true if tile is passable.
    /// </summary>
    public Func<int, int, bool>? IsPassable { get; set; }

    public override void Update(float deltaTime, World world)
    {
        // Process entities currently moving
        ProcessMovingEntities(world, deltaTime);
        
        // Process input for entities that can start moving
        ProcessInputEntities(world);
    }
    
    private void ProcessMovingEntities(World world, float deltaTime)
    {
        var toComplete = new List<Entity>();
        
        world.Query(in MovingQuery, (Entity entity, ref Transform transform, ref MoveTarget target) =>
        {
            // Advance progress
            target.Progress += target.Speed * deltaTime;
            
            if (target.Progress >= 1f)
            {
                // Snap to exact tile position
                transform.Position = target.TargetPosition;
                toComplete.Add(entity);
            }
            else
            {
                // Lerp towards target
                transform.Position = Vector2.Lerp(target.StartPosition, target.TargetPosition, target.Progress);
            }
        });
        
        // Remove MoveTarget from completed entities
        foreach (var entity in toComplete)
        {
            if (world.IsAlive(entity) && world.Has<MoveTarget>(entity))
            {
                world.Remove<MoveTarget>(entity);
            }
        }
    }
    
    private void ProcessInputEntities(World world)
    {
        var toStartMoving = new List<(Entity entity, int dx, int dy)>();
        
        world.Query(in InputQuery, (Entity entity, ref Transform transform, ref Physics physics, ref InputState input) =>
        {
            if (physics.Anchored)
                return;
            
            // Check for movement input
            int dx = 0, dy = 0;
            
            if (input.MoveX < -0.5f) dx = -1;
            else if (input.MoveX > 0.5f) dx = 1;
            
            if (input.MoveY < -0.5f) dy = -1;
            else if (input.MoveY > 0.5f) dy = 1;
            
            // Only allow cardinal movement (no diagonals in SS13)
            if (dx != 0 && dy != 0)
            {
                // Prefer horizontal movement
                dy = 0;
            }
            
            if (dx != 0 || dy != 0)
            {
                toStartMoving.Add((entity, dx, dy));
            }
        });
        
        // Start movement for entities
        foreach (var (entity, dx, dy) in toStartMoving)
        {
            if (!world.IsAlive(entity)) continue;
            
            var transform = world.Get<Transform>(entity);
            var physics = world.Get<Physics>(entity);
            
            // Calculate target tile
            int currentX = (int)MathF.Round(transform.Position.X);
            int currentY = (int)MathF.Round(transform.Position.Y);
            int targetX = currentX + dx;
            int targetY = currentY + dy;
            
            // Check collision
            bool canMove = IsPassable?.Invoke(targetX, targetY) ?? true;
            
            if (canMove)
            {
                // Start moving
                var moveTarget = new MoveTarget(targetX, targetY, 0f, physics.MoveSpeed)
                {
                    StartPosition = transform.Position
                };
                world.Add(entity, moveTarget);
            }
        }
    }
    
    /// <summary>
    /// Instantly starts movement for an entity (for testing/direct control).
    /// </summary>
    public static void StartMove(World world, Entity entity, int dx, int dy)
    {
        if (!world.IsAlive(entity)) return;
        if (!world.Has<Transform>(entity)) return;
        if (world.Has<MoveTarget>(entity)) return; // Already moving
        
        var transform = world.Get<Transform>(entity);
        float speed = world.Has<Physics>(entity) ? world.Get<Physics>(entity).MoveSpeed : 4f;
        
        int currentX = (int)MathF.Round(transform.Position.X);
        int currentY = (int)MathF.Round(transform.Position.Y);
        
        var target = new MoveTarget(currentX + dx, currentY + dy, 0f, speed)
        {
            StartPosition = transform.Position
        };
        world.Add(entity, target);
    }
}
