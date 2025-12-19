namespace OctopusData.Models;

public class OctopusMeterPoint
{
    public string Mpxn { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public int ProfileClass { get; set; }
    public int ConsumptionStandard { get; set; }
}