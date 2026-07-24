using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class EnergyAdded
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}