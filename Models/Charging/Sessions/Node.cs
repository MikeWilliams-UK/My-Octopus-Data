using System.Text.Json.Serialization;

namespace OctopusData.Models.Charging.Sessions;

public class Node
{
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime End { get; set; }

    [JsonPropertyName("energyAdded")]
    public EnergyAdded EnergyAdded { get; set; } = new();

    [JsonPropertyName("cost")]
    public Cost Cost { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("problems")]
    public List<Problem> Problems { get; set; } = new();

    [JsonPropertyName("stateOfChargeFinal")]
    public string StateOfChargeFinal { get; set; } = string.Empty;

    [JsonPropertyName("stateOfChargeChange")]
    public string StateOfChargeChange { get; set; } = string.Empty;
}