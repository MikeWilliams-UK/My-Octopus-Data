using System.Text.Json.Serialization;

namespace OctopusData.Models.Account;

public class Property
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("moved_in_at")]
    public DateTime MovedInAt { get; set; }

    [JsonPropertyName("moved_out_at")]
    public DateTime? MovedOutAt { get; set; }

    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; } = string.Empty;

    [JsonPropertyName("address_line_3")]
    public string AddressLine3 { get; set; } = string.Empty;

    [JsonPropertyName("town")]
    public string Town { get; set; } = string.Empty;

    [JsonPropertyName("county")]
    public string County { get; set; } = string.Empty;

    [JsonPropertyName("postcode")]
    public string Postcode { get; set; } = string.Empty;

    [JsonPropertyName("electricity_meter_points")]
    public List<ElectricityMeterPoint> ElectricityMeterPoints { get; set; } = [];

    [JsonPropertyName("gas_meter_points")]
    public List<GasMeterPoint> GasMeterPoints { get; set; } = [];
}