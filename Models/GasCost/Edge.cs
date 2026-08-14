using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; } = new();
}