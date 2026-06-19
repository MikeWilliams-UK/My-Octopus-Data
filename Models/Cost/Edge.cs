using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace OctopusData.Models.Cost;

public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; } = new();
}