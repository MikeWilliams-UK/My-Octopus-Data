using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

// Root
public class ElectricCosts
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}