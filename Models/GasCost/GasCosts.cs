using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

// Root
public class GasCosts
{
    [JsonPropertyName("data")]
    public Data Data { get; set; } = new();
}