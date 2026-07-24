using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; } = new();
}