using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Problem
{
    [JsonPropertyName("cause")]
    public string Cause { get; set; } = string.Empty;

    [JsonPropertyName("truncationCause")]
    public string TruncationCause { get; set; } = string.Empty;
}