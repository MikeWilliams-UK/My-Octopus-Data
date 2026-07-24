using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Cost
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}