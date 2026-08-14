using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class ElectricityMeterPoint
{
    [JsonPropertyName("agreements")]
    public List<Agreement> Agreements { get; set; } = [];
}