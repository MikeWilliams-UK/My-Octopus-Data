using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class Register
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public string Rate { get; set; } = string.Empty;

    [JsonPropertyName("is_settlement_register")]
    public bool IsSettlementRegister { get; set; }
}