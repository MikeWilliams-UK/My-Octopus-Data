using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Property
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("electricityMeterPoints")]
    public List<ElectricityMeterPoint> ElectricityMeterPoints { get; set; } = [];

    [JsonPropertyName("measurements")]
    public Measurements Measurements { get; set; } = new();
}