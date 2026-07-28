using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class ChargeHistory
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}