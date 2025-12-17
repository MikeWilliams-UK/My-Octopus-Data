using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class Meter
{
    [JsonPropertyName("serial_number")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("registers")]
    public List<Register> Registers { get; set; } = [];
}