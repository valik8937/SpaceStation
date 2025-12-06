namespace SpaceStation.Content.Components;

/// <summary>
/// Power distribution channels.
/// </summary>
public enum PowerChannel
{
    Equipment,
    Lighting,
    Environment
}

/// <summary>
/// Power consumer component.
/// </summary>
public record struct PowerConsumer(
    float PowerDraw = 100f,
    PowerChannel Channel = PowerChannel.Equipment,
    bool Powered = false,
    bool Enabled = true
);

/// <summary>
/// Power producer component.
/// </summary>
public record struct PowerProducer(
    float MaxOutput = 1000f,
    float CurrentOutput = 0f,
    bool Active = false
);

/// <summary>
/// Battery component for power storage.
/// </summary>
public record struct Battery(
    float MaxCharge = 10000f,
    float CurrentCharge = 10000f,
    float MaxChargeRate = 500f,
    float MaxDischargeRate = 1000f
)
{
    public readonly float ChargePercent => MaxCharge > 0 ? CurrentCharge / MaxCharge : 0f;
}
