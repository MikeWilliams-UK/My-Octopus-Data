using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class Costs
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}