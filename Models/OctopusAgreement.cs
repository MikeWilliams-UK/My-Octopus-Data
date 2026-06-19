namespace OctopusData.Models;

public class OctopusAgreement
{
    public string TariffCode { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}