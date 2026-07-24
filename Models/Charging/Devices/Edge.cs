using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Edge
{
    [JsonPropertyName("cursor")]
    public DateTime Cursor { get; set; }
}