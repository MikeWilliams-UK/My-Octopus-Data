using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class ChargingSessions
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; } = new();

    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; } = new();
}