using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Devices;

public class AllDevices
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}