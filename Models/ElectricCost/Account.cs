using System.Text.Json.Serialization;

namespace OctopusData.Models.ElectricCost;

public class Account
{
    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; } = [];
}