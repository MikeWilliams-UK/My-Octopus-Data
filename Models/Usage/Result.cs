using System.Text.Json.Serialization;

namespace OctopusData.Models.Usage;

public class Result
{
    [JsonPropertyName("consumption")]
    public double Consumption { get; set; }

    [JsonPropertyName("interval_start")]
    public DateTime IntervalStart { get; set; }

    [JsonPropertyName("interval_end")]
    public DateTime IntervalEnd { get; set; }

    public override string ToString()
    {
        return $"{IntervalStart} {IntervalEnd} {Consumption}";
    }
}