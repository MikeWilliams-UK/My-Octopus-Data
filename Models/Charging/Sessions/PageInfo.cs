using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class PageInfo
{
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    [JsonPropertyName("startCursor")]
    public DateTime StartCursor { get; set; }
}