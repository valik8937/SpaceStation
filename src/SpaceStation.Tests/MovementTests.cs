using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using SpaceStation.Content.Components;
using SpaceStation.Content.Systems;

namespace SpaceStation.Tests;

/// <summary>
/// Tests for grid-based movement system.
/// </summary>
[TestFixture]
public class MovementTests
{
    private World _world = null!;
    private MovementSystem _movementSystem = null!;

    [SetUp]
    public void Setup()
    {
        _world = World.Create();
        _movementSystem = new MovementSystem();
    }

    [TearDown]
    public void Teardown()
    {
        _world.Dispose();
    }

    /// <summary>
    /// Test that entity snaps to exact tile coordinates after movement.
    /// Position should be EXACTLY (6, 5), not (6.2, 5) or (5.8, 5).
    /// </summary>
    [Test]
    public void Test_Movement_Snapping()
    {
        // Arrange: Entity at (5, 5)
        var entity = _world.Create(
            new Transform(new Vector2(5, 5), 0f, 0),
            new Physics(Vector2.Zero, 4f, 70f, 0f, true, false),
            new InputState(MoveX: 1, MoveY: 0) // Right
        );

        // Act: First update starts movement
        _movementSystem.Update(0.05f, _world);

        // Clear input so we only do one move
        _world.Set(entity, new InputState(0, 0));

        // Continue until move completes
        for (int i = 0; i < 30; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Assert: Position should be EXACTLY (6, 5)
        var transform = _world.Get<Transform>(entity);
        Assert.That(transform.Position.X, Is.EqualTo(6f).Within(0.001f), "X position should be exactly 6");
        Assert.That(transform.Position.Y, Is.EqualTo(5f).Within(0.001f), "Y position should be exactly 5");
    }

    /// <summary>
    /// Test that entity cannot move through a wall.
    /// </summary>
    [Test]
    public void Test_Wall_Collision()
    {
        // Arrange: Set up collision checker to block (6, 5)
        _movementSystem.IsPassable = (x, y) => !(x == 6 && y == 5);

        // Entity at (5, 5) trying to move right into wall
        var entity = _world.Create(
            new Transform(new Vector2(5, 5), 0f, 0),
            new Physics(Vector2.Zero, 4f, 70f, 0f, true, false),
            new InputState(MoveX: 1, MoveY: 0) // Right
        );

        // Act: Simulate movement
        _movementSystem.Update(0.05f, _world);
        _world.Set(entity, new InputState(0, 0));

        for (int i = 0; i < 20; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Assert: Position should remain at (5, 5)
        var transform = _world.Get<Transform>(entity);
        Assert.That(transform.Position.X, Is.EqualTo(5f).Within(0.001f), "X should stay at 5 (blocked)");
        Assert.That(transform.Position.Y, Is.EqualTo(5f).Within(0.001f), "Y should stay at 5");
    }

    /// <summary>
    /// Test that diagonal input is resolved to cardinal direction.
    /// </summary>
    [Test]
    public void Test_Diagonal_Prevention()
    {
        // Arrange: Entity with diagonal input (right + down)
        var entity = _world.Create(
            new Transform(new Vector2(5, 5), 0f, 0),
            new Physics(Vector2.Zero, 4f, 70f, 0f, true, false),
            new InputState(MoveX: 1, MoveY: 1) // Diagonal
        );

        // Act: Single update starts movement
        _movementSystem.Update(0.05f, _world);

        // Clear input
        _world.Set(entity, new InputState(0, 0));

        // Complete the move
        for (int i = 0; i < 30; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Assert: Should only move in one cardinal direction (horizontal preferred)
        var transform = _world.Get<Transform>(entity);

        // Should have moved exactly 1 tile right
        Assert.That(transform.Position.X, Is.EqualTo(6f).Within(0.001f),
            $"X should be 6 (moved right). Actual: {transform.Position.X}");
        Assert.That(transform.Position.Y, Is.EqualTo(5f).Within(0.001f),
            $"Y should stay at 5. Actual: {transform.Position.Y}");
    }

    /// <summary>
    /// Test that anchored entities don't move.
    /// </summary>
    [Test]
    public void Test_Anchored_Entity_NoMove()
    {
        // Arrange: Anchored entity
        var entity = _world.Create(
            new Transform(new Vector2(5, 5), 0f, 0),
            new Physics(Vector2.Zero, 4f, 70f, 0f, true, true), // Anchored = true
            new InputState(MoveX: 1, MoveY: 0)
        );

        // Act
        for (int i = 0; i < 20; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Assert: Position unchanged
        var transform = _world.Get<Transform>(entity);
        Assert.That(transform.Position.X, Is.EqualTo(5f).Within(0.001f));
        Assert.That(transform.Position.Y, Is.EqualTo(5f).Within(0.001f));
    }

    /// <summary>
    /// Test multiple tile movements in sequence with persistent input.
    /// </summary>
    [Test]
    public void Test_Multiple_Tile_Movement()
    {
        // Arrange: Entity with continuous right input
        var entity = _world.Create(
            new Transform(new Vector2(5, 5), 0f, 0),
            new Physics(Vector2.Zero, 4f, 70f, 0f, true, false),
            new InputState(MoveX: 1, MoveY: 0)
        );

        // Act: Simulate long enough for multiple moves
        for (int i = 0; i < 80; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Clear input and wait for final move to complete
        _world.Set(entity, new InputState(0, 0));
        for (int i = 0; i < 30; i++)
        {
            _movementSystem.Update(0.05f, _world);
        }

        // Assert: Should have moved multiple tiles to the right
        var transform = _world.Get<Transform>(entity);
        Assert.That(transform.Position.X, Is.GreaterThan(6f), "Should have moved multiple tiles right");

        // Final position should be snapped (integer X)
        float fracPart = transform.Position.X - MathF.Floor(transform.Position.X);
        Assert.That(fracPart, Is.EqualTo(0f).Within(0.01f),
            $"X should be integer (snapped). Actual: {transform.Position.X}");
        Assert.That(transform.Position.Y, Is.EqualTo(5f).Within(0.001f), "Y should be unchanged");
    }
}
