using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class ChargeHistrory
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}