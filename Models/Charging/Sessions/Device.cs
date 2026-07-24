using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Device
{
    [JsonPropertyName("chargingSessions")]
    public ChargingSessions ChargingSessions { get; set; } = new();
}