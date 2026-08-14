using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class Data
{
    [JsonPropertyName("account")]
    public Account Account { get; set; } = new();
}