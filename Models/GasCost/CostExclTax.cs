using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class CostExclTax
{
    [JsonPropertyName("pricePerUnit")]
    public object PricePerUnit { get; set; } = new();

    [JsonPropertyName("costCurrency")]
    public string CostCurrency { get; set; } = string.Empty;

    [JsonPropertyName("estimatedAmount")]
    public string EstimatedAmount { get; set; } = string.Empty;
}