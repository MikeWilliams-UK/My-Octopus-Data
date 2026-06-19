using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class Account
{
    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; } = [];
}