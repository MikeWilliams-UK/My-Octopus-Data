using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class PublicSession
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}