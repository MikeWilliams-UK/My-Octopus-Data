using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Measurements
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; } = [];
}