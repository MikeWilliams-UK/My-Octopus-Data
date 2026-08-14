using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Data
{
    [JsonPropertyName("account")]
    public Account Account { get; set; } = new();
}