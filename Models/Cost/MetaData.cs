using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class MetaData
{
    [JsonPropertyName("statistics")]
    public List<Statistic> Statistics { get; set; } = [];
}