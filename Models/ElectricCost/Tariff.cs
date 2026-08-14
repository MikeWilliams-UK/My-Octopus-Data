using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Tariff
{
    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; } = string.Empty;
}