using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class ElectricityMeterPoint
{
    [JsonPropertyName("mpan")]
    public string Mpan { get; set; } = string.Empty;

    [JsonPropertyName("profile_class")]
    public int ProfileClass { get; set; }

    [JsonPropertyName("consumption_standard")]
    public int ConsumptionStandard { get; set; }

    [JsonPropertyName("meters")]
    public List<Meter> Meters { get; set; } = [];

    [JsonPropertyName("agreements")]
    public List<Agreement> Agreements { get; set; } = [];

    [JsonPropertyName("is_export")]
    public bool IsExport { get; set; }
}