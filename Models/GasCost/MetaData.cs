using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class MetaData
{
    [JsonPropertyName("statistics")]
    public List<Statistic> Statistics { get; set; } = [];
}