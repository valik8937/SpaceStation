namespace SpaceStation.Shared.Constants;

/// <summary>
/// Atmospheric simulation constants.
/// </summary>
public static class AtmosConstants
{
    // Gas constants
    public const float OneAtmosphere = 101.325f; // kPa
    public const float StandardTemp = 293.15f;   // 20°C in Kelvin
    public const float T0C = 273.15f;            // 0°C in Kelvin
    public const float T20C = 293.15f;           // 20°C in Kelvin
    
    // Pressure limits
    public const float HazardLowPressure = 20f;
    public const float WarningLowPressure = 50f;
    public const float WarningHighPressure = 550f;
    public const float HazardHighPressure = 750f;
    
    // Temperature limits
    public const float MinTemp = 2.7f;           // Cosmic background
    public const float MaxTemp = 100000f;
    public const float ColdDamageThreshold = 260f;
    public const float HeatDamageThreshold = 360f;
    
    // Oxygen levels (percentage)
    public const float MinBreathableOxygen = 16f;
    public const float OptimalOxygen = 21f;
    public const float ToxicOxygenThreshold = 140f;
    
    // Simulation
    public const float AtmosTickRate = 0.5f;     // seconds
    public const int GroupSize = 8;              // tiles per gas group
    public const float MinTransferMoles = 0.01f;
}
