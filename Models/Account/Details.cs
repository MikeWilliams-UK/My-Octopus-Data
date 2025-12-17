using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class Details
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; } = [];
}