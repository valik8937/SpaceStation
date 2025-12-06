using Arch.Core;
using Arch.Core.Extensions;
using SpaceStation.Core.Systems;
using SpaceStation.Content.Components;

namespace SpaceStation.Content.Systems;

/// <summary>
/// System managing power distribution.
/// Similar to SS13's SSpower subsystem.
/// </summary>
public sealed class PowerSystem : BaseSystem
{
    public override int Priority => 25;

    private static readonly QueryDescription ProducerQuery = new QueryDescription()
        .WithAll<PowerProducer>();

    private static readonly QueryDescription ConsumerQuery = new QueryDescription()
        .WithAll<PowerConsumer>();

    private static readonly QueryDescription BatteryQuery = new QueryDescription()
        .WithAll<Battery>();

    public override void Update(float deltaTime, World world)
    {
        // Calculate total power supply
        float totalSupply = 0f;
        world.Query(in ProducerQuery, (ref PowerProducer producer) =>
        {
            if (producer.Active)
            {
                totalSupply += producer.CurrentOutput;
            }
        });

        // Calculate total demand
        float totalDemand = 0f;
        world.Query(in ConsumerQuery, (ref PowerConsumer consumer) =>
        {
            if (consumer.Enabled)
            {
                totalDemand += consumer.PowerDraw;
            }
        });

        // Distribute power to consumers
        bool powered = totalSupply >= totalDemand;
        world.Query(in ConsumerQuery, (ref PowerConsumer consumer) =>
        {
            consumer.Powered = consumer.Enabled && powered;
        });

        // Update batteries
        float surplus = totalSupply - totalDemand;
        world.Query(in BatteryQuery, (ref Battery battery) =>
        {
            if (surplus > 0)
            {
                // Charge batteries with surplus
                var chargeAmount = MathF.Min(surplus, battery.MaxChargeRate * deltaTime);
                battery.CurrentCharge = MathF.Min(battery.MaxCharge, battery.CurrentCharge + chargeAmount);
            }
            else if (surplus < 0)
            {
                // Drain batteries to cover deficit
                var drainAmount = MathF.Min(-surplus, battery.MaxDischargeRate * deltaTime);
                battery.CurrentCharge = MathF.Max(0, battery.CurrentCharge - drainAmount);
            }
        });
    }
}
