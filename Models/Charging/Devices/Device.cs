using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Device
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("publicSession")]
    public PublicSession PublicSession { get; set; } = new();

    [JsonPropertyName("boostSession")]
    public BoostSession BoostSession { get; set; } = new();

    [JsonPropertyName("smartSession")]
    public SmartSession SmartSession { get; set; } = new();

    [JsonPropertyName("status")]
    public Status Status { get; set; } = new();
}