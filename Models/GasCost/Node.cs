using System.Text.Json.Serialization;

namespace OctopusData.Models.GasCost;

public class Node
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("startAt")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("durationInSeconds")]
    public int DurationInSeconds { get; set; }

    [JsonPropertyName("metaData")]
    public MetaData MetaData { get; set; } = new();
}