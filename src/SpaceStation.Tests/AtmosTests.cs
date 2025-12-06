using System.Numerics;
using Arch.Core;
using SpaceStation.Content.Components;
using SpaceStation.Content.Systems;

namespace SpaceStation.Tests;

/// <summary>
/// Tests for atmosphere system pressure equalization.
/// </summary>
[TestFixture]
public class AtmosTests
{
    private World _world = null!;
    private AtmosSystem _atmosSystem = null!;

    [SetUp]
    public void Setup()
    {
        _world = World.Create();
        _atmosSystem = new AtmosSystem();
    }

    [TearDown]
    public void Teardown()
    {
        _world.Dispose();
    }

    /// <summary>
    /// Test that Atmosphere.CreateStandard creates correct values.
    /// </summary>
    [Test]
    public void Test_CreateStandard_Atmosphere()
    {
        var atmos = Atmosphere.CreateStandard();

        Assert.That(atmos.Oxygen, Is.EqualTo(21f), "Standard O2 should be 21 moles");
        Assert.That(atmos.Nitrogen, Is.EqualTo(79f), "Standard N2 should be 79 moles");
        Assert.That(atmos.CarbonDioxide, Is.EqualTo(0f), "Standard CO2 should be 0 moles");
        Assert.That(atmos.Plasma, Is.EqualTo(0f), "Standard Plasma should be 0 moles");
        Assert.That(atmos.Temperature, Is.EqualTo(293.15f).Within(0.1f), "Standard temp should be ~20C");
        Assert.That(atmos.TotalMoles, Is.EqualTo(100f), "Total should be 100 moles");
    }

    /// <summary>
    /// Test that IsBreathable correctly identifies breathable atmosphere.
    /// </summary>
    [Test]
    public void Test_IsBreathable()
    {
        var standard = Atmosphere.CreateStandard();
        var vacuum = new Atmosphere { Oxygen = 0, Nitrogen = 0, Volume = 2500f, Temperature = 293.15f };

        Assert.That(AtmosSystem.IsBreathable(in standard), Is.True, "Standard atmos should be breathable");
        Assert.That(AtmosSystem.IsBreathable(in vacuum), Is.False, "Vacuum should not be breathable");
    }

    /// <summary>
    /// Test that isolated tiles don't lose gas.
    /// </summary>
    [Test]
    public void Test_Isolated_Tile_Stability()
    {
        // Arrange: Single tile with atmosphere
        var tile = _world.Create(
            new Transform(new Vector2(0, 0), 0f, 0),
            Atmosphere.CreateStandard()
        );

        var atmos_before = _world.Get<Atmosphere>(tile);
        float moles_before = atmos_before.TotalMoles;

        // Act: Simulate
        for (int i = 0; i < 100; i++)
        {
            _atmosSystem.Update(0.05f, _world);
        }

        // Assert: Gas should remain stable
        var atmos_after = _world.Get<Atmosphere>(tile);
        Assert.That(atmos_after.TotalMoles, Is.EqualTo(moles_before).Within(0.1f),
            "Isolated tile should maintain gas levels");
    }

    /// <summary>
    /// Test pressure equalization between adjacent tiles.
    /// NOTE: Requires gas flow implementation in AtmosSystem.
    /// </summary>
    [Test]
    [Ignore("Gas flow not yet implemented in AtmosSystem")]
    public void Test_Vacuum_Equalization()
    {
        // TODO: Implement after AtmosSystem gas flow is added
        Assert.Pass("Placeholder for gas equalization test");
    }
}
