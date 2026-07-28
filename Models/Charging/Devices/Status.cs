using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Status
{
    [JsonPropertyName("current")]
    public string Current { get; set; } = string.Empty;
}