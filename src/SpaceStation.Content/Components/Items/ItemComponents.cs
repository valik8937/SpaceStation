using SpaceStation.Shared.Enums;

namespace SpaceStation.Content.Components;

/// <summary>
/// Item size categories.
/// </summary>
public enum ItemSize
{
    Tiny,
    Small,
    Normal,
    Large,
    Huge,
    Gigantic
}

/// <summary>
/// Clothing slot types.
/// </summary>
public enum ClothingSlot
{
    None,
    Head,
    Mask,
    Neck,
    Eyes,
    Ears,
    Uniform,
    Suit,
    Gloves,
    Shoes,
    Back,
    Belt,
    Pocket,
    Id
}

/// <summary>
/// Tool quality types.
/// </summary>
public enum ToolQuality
{
    Cutting,
    Screwing,
    Prying,
    Pulsing,
    Welding,
    Anchoring,
    Digging,
    Mining,
    Sawing
}

/// <summary>
/// Item component for pickable objects.
/// </summary>
public record struct Item(
    string Name = "item",
    float Weight = 1f,
    ItemSize Size = ItemSize.Small,
    bool InInventory = false
);

/// <summary>
/// Tool component with qualities.
/// </summary>
public struct Tool
{
    public ToolQuality Qualities; // Using flags would be better
    public float SpeedModifier;

    public Tool()
    {
        Qualities = 0;
        SpeedModifier = 1f;
    }
}

/// <summary>
/// Weapon component for combat.
/// </summary>
public record struct Weapon(
    float Damage = 5f,
    DamageType DamageType = DamageType.Brute,
    float AttackCooldown = 0.5f,
    float ArmorPenetration = 0f,
    bool IsRanged = false
);

/// <summary>
/// Clothing component for wearable items.
/// </summary>
public record struct Clothing(
    ClothingSlot Slot = ClothingSlot.None,
    float BruteArmor = 0f,
    float BurnArmor = 0f,
    float ColdProtection = 0f,
    float HeatProtection = 0f
);
