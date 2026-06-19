using System.Text.Json.Serialization;

namespace OctopusData.Models.Cost;

public class Property
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}