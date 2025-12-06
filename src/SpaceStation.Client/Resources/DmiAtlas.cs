using System.Text.Json.Serialization;

namespace SpaceStation.Client.Resources;

/// <summary>
/// Represents a parsed DMI JSON atlas file.
/// </summary>
public class DmiAtlas
{
    [JsonPropertyName("meta")]
    public DmiMeta Meta { get; set; } = new();
    
    [JsonPropertyName("states")]
    public Dictionary<string, DmiState> States { get; set; } = new();
}

/// <summary>
/// Metadata from DMI atlas.
/// </summary>
public class DmiMeta
{
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("image")]
    public string? Image { get; set; }
    
    [JsonPropertyName("size")]
    public DmiSize Size { get; set; } = new();
}

/// <summary>
/// Default sprite size.
/// </summary>
public class DmiSize
{
    [JsonPropertyName("w")]
    public int Width { get; set; } = 32;
    
    [JsonPropertyName("h")]
    public int Height { get; set; } = 32;
}

/// <summary>
/// A single icon state within the atlas.
/// </summary>
public class DmiState
{
    [JsonPropertyName("loop")]
    public bool Loop { get; set; } = true;
    
    [JsonPropertyName("directions")]
    public Dictionary<string, List<DmiFrame>>? Directions { get; set; }
}

/// <summary>
/// A single frame of animation.
/// </summary>
public class DmiFrame
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
    
    [JsonPropertyName("w")]
    public int W { get; set; } = 32;
    
    [JsonPropertyName("h")]
    public int H { get; set; } = 32;
    
    [JsonPropertyName("delay_ms")]
    public float DelayMs { get; set; } = 100f;
}
