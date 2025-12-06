using SpaceStation.Shared.Enums;

namespace SpaceStation.Content.Components;

/// <summary>
/// Atmospheric gas mixture component.
/// </summary>
public struct Atmosphere
{
    // Gas amounts in moles
    public float Oxygen;
    public float Nitrogen;
    public float CarbonDioxide;
    public float Plasma;
    public float NitrousOxide;

    /// <summary>
    /// Temperature in Kelvin.
    /// </summary>
    public float Temperature;

    /// <summary>
    /// Volume in liters.
    /// </summary>
    public float Volume;

    public readonly float TotalMoles =>
        Oxygen + Nitrogen + CarbonDioxide + Plasma + NitrousOxide;

    public readonly float Pressure =>
        Volume > 0 ? (TotalMoles * 8.314f * Temperature) / Volume : 0f;

    public static Atmosphere CreateStandard(float volume = 2500f)
    {
        return new Atmosphere
        {
            Oxygen = 21f,
            Nitrogen = 79f,
            CarbonDioxide = 0f,
            Plasma = 0f,
            NitrousOxide = 0f,
            Temperature = 293.15f, // 20°C
            Volume = volume
        };
    }
}

/// <summary>
/// Gas container component for tanks and canisters.
/// </summary>
public record struct GasContainer(
    float MaxPressure = 10000f,
    bool ValveOpen = false,
    float ReleasePressure = 101.325f
);

/// <summary>
/// Marker component for entities exposed to space.
/// </summary>
public struct ExposedToSpace { }

/// <summary>
/// Marker component for sealed areas.
/// </summary>
public struct Sealed { }
