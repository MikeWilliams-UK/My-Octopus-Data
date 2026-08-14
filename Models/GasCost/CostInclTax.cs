using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class CostInclTax
{
    [JsonPropertyName("costCurrency")]
    public string CostCurrency { get; set; } = string.Empty;

    [JsonPropertyName("estimatedAmount")]
    public string EstimatedAmount { get; set; } = string.Empty;
}