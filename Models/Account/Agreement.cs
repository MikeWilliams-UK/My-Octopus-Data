using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class Agreement
{
    [JsonPropertyName("tariff_code")]
    public string TariffCode { get; set; } = string.Empty;

    [JsonPropertyName("valid_from")]
    public DateTime ValidFrom { get; set; }

    [JsonPropertyName("valid_to")]
    public DateTime ValidTo { get; set; }
}