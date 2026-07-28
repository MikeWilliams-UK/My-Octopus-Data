using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class Data
{
    [JsonPropertyName("devices")]
    public List<Device> Devices { get; set; } = [];
}