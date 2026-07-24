using System.Text.Json.Serialization;

namespace OctopusData.Models;

public class KrakenResponse
{
    [JsonPropertyName("data")]
    public KrakenData Data { get; set; } = new();
}

public class KrakenData
{
    [JsonPropertyName("obtainKrakenToken")]
    public ObtainKrakenToken ObtainKrakenToken { get; set; } = new();
}

public class ObtainKrakenToken
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public Payload Payload { get; set; } = new();

    [JsonPropertyName("refreshExpiresIn")]
    public long RefreshExpiresIn { get; set; }
}

public class Payload
{
    [JsonPropertyName("refreshExpiresIn")]
    public string Sub { get; set; } = string.Empty;

    [JsonPropertyName("qty")]
    public string Qty { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("tokenUse")]
    public string TokenUse { get; set; } = string.Empty;

    [JsonPropertyName("iss")]
    public string Iss { get; set; } = string.Empty;

    [JsonPropertyName("iat")]
    public long Iat { get; set; }

    [JsonPropertyName("exp")]
    public long Exp { get; set; }

    [JsonPropertyName("origIat")]
    public long OrigIat { get; set; }
}