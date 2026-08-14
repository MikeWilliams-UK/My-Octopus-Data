using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class PricePerUnit
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}