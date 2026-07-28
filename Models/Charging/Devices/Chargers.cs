using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Chargers
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}