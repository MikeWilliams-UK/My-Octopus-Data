using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Statistic
{
    [JsonPropertyName("costExclTax")]
    public CostExclTax CostExclTax { get; set; } = new();

    [JsonPropertyName("costInclTax")]
    public CostInclTax CostInclTax { get; set; } = new();

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;


    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}