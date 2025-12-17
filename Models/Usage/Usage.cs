using System.Text.Json.Serialization;

namespace OctopusData.Models.Usage;

public class Usage
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public object Next { get; set; }

    [JsonPropertyName("previous")]
    public object Previous { get; set; }

    [JsonPropertyName("results")]
    public List<Result> Results { get; set; } = [];
}