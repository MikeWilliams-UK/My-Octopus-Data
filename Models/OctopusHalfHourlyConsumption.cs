namespace OctopusData.Models;

public class OctopusHalfHourlyConsumption
{
    public OctopusInterval Interval { get; set; } = new();

    public double Consumption { get; set; }
}