using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Agreement
{
    [JsonPropertyName("tariff")]
    public Tariff Tariff { get; set; } = new();

    [JsonPropertyName("validFrom")]
    public DateTime ValidFrom { get; set; }

    [JsonPropertyName("validTo")]
    public DateTime ValidTo { get; set; }
}