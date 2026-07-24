using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Data
{
    [JsonPropertyName("devices")]
    public List<Device> Devices { get; set; } = new();
}