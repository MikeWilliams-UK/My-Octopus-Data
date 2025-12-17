using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class GasMeterPoint
{
    [JsonPropertyName("mprn")]
    public string Mprn { get; set; } = string.Empty;

    [JsonPropertyName("consumption_standard")]
    public int ConsumptionStandard { get; set; }

    [JsonPropertyName("meters")]
    public List<Meter> Meters { get; set; } = [];

    [JsonPropertyName("agreements")]
    public List<Agreement> Agreements { get; set; } = [];
}