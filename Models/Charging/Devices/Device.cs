using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Device
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("publicSession")]
    public PublicSession PublicSession { get; set; }

    [JsonPropertyName("boostSession")]
    public BoostSession BoostSession { get; set; }

    [JsonPropertyName("smartSession")]
    public SmartSession SmartSession { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }
}