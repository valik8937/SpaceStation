using Arch.Core;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Components;
using SpaceStation.Shared.Constants;

namespace SpaceStation.Content.Systems;

/// <summary>
/// System simulating atmospheric gas flow and reactions.
/// Similar to SS13's SSair subsystem.
/// </summary>
public sealed class AtmosSystem : BaseSystem
{
    public override int Priority => 30;

    private float _tickAccumulator = 0f;

    public override void Update(float deltaTime, World world)
    {
        _tickAccumulator += deltaTime;

        // Run atmos at its own tick rate
        if (_tickAccumulator < AtmosConstants.AtmosTickRate)
            return;

        _tickAccumulator -= AtmosConstants.AtmosTickRate;

        ProcessAtmosphere(world);
    }

    private void ProcessAtmosphere(World world)
    {
        // TODO: Implement gas flow simulation
        // This would include:
        // - Gas equalization between adjacent tiles
        // - Space exposure (venting)
        // - Temperature equalization
        // - Gas reactions (plasma fire, etc.)
    }

    /// <summary>
    /// Checks if an atmosphere is breathable.
    /// </summary>
    public static bool IsBreathable(in Atmosphere atmos)
    {
        var totalMoles = atmos.TotalMoles;
        if (totalMoles <= 0) return false;

        var oxygenPercent = (atmos.Oxygen / totalMoles) * 100f;
        var pressure = atmos.Pressure;

        return oxygenPercent >= AtmosConstants.MinBreathableOxygen &&
               oxygenPercent <= AtmosConstants.ToxicOxygenThreshold &&
               pressure >= AtmosConstants.HazardLowPressure &&
               pressure <= AtmosConstants.HazardHighPressure;
    }
}
