using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class BoostSession
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; } = new();
}