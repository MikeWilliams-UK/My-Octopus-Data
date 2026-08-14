using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Measurements
{
    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; } = [];

    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; } = new();
}