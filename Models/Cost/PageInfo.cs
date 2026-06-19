using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class PageInfo
{
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    [JsonPropertyName("startCursor")]
    public string StartCursor { get; set; } = string.Empty;

    [JsonPropertyName("endCursor")]
    public string EndCursor { get; set; } = string.Empty;
}