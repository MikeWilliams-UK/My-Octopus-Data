using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class Data
{
    [JsonPropertyName("account")]
    public Account Account { get; set; } = new();
}