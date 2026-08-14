using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class MetaData
{
    [JsonPropertyName("statistics")]
    public List<Statistic> Statistics { get; set; } = [];
}